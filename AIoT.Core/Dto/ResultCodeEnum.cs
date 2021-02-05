using System.ComponentModel.DataAnnotations;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// Api请求状态码
    /// </summary>
    public enum ResultCodeEnum
    {
        /// <summary>
        /// 操作成功
        ///</summary>
        [Display(Name = "操作成功")]
        Ok = 0,

        /// <summary>
        /// 操作失败
        ///</summary>
        [Display(Name = "操作失败")]
        Fail = 1,
        /// <summary>
        /// 已登录
        ///</summary>
        [Display(Name = "已登录")]
        LoggedIn = 2,
        /// <summary>
        /// 部分操作失败
        ///</summary>
        [Display(Name = "部分操作失败")]
        PartFail = 4,
        /// <summary>
        /// 服务数据异常
        ///</summary>
        [Display(Name = "服务数据异常")]
        ServerError = 10,
        /// <summary>
        /// 账号已在他处登录，请退出后再进行登录
        ///</summary>
        [Display(Name = "账号已在他处登录，请退出后再进行登录")]
        LoginOther = 19,
        /// <summary>
        /// 未登录
        ///</summary>
        [Display(Name = "未登录")]
        Unauthorized = 20,

        /// <summary>
        /// 未授权
        /// </summary>
        [Display(Name = "未授权")]
        Forbidden = 21,

        /// <summary>
        /// Token 失效
        /// </summary>
        [Display(Name = "Token 失效")]
        InvalidToken = 22,

        /// <summary>
        /// 密码验证失败
        /// </summary>
        [Display(Name = "密码验证失败")]
        SpaFailed = 23,

        /// <summary>
        /// 错误的新密码
        /// </summary>
        [Display(Name = "错误的新密码")]
        WrongNewPassword = 24,
        /// <summary>
        /// 参数校验失败
        /// </summary>
        [Display(Name = "参数校验失败")]
        ValidationFailed = 400,
        /// <summary>
        /// 参数验证失败
        /// </summary>
        [Display(Name = "参数验证失败")]
        InvalidData = 403,

        /// <summary>
        /// 没有此条记录
        ///</summary>
        [Display(Name = "没有此条记录")]
        NoRecord = 404,

        /// <summary>
        /// 重复记录
        /// </summary>
        [Display(Name = "重复记录")]
        DuplicateRecord = 405,

        /// <summary>
        /// 缺失基础数据
        /// </summary>
        [Display(Name = "缺失基础数据")]
        MissEssentialData = 406,

        /// <summary>
        /// 已被引用，不允许删除
        /// </summary>
        [Display(Name = "已被引用，不允许删除")]
        DeleteReferencedData = 410,
    }
}
