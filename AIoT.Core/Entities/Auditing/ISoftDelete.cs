namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识是否软删除
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// 标记是否已删除
        /// </summary>
        bool IsDeleted { get; set; }
    }
}