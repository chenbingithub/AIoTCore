using Microsoft.AspNetCore.Mvc;
using System.IO;

// ReSharper disable once CheckNamespace
namespace AIoT.Core.Web
{
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// 生成下载文件
        /// </summary>
        public static FileContentResult DownloadFile(this ControllerBase controller, byte[] fileContents, string fileDownloadName)
        {
            controller.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return controller.File(fileContents, "application/octet-stream", fileDownloadName);
        }

        /// <summary>
        /// 生成下载文件(文件流)
        /// </summary>
        public static FileStreamResult DownloadFile(this ControllerBase controller, Stream stream,string contentType, string fileDownloadName)
        {
            controller.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
            return controller.File(stream, contentType, fileDownloadName);
        }
    }
}
