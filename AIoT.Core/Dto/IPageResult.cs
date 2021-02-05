namespace AIoT.Core.Dto
{
    /// <summary>
    /// 分页结果
    /// </summary>
    public interface IPageResult : IPageInfo
    {
        /// <summary>
        /// 总数
        /// </summary>
        int TotalCount { get; set; }
    }
}
