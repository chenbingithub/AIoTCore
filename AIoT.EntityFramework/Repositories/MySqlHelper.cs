﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIoT.Core.Dto;
using AIoT.Core.Runtime;
 using MySql.Data.MySqlClient;

namespace Fangte.Core.Repository.Internal
{
    /// <summary>
    /// MySql扩展方法
    /// </summary>
    public static class MySqlHelper
    {
        private const string defaultFieldTerminator = "\t";
        private const string defaultLineTerminator = "\n";
        private const char defaultEscapeCharacter = '\\';
        private const string StreamPrefix = ":STREAM:";

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> _cacheTypeProperties =
            new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// 生成 Load Data 命令
        /// </summary>
        public static string BuildSqlCommand(this MySqlBulkLoader bulk)
        {
            var sqlCommandMain = new StringBuilder("LOAD DATA ");
            if (bulk.Priority == MySqlBulkLoaderPriority.Low)
                sqlCommandMain.Append("LOW_PRIORITY ");
            else if (bulk.Priority == MySqlBulkLoaderPriority.Concurrent)
                sqlCommandMain.Append("CONCURRENT ");

            if (bulk.Local)
                sqlCommandMain.Append("LOCAL ");

            sqlCommandMain.Append("INFILE ");

            if (bulk.FileName != null)
                sqlCommandMain.AppendFormat("'{0}' ", StreamPrefix);
            else if (Path.DirectorySeparatorChar != '\\')
                sqlCommandMain.AppendFormat("'{0}' ", bulk.FileName);
            else
                sqlCommandMain.AppendFormat("'{0}' ", bulk.FileName?.Replace("\\", "\\\\"));

            if (bulk.ConflictOption == MySqlBulkLoaderConflictOption.Ignore)
                sqlCommandMain.Append("IGNORE ");
            else if (bulk.ConflictOption == MySqlBulkLoaderConflictOption.Replace)
                sqlCommandMain.Append("REPLACE ");

            sqlCommandMain.AppendFormat("INTO TABLE {0} ", bulk.TableName);

            if (!string.IsNullOrEmpty(bulk.CharacterSet))
                sqlCommandMain.AppendFormat("CHARACTER SET {0} ", bulk.CharacterSet);

            var sqlCommandFragment = new StringBuilder();
            if (bulk.FieldTerminator != defaultFieldTerminator)
                sqlCommandFragment.AppendFormat("TERMINATED BY \'{0}\' ", bulk.FieldTerminator);

            if (bulk.FieldQuotationCharacter != 0)
                sqlCommandFragment.AppendFormat("{0} ENCLOSED BY \'{1}\' ", (bulk.FieldQuotationOptional ? "OPTIONALLY" : ""), bulk.FieldQuotationCharacter);

            if (bulk.EscapeCharacter != defaultEscapeCharacter && bulk.EscapeCharacter != 0)
                sqlCommandFragment.AppendFormat("ESCAPED BY \'{0}\' ", bulk.EscapeCharacter);

            if (sqlCommandFragment.Length > 0)
            {
                sqlCommandMain.AppendFormat("FIELDS {0}", sqlCommandFragment);
                sqlCommandFragment.Clear();
            }

            if (!string.IsNullOrEmpty(bulk.LinePrefix))
                sqlCommandFragment.AppendFormat("STARTING BY \'{0}\' ", bulk.LinePrefix);

            if (bulk.LineTerminator != defaultLineTerminator)
                sqlCommandFragment.AppendFormat("TERMINATED BY \'{0}\' ", bulk.LineTerminator);

            if (sqlCommandFragment.Length > 0)
                sqlCommandMain.AppendFormat("LINES {0}", sqlCommandFragment);

            if (bulk.NumberOfLinesToSkip > 0)
                sqlCommandMain.AppendFormat("IGNORE {0} LINES ", bulk.NumberOfLinesToSkip);

            if (bulk.Columns.Count > 0)
            {
                sqlCommandMain.Append("(");
                sqlCommandMain.Append(bulk.Columns[0]);
                for (int i = 1; i < bulk.Columns.Count; i++)
                    sqlCommandMain.AppendFormat(",{0}", bulk.Columns[i]);
                sqlCommandMain.Append(") ");
            }

            if (bulk.Expressions.Count > 0)
            {
                sqlCommandMain.Append("SET ");
                sqlCommandMain.Append(bulk.Expressions[0]);
                for (int i = 1; i < bulk.Expressions.Count; i++)
                    sqlCommandMain.AppendFormat(",{0}", bulk.Expressions[i]);
            }

            return sqlCommandMain.ToString();
        }

