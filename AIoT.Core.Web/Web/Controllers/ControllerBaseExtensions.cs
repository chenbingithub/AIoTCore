using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Web;

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

        public static IActionResult DownloadBigFile(this ControllerBase controller, byte[] bytes, string fileName)
        {


            int bufferSize = 1024;//这就是ASP.NET Core循环读取下载文件的缓存大小，这里我们设置为了1024字节，也就是说ASP.NET Core每次会从下载文件中读取1024字节的内容到服务器内存中，然后发送到客户端浏览器，这样避免了一次将整个下载文件都加载到服务器内存中，导致服务器崩溃

            controller.Response.ContentType = "application/octet-stream";//由于我们下载的是一个Excel文件，所以设置ContentType为application/vnd.ms-excel
                                                                         //在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，
                                                                         //注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名
            var contentDisposition = "attachment;" + "filename=" + HttpUtility.UrlEncode(fileName);
            controller.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");


            controller.Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            //使用FileStream开始循环读取要下载文件的内容
            using (MemoryStream fs = new MemoryStream(bytes))
            {
                using (controller.Response.Body)//调用Response.Body.Dispose()并不会关闭客户端浏览器到ASP.NET Core服务器的连接，之后还可以继续往Response.Body中写入数据
                {
                    long contentLength = fs.Length;//获取下载文件的大小
                    controller.Response.ContentLength = contentLength;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

                    byte[] buffer;
                    long hasRead = 0;//变量hasRead用于记录已经发送了多少字节的数据到客户端浏览器

                    //如果hasRead小于contentLength，说明下载文件还没读取完毕，继续循环读取下载文件的内容，并发送到客户端浏览器
                    while (hasRead < contentLength)
                    {
                        //HttpContext.RequestAborted.IsCancellationRequested可用于检测客户端浏览器和ASP.NET Core服务器之间的连接状态，如果HttpContext.RequestAborted.IsCancellationRequested返回true，说明客户端浏览器中断了连接
                        if (controller.HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            //如果客户端浏览器中断了到ASP.NET Core服务器的连接，这里应该立刻break，取消下载文件的读取和发送，避免服务器耗费资源
                            break;
                        }

                        buffer = new byte[bufferSize];

                        int currentRead = fs.Read(buffer, 0, bufferSize);//从下载文件中读取bufferSize(1024字节)大小的内容到服务器内存中

                        controller.Response.Body.Write(buffer, 0, currentRead);//发送读取的内容数据到客户端浏览器
                        controller.Response.Body.Flush();//注意每次Write后，要及时调用Flush方法，及时释放服务器内存空间

                        hasRead += currentRead;//更新已经发送到客户端浏览器的字节数
                    }
                }
            }

            return new EmptyResult();
        }
        /// <summary>
        /// DownloadBigFile用于下载大文件，循环读取大文件的内容到服务器内存，然后发送给客户端浏览器
        /// </summary>
        public static IActionResult DownloadBigFile(this ControllerBase controller, string filePath)
        {
            //var filePath = @"D:\Download\测试文档.xlsx";//要下载的文件地址，这个文件会被分成片段，通过循环逐步读取到ASP.NET Core中，然后发送给客户端浏览器
            var fileName = Path.GetFileName(filePath);//测试文档.xlsx

            int bufferSize = 1024;//这就是ASP.NET Core循环读取下载文件的缓存大小，这里我们设置为了1024字节，也就是说ASP.NET Core每次会从下载文件中读取1024字节的内容到服务器内存中，然后发送到客户端浏览器，这样避免了一次将整个下载文件都加载到服务器内存中，导致服务器崩溃

            controller.Response.ContentType = "application/octet-stream";//由于我们下载的是一个Excel文件，所以设置ContentType为application/vnd.ms-excel
                                                                         //在Response的Header中设置下载文件的文件名，这样客户端浏览器才能正确显示下载的文件名，
                                                                         //注意这里要用HttpUtility.UrlEncode编码文件名，否则有些浏览器可能会显示乱码文件名
            var contentDisposition = "attachment;" + "filename=" + HttpUtility.UrlEncode(fileName);
            controller.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");


            controller.Response.Headers.Add("Content-Disposition", new string[] { contentDisposition });

            //使用FileStream开始循环读取要下载文件的内容
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (controller.Response.Body)//调用Response.Body.Dispose()并不会关闭客户端浏览器到ASP.NET Core服务器的连接，之后还可以继续往Response.Body中写入数据
                {
                    long contentLength = fs.Length;//获取下载文件的大小
                    controller.Response.ContentLength = contentLength;//在Response的Header中设置下载文件的大小，这样客户端浏览器才能正确显示下载的进度

                    byte[] buffer;
                    long hasRead = 0;//变量hasRead用于记录已经发送了多少字节的数据到客户端浏览器

                    //如果hasRead小于contentLength，说明下载文件还没读取完毕，继续循环读取下载文件的内容，并发送到客户端浏览器
                    while (hasRead < contentLength)
                    {
                        //HttpContext.RequestAborted.IsCancellationRequested可用于检测客户端浏览器和ASP.NET Core服务器之间的连接状态，如果HttpContext.RequestAborted.IsCancellationRequested返回true，说明客户端浏览器中断了连接
                        if (controller.HttpContext.RequestAborted.IsCancellationRequested)
                        {
                            //如果客户端浏览器中断了到ASP.NET Core服务器的连接，这里应该立刻break，取消下载文件的读取和发送，避免服务器耗费资源
                            break;
                        }

                        buffer = new byte[bufferSize];

                        int currentRead = fs.Read(buffer, 0, bufferSize);//从下载文件中读取bufferSize(1024字节)大小的内容到服务器内存中

                        controller.Response.Body.Write(buffer, 0, currentRead);//发送读取的内容数据到客户端浏览器
                        controller.Response.Body.Flush();//注意每次Write后，要及时调用Flush方法，及时释放服务器内存空间

                        hasRead += currentRead;//更新已经发送到客户端浏览器的字节数
                    }
                }
            }

            return new EmptyResult();
        }
    }
}
