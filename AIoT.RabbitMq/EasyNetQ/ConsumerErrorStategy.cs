using System;
using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using EasyNetQComponent = EasyNetQ;

namespace AIoT.RabbitMq.EasyNetQ
{
    /// <inheritdoc />
    public class ConsumerErrorStategy : IConsumerErrorStrategy
    {
        private readonly ILogger _logger;
        private readonly EasyNetQComponent.IConnectionFactory connectionFactory;
        private readonly ITypeNameSerializer typeNameSerializer;
        private readonly object syncLock = new object();

        private IConnection connection;

        /// <inheritdoc />
        public ConsumerErrorStategy(
            ILoggerFactory loggerFactory,
            EasyNetQComponent.IConnectionFactory connectionFactory,
            ITypeNameSerializer typeNameSerializer
            )
        {
            _logger = loggerFactory.CreateLogger(GetType().FullName);

            this.connectionFactory = connectionFactory;
            this.typeNameSerializer = typeNameSerializer;
        }

        /// <summary>
        /// 连接RabbitMQ客户端
        /// </summary>
        protected void Connect()
        {
            if (connection == null || !connection.IsOpen)
            {
                lock (syncLock)
                {
                    if ((connection == null || !connection.IsOpen) && !(disposing || disposed))
                    {
                        if (connection != null)
                        {
                            try
                            {
                                connection.Dispose();
                            }
                            catch
                            {
                                if (connection.CloseReason != null)
                                {
                                    _logger.LogInformation("Connection {connection} has shutdown with reason={reason}", connection.ToString(), connection.CloseReason.Cause);
                                }
                                else
                                {
                                    throw;
                                }
                            }

                        }

                        connection = connectionFactory.CreateConnection();
                        if (disposing || disposed)
                        {
                            connection.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 声明失败队列
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private string DeclareFailedExchangeWithQueue(IModel model, ConsumerExecutionContext context)
        {
            var failedExchangeName = $"Failed_{context.Info.Exchange}";
            var failedQueueName = $"Failed_{context.Info.Queue}";
            var routingKey = string.IsNullOrEmpty(context.Info.RoutingKey) ? context.Info.Queue.Substring(0, context.Info.Exchange.Length + 1) : context.Info.RoutingKey;
            DeclareAndBindExchangeWithQueue(model, failedExchangeName, failedQueueName, routingKey);

            return failedExchangeName;
        }

        /// <summary>
        /// 声明重试队列
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private string DeclareReTryExchangeWithQueue(IModel model, ConsumerExecutionContext context)
        {
            var reTryExchangeName = $"ReTry_{context.Info.Exchange}";
            var reTryQueueName = $"ReTry_{context.Info.Queue}";
            var routingKey = string.IsNullOrEmpty(context.Info.RoutingKey) ? context.Info.Queue.Substring(context.Info.Exchange.Length + 1) : context.Info.RoutingKey;
            DeclareAndBindExchangeWithQueue(model, reTryExchangeName, reTryQueueName, routingKey);
            return reTryExchangeName;
        }

        /// <summary>
        /// 声明队列并绑定
        /// </summary>
        /// <param name="model"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        private void DeclareAndBindExchangeWithQueue(IModel model, string exchangeName, string queueName, string routingKey)
        {
            model.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            model.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
            model.QueueBind(queueName, exchangeName, routingKey);
        }

        /// <summary>
        /// 消费异常处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            if (disposed || disposing)
            {
                _logger.LogError(exception,
                    "ErrorStrategy was already disposed, when attempting to handle consumer error. Error message will not be published and message with receivedInfo={receivedInfo} will be requeued",
                    context.Info
                );

                return AckStrategies.NackWithRequeue;
            }

            try
            {
                Connect();
                _logger.LogError(exception, $"消费类型为{context.Properties.Type ?? "UnKown"}的消息时出现异常 {exception.ToString()}");
                using (var model = connection.CreateModel())
                {
                    try
                    {
                        var temp = context.Properties.Headers["ReTryCount"];
                        if ((int)temp >= 5)
                        {
                            //进入失败队列，不重试
                            var failedExchange = DeclareFailedExchangeWithQueue(model, context);
                            var failedProperties = model.CreateBasicProperties();
                            failedProperties.Persistent = true;
                            failedProperties.Type = context.Properties.Type == null ? typeNameSerializer.Serialize(typeof(object)) : context.Properties.Type;
                            var failedRoutingKey = string.IsNullOrEmpty(context.Info.RoutingKey) ? context.Info.Queue.Substring(context.Info.Exchange.Length + 1) : context.Info.RoutingKey;
                            model.BasicPublish(failedExchange, failedRoutingKey, failedProperties, context.Body);
                            return AckStrategies.Ack;

                        }
                        else
                        {
                            context.Properties.Headers["ReTryCount"] = (int)context.Properties.Headers["ReTryCount"] + 1;
                        }
                    }
                    catch
                    {
                        context.Properties.Headers.Add("ReTryCount", 1);
                    }
                    //canal同步不需要进入重试队列，直接重试即可
                    if (context.Info.Exchange.Contains("ChangeLogModel"))
                    {
                        return AckStrategies.NackWithRequeue;
                    }
                    //进入死信队列，进行重试
                    var errorExchange = DeclareReTryExchangeWithQueue(model, context);
                    var messageBody = context.Body; //CreateErrorMessage(context, exception);
                    var properties = model.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Type = context.Properties.Type == null ? typeNameSerializer.Serialize(typeof(object)) : context.Properties.Type;
                    properties.Headers = context.Properties.Headers;
                    var errorRoutingKey = string.IsNullOrEmpty(context.Info.RoutingKey) ? context.Info.Queue.Substring(context.Info.Exchange.Length + 1) : context.Info.RoutingKey;
                    model.BasicPublish(errorExchange, errorRoutingKey, properties, messageBody);
                    return AckStrategies.Ack;
                }
            }
            catch (BrokerUnreachableException unreachableException)
            {
                _logger.LogError(unreachableException, $"Cannot connect to broker while attempting to publish error message {unreachableException.ToString()}");
            }
            catch (OperationInterruptedException interruptedException)
            {
                _logger.LogError(interruptedException, $"Broker connection was closed while attempting to publish error message {interruptedException.ToString()}");
            }
            catch (Exception unexpectedException)
            {
                _logger.LogError(unexpectedException, $"Failed to publish error message {unexpectedException.ToString()}");
            }
            return AckStrategies.NackWithRequeue;
        }

        /// <inheritdoc />
        public AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return AckStrategies.NackWithRequeue;
        }



        private bool disposed;
        private bool disposing;

        /// <summary>
        /// 回收释放
        /// </summary>
        public virtual void Dispose()
        {
            if (disposed) return;
            disposing = true;

            if (connection != null) { connection.Dispose(); }

            disposed = true;
        }
    }
}
