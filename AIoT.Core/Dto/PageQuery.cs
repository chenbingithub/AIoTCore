using System;
using System.Linq;
using System.Linq.Expressions;
using AIoT.Core.Extensions;
using AIoT.Core.Repository;


namespace AIoT.Core.Dto
{
    /// <summary>
    /// ��ҳ��ѯ
    /// </summary>
    public class PageQuery : PageSortInfo, IPageQuery
    {
        /// <summary>
        /// ָ����ѯ����
        /// </summary>
        protected LambdaExpression Filter { get; set; }

        
        /// <summary>
        /// ����
        /// </summary>
        public virtual void And<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
           
            Filter = (Filter as Expression<Func<TEntity, bool>>).AndNull(filter);
        }
        
        /// <summary>
        /// ��ȡ��ѯ����
        /// </summary>
        /// <typeparam name="TEntity">Ҫ��ѯ��ʵ������</typeparam>
        public Expression<Func<TEntity, bool>> GetFilter<TEntity>() where TEntity : class
        {
            
            return (Filter as Expression<Func<TEntity, bool>>).AndNull(this.GetQueryExpression<TEntity>());
        }
    }
}