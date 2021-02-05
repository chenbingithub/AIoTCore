using System.Threading.Tasks;

namespace AIoT.Core.Authorization
{
    /// <summary>
    /// 功能权限验证接口
    /// </summary>
    public interface IPermissionChecker
    {
        /// <summary>
        /// 当前用户是否有指定功能权限
        /// </summary>
        Task<bool> IsGrantedAsync(string permission);

        /// <summary>
        /// 指定用户是否有指定功能权限
        /// </summary>
        Task<bool> IsGrantedAsync(string userId, string permission);
        /// <summary>
        /// 验证url路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> IsGrantedPathAsync(string path);
        /// <summary>
        /// 验证url路径
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> IsGrantedPathAsync(string userId, string path);
    }
}
