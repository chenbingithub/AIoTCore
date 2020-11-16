using System;
using System.Collections.Generic;
using System.Text;

namespace AIoT.Core.EntityFrameworkCore
{
    public class CustomOptions
    {
        public  string MySqlConnectionString { get; set; }
        public  string SqlServerConnectionString { get; set; }
        public  string OracleConnectionString { get; set; }
    }
}
