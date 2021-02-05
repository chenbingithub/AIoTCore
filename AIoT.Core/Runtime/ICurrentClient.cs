namespace AIoT.Core.Runtime
{
    public interface ICurrentClient
    {/// <summary>
        /// 获取客户端浏览器信息
        /// </summary>
        string BrowserInfo { get; }

        /// <summary>
        /// 获取客户端Ip地址
        /// </summary>
        string ClientIpAddress { get; }

    }
}