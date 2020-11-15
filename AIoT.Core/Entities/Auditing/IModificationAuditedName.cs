namespace AIoT.Core.Entities.Auditing
{
    /// <summary>
    /// 标识修改审计
    /// </summary>
    public interface IModificationAuditedName
    {
        /// <summary>
        /// 最后修改者
        /// </summary>
        string LastModifierUserName{ get; set; }
    }
}
