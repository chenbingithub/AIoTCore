namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识创建审计
    /// </summary>
    public interface ICreationAudited<TUserId> : IHasCreationTime
    {
        /// <summary>
        /// 创建者
        /// </summary>
        TUserId CreatorUserId { get; set; }
    }
}