        /// <summary>
        /// 执行 MySql数据库批量插入
        /// 注：MySqlBulkLoader 内部使用 MySqlConnection.CurrentTransaction，不需要外部指定事务
        /// 需要在连接字符串增加：AllowLoadLocalInfile=true
        /// </summary>
        public static async Task<int> MySqlBulkInsertAsync(this MySqlConnection con, 
            IDictionary<string, Type> columns, IEnumerable<IDictionary<string, object>> data,
            string tableName, MySqlBulkLoaderConflictOption dumpcate = MySqlBulkLoaderConflictOption.None,
            CancellationToken cancellationToken = default)
        {
            return await con.MySqlBulkInsertAsync(GetPropertiesCacheByKeyValue(columns), data, tableName, dumpcate, cancellationToken);
        }

        /// <summary>
        /// 执行 MySql数据库批量插入
        /// 注：MySqlBulkLoader 内部使用 MySqlConnection.CurrentTransaction，不需要外部指定事务
        /// 需要在连接字符串增加：AllowLoadLocalInfile=true
        /// </summary>
        public static async Task<int> MySqlBulkInsertAsync<T>(this MySqlConnection con, IEnumerable<T> data,
            string tableName = null, MySqlBulkLoaderConflictOption dumpcate = MySqlBulkLoaderConflictOption.None,
            CancellationToken cancellationToken = default)
        {
            return await con.MySqlBulkInsertAsync(GetPropertiesCacheByType(typeof(T)), data, tableName, dumpcate, cancellationToken);
        }


