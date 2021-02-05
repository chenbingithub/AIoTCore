namespace AIoT.Core.Dto
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public interface IPageInfo
    {
        /// <summary>
        /// 页号，从 1 开始
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        /// 分页大小
        /// </summary>
        int PageSize { get; set; }
    }
}
