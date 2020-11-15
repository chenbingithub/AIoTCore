namespace AIoT.Core.EntityFrameworkCore
{
    /// <summary>
    /// 数据库连接配置
    /// </summary>
    public class DbConnectionOptions
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; }

        /// <inheritdoc />
        public DbConnectionOptions()
        {
            ConnectionStrings = new ConnectionStrings();
        }
    }
}