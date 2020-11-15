namespace AIoT.Core.Runtime
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    public interface IClientInfo
    {
        /// <summary>
        /// 获取客户端浏览器信息
        /// </summary>
        string BrowserInfo { get; }

        /// <summary>
        /// 获取客户端Ip地址
        /// </summary>
        string ClientIpAddress { get; }
    }
}