        /// <summary>
        /// 执行 MySql数据库批量插入
        /// 注：MySqlBulkLoader 内部使用 MySqlConnection.CurrentTransaction，不需要外部指定事务
        /// 需要在连接字符串增加：AllowLoadLocalInfile=true
        /// </summary>
        public static async Task<int> MySqlBulkInsertAsync<T>(this MySqlConnection con,
            IEnumerable<(string Name, Type PropertyType, Func<object, object> GetValue)> columns, 
            IEnumerable<T> data, 
            string tableName = null, 
            MySqlBulkLoaderConflictOption dumpcate = MySqlBulkLoaderConflictOption.None,
            CancellationToken cancellationToken = default)
        {
            //var profiler = MiniProfiler.Current;
            //CustomTiming timing = null;

            var type = typeof(T);
            tableName ??= type.Name;
            var properites = columns;
            string tmpPath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid():N}.csv");
            try
            {
                //var stream = new MemoryStream();
               
                var bulk = new MySqlBulkLoader(con)
                {
                    CharacterSet = "utf8",
                    FieldTerminator = ",",
                    LineTerminator = "\n",
                    EscapeCharacter = '\\',
                    FieldQuotationCharacter = '\"',
                    FieldQuotationOptional = true,
                    NumberOfLinesToSkip = 1,
                    Local = true,
                    TableName = $"`{tableName}`",
                    FileName = tmpPath,
                    //SourceStream = stream,
                    ConflictOption = dumpcate,
                };

                foreach (var property in properites)
                {
                    var propertyType = property.PropertyType.GetNonNullableType();
                    if (propertyType == typeof(bool))
                    {
                        var col = $"@{property.Name}";
                        bulk.Columns.Add(col);
                        bulk.Expressions.Add($"`{property.Name}`=CAST({col} AS UNSIGNED)");
                    }
                    else if (propertyType == typeof(byte[]))
                    {
                        var col = $"@{property.Name}";
                        bulk.Columns.Add(col);
                        bulk.Expressions.Add($"`{property.Name}`=UNHEX({col})");
                    }
                    else
                    {
                        bulk.Columns.Add($"`{property.Name}`");
                    }
                }

                #region 生成 CSV 文件

                var lineEscape = $"\\n";
                var fieldEscape = $"\\,";
                var quata = bulk.FieldQuotationCharacter.ToString();
                var quotaEscape = string.Concat(quata, quata);

                using (var writer = new StreamWriter(tmpPath,false, Encoding.UTF8))
                {
                    writer.Write(string.Join(bulk.FieldTerminator, bulk.Columns));

                    foreach (var entity in data)
                    {
                        writer.Write(bulk.LineTerminator);

                        var firstField = true;
                        foreach (var property in properites)
                        {
                            var clrVal = property.GetValue(entity);

                            if (!firstField)
                            {
                                writer.Write(bulk.FieldTerminator);
                            }
                            firstField = false;

                            switch (clrVal)
                            {
                                case null:
                                    writer.Write("NULL");
                                    break;
                                case Enum enumVal:
                                    writer.Write(enumVal.ToString("D"));
                                    break;
                                case DateTime dateTime:
                                    writer.Write(dateTime.TimeOfDay == TimeSpan.Zero
                                        ? $"{quata}{dateTime:yyyy-MM-dd}{quata}"
                                        : $"{quata}{dateTime:yyyy-MM-dd HH:mm:ss.ffff}{quata}");
                                    break;
                                case DateTimeOffset dateTimeOffset:
                                    writer.Write($"{quata}{dateTimeOffset.UtcDateTime:yyyy-MM-dd HH:mm:ss.ffff}{quata}");
                                    break;
                                case byte[] bytes:
                                    //writer.Write("0x");
                                    foreach (var b in bytes)
                                    {
                                        writer.Write(b.ToString("x2"));
                                    }
                                    break;
                                case bool boolval:
                                    writer.Write(boolval ? $"1" : $"0");
                                    break;
                                default:
                                    var strValue = clrVal.ToString();
                                    strValue = strValue.Replace("\\", "\\\\")
                                        .Replace("\n", "\\n")
                                        .Replace("\r", "\\r")
                                        .Replace("\t", "\\t")
                                        .Replace(bulk.FieldTerminator, fieldEscape)
                                        .Replace(bulk.LineTerminator, lineEscape)
                                        .Replace(quata, quotaEscape);

                                    writer.Write($"{quata}{strValue}{quata}");
                                    break;
                            }
                        }
                    }
                }

                #endregion

        
                var insertCount = await bulk.LoadAsync(cancellationToken);
               
                return insertCount;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"MySqlBulkInsertAsync():{ex.Message}");
                //if (timing != null) timing.Errored = true;
                throw ex;
            }
            finally
            {
                File.Delete(tmpPath);
            }
        }


        private static IEnumerable<(string Name, Type PropertyType, Func<object, object> GetValue)> GetPropertiesCacheByType(Type type)
        {
            var properites = _cacheTypeProperties.GetOrAdd(type.TypeHandle,
                p => type.GetProperties(BindingFlags.Instance | BindingFlags.Public));

            foreach (var property in properites)
            {
                yield return (property.Name, property.PropertyType, p => property.GetValue(p));
            }
        }
        
        private static IEnumerable<(string Name, Type PropertyType, Func<object, object> GetValue)> GetPropertiesCacheByKeyValue(IDictionary<string, Type> keyValue)
        {
            foreach (var property in keyValue)
            {
                yield return (property.Key, property.Value, p => ((IDictionary<string, object>) p)[property.Key]);
            }
        }
    }
}
