using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AIoT.Core.Enums;
using AIoT.Core.Extensions;
using Newtonsoft.Json;
using NewtonJson = Newtonsoft.Json;
using TextJson = System.Text.Json.Serialization;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 状态码
    /// </summary>
    [Serializable]
    public readonly partial struct ResultCode
    {
        /// <summary>
        /// 操作成功
        ///</summary>
        [Display(Name = "操作成功")]
        public static ResultCode Ok = ResultCodeEnum.Ok;

        /// <summary>
        /// 操作失败
        ///</summary>
        [Display(Name = "操作失败")]
        public static ResultCode Fail = ResultCodeEnum.Fail;
        /// <summary>
        /// 已登录
        ///</summary>
        [Display(Name = "已登录")]
        public static ResultCode LoggedIn = ResultCodeEnum.LoggedIn;

        /// <summary>
        /// 部分操作失败
        ///</summary>
        [Display(Name = "部分操作失败")]
        public static ResultCode PartFail = ResultCodeEnum.PartFail;

        /// <summary>
        /// 服务数据异常
        ///</summary>
        [Display(Name = "服务数据异常")]
        public static ResultCode ServerError = ResultCodeEnum.ServerError;

        /// <summary>
        /// 未登录
        ///</summary>
        [Display(Name = "未登录")]
        public static ResultCode Unauthorized = ResultCodeEnum.Unauthorized;
        /// <summary>
        /// 该账号已在其他设备登录
        ///</summary>
        [Display(Name = "该账号已在其他设备登录")]
        public static ResultCode LoginOther = ResultCodeEnum.LoginOther;
        /// <summary>
        /// 未授权
        /// </summary>
        [Display(Name = "未授权")]
        public static ResultCode Forbidden = ResultCodeEnum.Forbidden;

        /// <summary>
        /// Token 失效
        /// </summary>
        [Display(Name = "Token 失效")]
        public static ResultCode InvalidToken = ResultCodeEnum.InvalidToken;

        /// <summary>
        /// 密码验证失败
        /// </summary>
        [Display(Name = "密码验证失败")]
        public static ResultCode SpaFailed = ResultCodeEnum.SpaFailed;

        /// <summary>
        /// 错误的新密码
        /// </summary>
        [Display(Name = "错误的新密码")]
        public static ResultCode WrongNewPassword = ResultCodeEnum.WrongNewPassword;

        /// <summary>
        /// 参数验证失败
        /// </summary>
        [Display(Name = "参数验证失败")]
        public static ResultCode InvalidData = ResultCodeEnum.InvalidData;

        /// <summary>
        /// 没有此条记录
        ///</summary>
        [Display(Name = "没有此条记录")]
        public static ResultCode NoRecord = ResultCodeEnum.NoRecord;

        /// <summary>
        /// 重复记录
        /// </summary>
        [Display(Name = "已有记录，请勿重复操作")]
        public static ResultCode DuplicateRecord = ResultCodeEnum.DuplicateRecord;

        /// <summary>
        /// 缺失基础数据
        /// </summary>
        [Display(Name = "缺失基础数据")]
        public static ResultCode MissEssentialData = ResultCodeEnum.MissEssentialData;
        /// <summary>
        /// 已被引用，不允许删除
        /// </summary>
        [Display(Name = "已被引用，不允许删除")]
        public static ResultCode DeleteReferencedData = ResultCodeEnum.DeleteReferencedData;
    }

    

    [JsonConverter(typeof(ResultCodeNewtonJsonConverter))]
    [TextJson.JsonConverter(typeof(ResultCodeTextJsonConvert))]
    public  partial struct ResultCode : IEquatable<ResultCode>, IComparable<ResultCode>, IComparable, IConvertible, IFormattable
    {
        private readonly string _display;
        private readonly int _val;

        /// <summary>
        /// 状态码
        /// </summary>
        public ResultCode(int code)
        {
            _display = default;
            _val = code;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public ResultCode(int code, string display)
        {
            _display = display;
            _val = code;
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public ResultCode(Enum code)
        {
            _display = code.DisplayName();
            _val = Convert.ToInt32(code);
        }

        public string DisplayName() => _display;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case int intVal:
                    return _val == intVal;
                case ResultCode code:
                    return _val == code._val;
                case Enum enumVal:
                    return _val == Convert.ToInt32(enumVal);
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => _val.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => _val.ToString();

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider)
            => _val.ToString(format, formatProvider);

        /// <inheritdoc />
        public bool Equals(ResultCode other) => this == other;

        /// <inheritdoc />
        public int CompareTo(object obj)
        {

            switch (obj)
            {
                case int intVal:
                    return _val.CompareTo(intVal);
                case ResultCode code:
                    return _val.CompareTo(code._val);
                case Enum enumVal:
                    return _val.CompareTo(Convert.ToInt32(enumVal));
                default:
                    return _val.CompareTo(obj);
            }
        }

        /// <inheritdoc />
        public int CompareTo(ResultCode other) => _val.CompareTo(other._val);

        public static bool operator ==(ResultCode a, ResultCode b) => a._val == b._val;

        public static bool operator !=(ResultCode a, ResultCode b) => a._val != b._val;

        public static bool operator ==(ResultCode a, int b) => a._val == b;

        public static bool operator !=(ResultCode a, int b) => a._val != b;

        public static bool operator >(ResultCode a, ResultCode b) => a._val > b._val;

        public static bool operator <(ResultCode a, ResultCode b) => a._val < b._val;

        public static bool operator >=(ResultCode a, ResultCode b) => a._val >= b._val;

        public static bool operator <=(ResultCode a, ResultCode b) => a._val <= b._val;

        public static implicit operator int(ResultCode code) => code._val;

        public static implicit operator ResultCode(int code) => new ResultCode(code);

        public static implicit operator ResultCode(Enum code) => new ResultCode(code);

        /// <inheritdoc />
        TypeCode IConvertible.GetTypeCode() => TypeCode.Int32;

        /// <inheritdoc />
        bool IConvertible.ToBoolean(IFormatProvider provider) => Convert.ToBoolean(_val);

        /// <inheritdoc />
        byte IConvertible.ToByte(IFormatProvider provider) => Convert.ToByte(_val);

        /// <inheritdoc />
        char IConvertible.ToChar(IFormatProvider provider) => Convert.ToChar(_val);

        /// <inheritdoc />
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => Convert.ToDateTime(_val);

        /// <inheritdoc />
        decimal IConvertible.ToDecimal(IFormatProvider provider) => Convert.ToDecimal(_val);

        /// <inheritdoc />
        double IConvertible.ToDouble(IFormatProvider provider) => Convert.ToDouble(_val);

        /// <inheritdoc />
        short IConvertible.ToInt16(IFormatProvider provider) => Convert.ToInt16(_val);

        /// <inheritdoc />
        int IConvertible.ToInt32(IFormatProvider provider) => Convert.ToInt32(_val);

        /// <inheritdoc />
        long IConvertible.ToInt64(IFormatProvider provider) => Convert.ToInt64(_val);

        /// <inheritdoc />
        sbyte IConvertible.ToSByte(IFormatProvider provider) => Convert.ToSByte(_val);

        /// <inheritdoc />
        float IConvertible.ToSingle(IFormatProvider provider) => Convert.ToSingle(_val);

        /// <inheritdoc />
        string IConvertible.ToString(IFormatProvider provider) => Convert.ToString(_val);

        /// <inheritdoc />
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) =>
            Convert.ChangeType(_val, conversionType, provider);

        /// <inheritdoc />
        ushort IConvertible.ToUInt16(IFormatProvider provider) => Convert.ToUInt16(_val);

        /// <inheritdoc />
        uint IConvertible.ToUInt32(IFormatProvider provider) => Convert.ToUInt32(_val);

        /// <inheritdoc />
        ulong IConvertible.ToUInt64(IFormatProvider provider) => Convert.ToUInt64(_val);
    }

    public class ResultCodeNewtonJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, NewtonJson.JsonSerializer serializer)
        {
            if (value == null) writer.WriteNull();
            else writer.WriteValue((ResultCode)value);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, NewtonJson.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var val = Convert.ToInt32(reader.Value);
            return (ResultCode)val;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(ResultCode) == objectType;
        }
    }

    public class ResultCodeTextJsonConvert : TextJson.JsonConverter<ResultCode>
    {
        /// <inheritdoc />
        public override ResultCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = Convert.ToInt32(reader.GetInt32());
            return val;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, ResultCode value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
