namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识修改审计
    /// </summary>
    public interface IModificationAudited<TUserId> : IHasModificationTime
    {
        /// <summary>
        /// 最后修改者
        /// </summary>
        TUserId LastModifierUserId { get; set; }
    }
}
