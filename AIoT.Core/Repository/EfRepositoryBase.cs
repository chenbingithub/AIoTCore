using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIoT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AIoT.Core.Repository
{
    /// <summary>
    /// Ef 仓储基类
    /// </summary>
    public abstract class EfRepositoryBase : IRepository
    {
        private static readonly Regex _formatRegex = new Regex(@"\{(?<Num>\d+)\}", RegexOptions.Compiled);

        /// <summary>
        /// 解析 <see cref="FormattableString"/>
        /// </summary>
        protected static (string sql, object param) UnwarpFormattableSql(FormattableString formatSql)
        {
            var sql = _formatRegex.Replace(formatSql.Format, p => $"@p{p.Groups["Num"].Value}");
            var args = formatSql.GetArguments().Select((p, i) => new KeyValuePair<string, object>($"@p{i}", p));
            return (sql, args);
        }

        /// <summary>
        /// 执行 ADO.NET 命令
        /// </summary>
        protected static async Task<T> ExecuteAsync<T>(DbContext context, Func<IDbConnection, IDbTransaction, Task<T>> invoke)
        {
            var con = context.Database.GetDbConnection();
            var tran = context.Database.CurrentTransaction?.GetDbTransaction();

            //var profiledCon = new ProfiledDbConnection(con, MiniProfiler.Current);
            //var profiledTran = tran != null ? new ProfiledDbTransaction(tran, profiledCon) : null;
            
            return await invoke(con, tran);
        }

        /// <summary>
        /// 生成匹配指定主键的表达式
        /// </summary>
        protected static Expression<Func<TEntity, bool>> CreateEqualityExpressionForKeys<TEntity, TKey>(TKey id)
        {
            var lambdaParam = Expression.Parameter(typeof(TEntity));

            var lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, nameof(IEntity<TKey>.Id)),
                Expression.Constant(id, typeof(TKey))
            );

            return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
        }

        /// <summary>
        /// 生成匹配指定主键的表达式
        /// </summary>
        protected static Expression<Func<TEntity, bool>> CreateEqualityExpressionForKeys<TEntity>(DbContext context, params object[] keyValues)
            where TEntity : class, IEntity
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new Exception($"the type {typeof(TEntity).FullName} not map to {context.GetType().FullName}");
            }

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null)
            {
                throw new Exception($"the type {typeof(TEntity).FullName} not have primary key");
            }

            Expression<Func<TEntity, bool>> expression = null;
            var list = entityType.FindPrimaryKey().Properties;
            for (var i = 0; i < list.Count; i++)
            {
                var property = list[i];
                var value = keyValues[i];
                var lambdaParam = Expression.Parameter(typeof(TEntity));

                var lambdaBody = Expression.Equal(
                    Expression.PropertyOrField(lambdaParam, property.Name),
                    Expression.Constant(value, property.ClrType)
                );

                var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
                expression = expression.And(lambdaExpression);
            }

            return expression;
        }
    }
}
