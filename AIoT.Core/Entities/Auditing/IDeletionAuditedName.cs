
namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识删除审记
    /// </summary>
    public interface IDeletionAuditedName 
    {
        /// <summary>
        /// 删除者
        /// </summary>
        string DeleterUserName { get; set; }
    }
}
