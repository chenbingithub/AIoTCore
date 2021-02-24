using System;
using System.Collections.Generic;
using System.Text;

namespace AIoT.Core.Mqtt
{
   public class MqttOptions
    {
        public  string HostIp { get; set; }
        public  int HostPort { get; set; }
        public  string UserName { get; set; }
        public  string Password { get; set; }
        public  string ClientId{ get; set; }
        public  bool EnableSsl { get; set; }
        /// <summary>
        /// 订阅主题前缀$"{ProjectID}/{PeojectSecret}/{GWID}"
        /// </summary>
        public string TopicPrefix { get; set; }
    }
}
