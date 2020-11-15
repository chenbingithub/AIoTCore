
namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识创建审计
    /// </summary>
    public interface ICreationAuditedName 
    {
        /// <summary>
        /// 创建者
        /// </summary>
        string CreatorUserName { get; set; }
    }
}
