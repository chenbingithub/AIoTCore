using System.Collections.Generic;

namespace AIoT.EntityFramework.EntityFrameworkCore
{
    public class CustomOptions
    {
        public List<SqlConnectionString> Options { get; set; }
    }

    public class SqlConnectionString 
    {
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DatabaseKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DatabaseProvider { get; set; }
    }
}
