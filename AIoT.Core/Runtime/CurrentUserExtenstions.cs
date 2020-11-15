using IdentityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using Castle.DynamicProxy.Internal;

namespace AIoT.Core.Runtime
{
    /// <summary>
    /// 当前会话用户
    /// </summary>
    public static class CurrentUserExtenstions
    {
        /// <summary>
        /// 获取当前用户Id
        /// </summary>
        /// <exception cref="InvalidOperationException">用户Id解析失败</exception>
        public static long GetUserId(this ICurrentUser currentUser)
        {
            if (long.TryParse(currentUser.UserId, out var userId))
                return userId;

            throw new InvalidOperationException($"用户Id解析失败: {currentUser.UserId} 不是有效的 long 类型");
        }

        /// <summary>
        /// 获取当前用户Id
        /// </summary>
        public static long? TryGetUserId(this ICurrentUser currentUser)
        {
            if (long.TryParse(currentUser.UserId, out var userId))
                return userId;

            return null;
        }

        /// <summary>
        /// 获取当前 客户端Id
        /// </summary>
        public static string GetClientId(this ICurrentUser currentUser)
        {
            return currentUser.Claims?.GetValue(JwtClaimTypes.ClientId);
        }

        /// <summary>
        /// 获取当前用户角色Id
        /// </summary>
        public static IEnumerable<int> GetAllRoles(this ICurrentUser currentUser)
        {
            return currentUser.Claims.Where(p => p.Type == JwtClaimTypes.Role)
                .Select(p => p.Value.As<int?>()).Where(p => p.HasValue).Select(p => p.Value);
        }
        /// <summary>
        /// 获取单个值
        /// </summary>
        public static string GetValue(this IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(p => p.Type == type)?.Value;
        }
        public static Type GetNonNullableType(this Type type)
        {
            if (!type.IsNullableType())
            {
                return type;
            }

            return type.GetGenericArguments()[0];
        }
        /// <summary>
        /// 字符串是转换为 指定类型
        /// </summary>
        /// <typeparam name="TValue">转换的目标类型</typeparam>
        public static TValue As<TValue>(this string value, TValue defaultValue = default)
        {
            try
            {
                var valType = typeof(TValue).GetNonNullableType();

                TypeConverter converter = TypeDescriptor.GetConverter(valType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    return (TValue)converter.ConvertFrom(value);
                }
                converter = TypeDescriptor.GetConverter(typeof(TValue));
                if (converter.CanConvertTo(valType))
                {
                    return (TValue)converter.ConvertTo(value, valType);
                }
            }
            catch (Exception)
            {
            }
            return defaultValue;
        }
    }
}