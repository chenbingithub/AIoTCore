using System;

namespace AIoT.Core.Data
{
    /// <summary>
    /// 是否启用只读库
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ReadonlyAttribute : DataFilterAttribute
    {
        /// <summary>
        /// 是否启用只读库
        /// </summary>
        public ReadonlyAttribute() : this(true) { }

        /// <summary>
        /// 是否启用只读库
        /// </summary>
        public ReadonlyAttribute(bool enable) : base(typeof(IReadonly), enable)
        {
        }
    }
}
