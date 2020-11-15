namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识是否启用
    /// </summary>
    public interface IPassivable
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsActive { get; }
    }
}