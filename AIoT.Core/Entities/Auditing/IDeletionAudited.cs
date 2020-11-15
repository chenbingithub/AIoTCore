namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识删除审记
    /// </summary>
    public interface IDeletionAudited<TUserId> : IHasDeletionTime
    {
        /// <summary>
        /// 删除者
        /// </summary>
        TUserId DeleterUserId { get; set; }
    }
}
