﻿using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ClickHouse.Client.ADO;

namespace FreeSql.ClickHouse
{
    class ClickHouseDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public ClickHouseDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public int GetDbType(DbColumnInfo column) => (int)GetClickHouseDbType(column);
        DbType GetClickHouseDbType(DbColumnInfo column)
        {
            if (column.DbTypeText == "Nullable")
            {
                column.DbTypeText = column.DbTypeTextFull;
                   //(?<=\()(\S +)(?=\))
            }
            switch (column.DbTypeText?.ToLower())
            {
                case "bit":
                case "tinyint":
                case "bool":
                case "sbyte":
                case "int8":
                case "nullable(int8)": return DbType.SByte;
                case "byte":
                case "uint8":
                case "nullable(uint8)": return DbType.Byte;
                case "smallint":
                case "int16":
                case "nullable(int16)": return DbType.Int16;
                case "uint16":
                case "nullable(uint16)": return DbType.UInt16;
                case "int32":
                case "int":
                case "nullable(int32)": return DbType.Int32;
                case "uint":
                case "uint32":
                case "nullable(uint32)": return DbType.UInt32;
                case "bigint":
                case "int64":
                case "long":
                case "nullable(int64)": return DbType.Int64;
                case "uint64":
                case "ulong":
                case "nullable(uint64)": return DbType.UInt64;
                case "real":
                case "Float64":
                case "double":
                case "nullable(float64)": return DbType.Double;
                case "Float32":
                case "float":
                case "nullable(float32)": return DbType.Single;
                case "decimal":
                case "decimal128":
                case "nullable(decimal128)": return DbType.Decimal;
                case "date":
                case "nullable(date)": return DbType.Date;
                case "datetime":
                case "nullable(datetime)": return DbType.DateTime;
                case "datetime64":
                case "nullable(datetime64)": return DbType.DateTime;
                case "tinytext":
                case "text": 
                case "mediumtext":
                case "longtext":
                case "char":
                case "string":
                case "nullable(string)":
                case "varchar": return DbType.String;
                default:
                    {
                        if (column.DbTypeText?.ToLower().Contains("datetime") == true) 
                            return DbType.DateTime;
                        return DbType.String;
                    }
            }
        }

        static readonly Dictionary<int, DbToCs> _dicDbToCs = new Dictionary<int, DbToCs>() {
                { (int)DbType.SByte, new DbToCs("(sbyte?)", "sbyte.Parse({0})", "{0}.ToString()", "sbyte?", typeof(sbyte), typeof(sbyte?), "{0}.Value", "GetByte") },
                { (int)DbType.Int16, new DbToCs("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)DbType.Int32, new DbToCs("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)DbType.Int64, new DbToCs("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },

                { (int)DbType.Byte, new DbToCs("(byte?)", "byte.Parse({0})", "{0}.ToString()", "byte?", typeof(byte), typeof(byte?), "{0}.Value", "GetByte") },
                { (int)DbType.UInt16, new DbToCs("(ushort?)", "ushort.Parse({0})", "{0}.ToString()", "ushort?", typeof(ushort), typeof(ushort?), "{0}.Value", "GetInt16") },
                { (int)DbType.UInt32, new DbToCs("(uint?)", "uint.Parse({0})", "{0}.ToString()", "uint?", typeof(uint), typeof(uint?), "{0}.Value", "GetInt32") },
                { (int)DbType.UInt64, new DbToCs("(ulong?)", "ulong.Parse({0})", "{0}.ToString()", "ulong?", typeof(ulong), typeof(ulong?), "{0}.Value", "GetInt64") },

                { (int)DbType.Double, new DbToCs("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)DbType.Single, new DbToCs("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)DbType.Decimal, new DbToCs("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { (int)DbType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)DbType.Date, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)DbType.DateTime, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)DbType.DateTime2, new DbToCs("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },

                { (int)DbType.Guid, new DbToCs("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid?", typeof(Guid), typeof(Guid?), "{0}", "GetString") },
                { (int)DbType.String, new DbToCs("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

            };

        public string GetCsConvert(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csConvert : trydc.csConvert.Replace("?", "")) : null;
        public string GetCsParse(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csParse : null;
        public string GetCsStringify(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csStringify : null;
        public string GetCsType(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? (column.IsNullable ? trydc.csType : trydc.csType.Replace("?", "")) : null;
        public Type GetCsTypeInfo(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeInfo : null;
        public string GetCsTypeValue(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.csTypeValue : null;
        public string GetDataReaderMethod(DbColumnInfo column) => _dicDbToCs.TryGetValue(column.DbType, out var trydc) ? trydc.dataReaderMethod : null;

        public List<string> GetDatabases()
        {
            var sql = @" select name from system.databases where name not in ('system', 'default')";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false; 
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1)
            {
                var database = "";
                using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
                {
                    database = conn.Value.Database;
                }
                tbname = new[] { database, tbname[0] };
            }
            if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $" SELECT 1 FROM information_schema.tables WHERE {(ignoreCase ? "lower(table_schema)" : "table_schema")} = {_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(table_name)" : "table_name")} = {_commonUtils.FormatSql("{0}", tbname[1])}";
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            var loc1 = new List<DbTableInfo>();
            var loc2 = new Dictionary<string, DbTableInfo>(StringComparer.CurrentCultureIgnoreCase);
            var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>(StringComparer.CurrentCultureIgnoreCase);
            string[] tbname = null;
            if (string.IsNullOrEmpty(tablename) == false)
            {
                tbname = _commonUtils.SplitTableName(tablename);
                if (tbname?.Length == 1)
                {
                    using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
                    {
                        if (string.IsNullOrEmpty(conn.Value.Database)) return loc1;
                        tbname = new[] { conn.Value.Database, tbname[0] };
                    }
                }
                if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
                database = new[] { tbname[0] };
            }
            else if (database == null || database.Any() == false)
            {
                using (var conn = _orm.Ado.MasterPool.Get())
                {
                    if (string.IsNullOrEmpty(conn.Value.Database)) return loc1;
                    database = new[] { conn.Value.Database };
                }
            }

            var databaseIn = string.Join(",", database.Select(a => _commonUtils.FormatSql("{0}", a)));
            var sql = $@"
select 
concat(a.table_schema, '.', a.table_name) 'id',
a.table_schema 'schema',
a.table_name 'table',
a.table_comment,
a.table_type 'type'
from information_schema.tables a
where {(ignoreCase ? "lower(a.table_schema)" : "a.table_schema")} in ({databaseIn}){(tbname == null ? "" : $" and {(ignoreCase ? "lower(a.table_name)" : "a.table_name")}={_commonUtils.FormatSql("{0}", tbname[1])}")}";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var loc6 = new List<string[]>();
            var loc66 = new List<string[]>();
            var loc6_1000 = new List<string>();
            var loc66_1000 = new List<string>();
            foreach (var row in ds)
            {
                var table_id = string.Concat(row[0]);
                var schema = string.Concat(row[1]);
                var table = string.Concat(row[2]);
                var comment = string.Concat(row[3]);
                var type = string.Concat(row[4]) == "VIEW" ? DbTableType.VIEW : DbTableType.TABLE;
                if (database.Length == 1)
                {
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                    schema = "";
                }
                loc2.Add(table_id, new DbTableInfo { Id = table_id, Schema = schema, Name = table, Comment = comment, Type = type });
                loc3.Add(table_id, new Dictionary<string, DbColumnInfo>(StringComparer.CurrentCultureIgnoreCase));
                switch (type)
                {
                    case DbTableType.TABLE:
                    case DbTableType.VIEW:
                        loc6_1000.Add(table.Replace("'", "''"));
                        if (loc6_1000.Count >= 500)
                        {
                            loc6.Add(loc6_1000.ToArray());
                            loc6_1000.Clear();
                        }
                        break;
                    case DbTableType.StoreProcedure:
                        loc66_1000.Add(table.Replace("'", "''"));
                        if (loc66_1000.Count >= 500)
                        {
                            loc66.Add(loc66_1000.ToArray());
                            loc66_1000.Clear();
                        }
                        break;
                }
            }
            if (loc6_1000.Count > 0) loc6.Add(loc6_1000.ToArray());
            if (loc66_1000.Count > 0) loc66.Add(loc66_1000.ToArray());

            if (loc6.Count == 0) return loc1;
            var loc8 = new StringBuilder().Append("(");
            for (var loc8idx = 0; loc8idx < loc6.Count; loc8idx++)
            {
                if (loc8idx > 0) loc8.Append(" OR ");
                loc8.Append("a.table_name in (");
                for (var loc8idx2 = 0; loc8idx2 < loc6[loc8idx].Length; loc8idx2++)
                {
                    if (loc8idx2 > 0) loc8.Append(",");
                    loc8.Append($"'{loc6[loc8idx][loc8idx2]}'");
                }
                loc8.Append(")");
            }
            loc8.Append(")");

            sql = $@"
select
concat(a.table_schema, '.', a.table_name),
a.column_name,
a.data_type,
ifnull(a.character_maximum_length, 0) 'len',
a.column_type,
case when a.is_nullable = 'YES' then 1 else 0 end 'is_nullable',
case when locate('auto_increment', a.extra) > 0 then 1 else 0 end 'is_identity',
a.column_comment 'comment',
a.column_default 'default_value'
from information_schema.columns a
where {(ignoreCase ? "lower(a.table_schema)" : "a.table_schema")} in ({databaseIn}) and {loc8}
";
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var position = 0;
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
                string type = string.Concat(row[2]);
                //long max_length = long.Parse(string.Concat(row[3]));
                string sqlType = string.Concat(row[4]);
                var m_len = Regex.Match(sqlType, @"\w+\((\d+)");
                int max_length = m_len.Success ? int.Parse(m_len.Groups[1].Value) : -1;
                bool is_nullable = string.Concat(row[5]) == "1";
                bool is_identity = string.Concat(row[6]) == "1";
                string comment = string.Concat(row[7]);
                string defaultValue = string.Concat(row[8]);
                if (max_length == 0) max_length = -1;
                if (database.Length == 1)
                {
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                }
                loc3[table_id].Add(column, new DbColumnInfo
                {
                    Name = column,
                    MaxLength = max_length,
                    IsIdentity = is_identity,
                    IsNullable = is_nullable,
                    IsPrimary = false,
                    DbTypeText = type,
                    DbTypeTextFull = sqlType,
                    Table = loc2[table_id],
                    Comment = comment,
                    DefaultValue = defaultValue,
                    Position = ++position
                });
                loc3[table_id][column].DbType = this.GetDbType(loc3[table_id][column]);
                loc3[table_id][column].CsType = this.GetCsTypeInfo(loc3[table_id][column]);
            }

            sql = $@"
select 
concat(a.table_schema, '.', a.table_name) 'table_id',
a.column_name,
a.index_name 'index_id',
case when a.non_unique = 0 then 1 else 0 end 'IsUnique',
case when a.index_name = 'PRIMARY' then 1 else 0 end 'IsPrimaryKey',
0 'IsClustered',
0 'IsDesc'
from information_schema.statistics a
where {(ignoreCase ? "lower(a.table_schema)" : "a.table_schema")} in ({databaseIn}) and {loc8}
";
            ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            if (ds == null) return loc1;

            var indexColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>(StringComparer.CurrentCultureIgnoreCase);
            var uniqueColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var row in ds)
            {
                string table_id = string.Concat(row[0]);
                string column = string.Concat(row[1]);
                string index_id = string.Concat(row[2]);
                bool is_unique = string.Concat(row[3]) == "1";
                bool is_primary_key = string.Concat(row[4]) == "1";
                bool is_clustered = string.Concat(row[5]) == "1";
                bool is_desc = string.Concat(row[6]) == "1";
                if (database.Length == 1)
                    table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                if (loc3.ContainsKey(table_id) == false || loc3[table_id].ContainsKey(column) == false) continue;
                var loc9 = loc3[table_id][column];
                if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;

                Dictionary<string, DbIndexInfo> loc10 = null;
                DbIndexInfo loc11 = null;
                if (!indexColumns.TryGetValue(table_id, out loc10))
                    indexColumns.Add(table_id, loc10 = new Dictionary<string, DbIndexInfo>(StringComparer.CurrentCultureIgnoreCase));
                if (!loc10.TryGetValue(index_id, out loc11))
                    loc10.Add(index_id, loc11 = new DbIndexInfo());
                loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                if (is_unique && !is_primary_key)
                {
                    if (!uniqueColumns.TryGetValue(table_id, out loc10))
                        uniqueColumns.Add(table_id, loc10 = new Dictionary<string, DbIndexInfo>(StringComparer.CurrentCultureIgnoreCase));
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new DbIndexInfo());
                    loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                }
            }
            foreach (string table_id in indexColumns.Keys)
            {
                foreach (var column in indexColumns[table_id])
                    loc2[table_id].IndexesDict.Add(column.Key, column.Value);
            }
            foreach (string table_id in uniqueColumns.Keys)
            {
                foreach (var column in uniqueColumns[table_id])
                {
                    column.Value.Columns.Sort((c1, c2) => c1.Column.Name.CompareTo(c2.Column.Name));
                    loc2[table_id].UniquesDict.Add(column.Key, column.Value);
                }
            }

            if (tbname == null)
            {
                sql = $@"
select 
concat(a.constraint_schema, '.', a.table_name) 'table_id',
a.column_name,
a.constraint_name 'FKId',
concat(a.referenced_table_schema, '.', a.referenced_table_name) 'ref_table_id',
1 'IsForeignKey',
a.referenced_column_name 'ref_column'
from information_schema.key_column_usage a
where {(ignoreCase ? "lower(a.constraint_schema)" : "a.constraint_schema")} in ({databaseIn}) and {loc8} and not isnull(position_in_unique_constraint)
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var fkColumns = new Dictionary<string, Dictionary<string, DbForeignInfo>>(StringComparer.CurrentCultureIgnoreCase);
                foreach (var row in ds)
                {
                    string table_id = string.Concat(row[0]);
                    string column = string.Concat(row[1]);
                    string fk_id = string.Concat(row[2]);
                    string ref_table_id = string.Concat(row[3]);
                    bool is_foreign_key = string.Concat(row[4]) == "1";
                    string referenced_column = string.Concat(row[5]);
                    if (database.Length == 1)
                    {
                        table_id = table_id.Substring(table_id.IndexOf('.') + 1);
                        ref_table_id = ref_table_id.Substring(ref_table_id.IndexOf('.') + 1);
                    }
                    if (loc3.ContainsKey(table_id) == false || loc3[table_id].ContainsKey(column) == false) continue;
                    var loc9 = loc3[table_id][column];
                    if (loc2.ContainsKey(ref_table_id) == false) continue;
                    var loc10 = loc2[ref_table_id];
                    var loc11 = loc3[ref_table_id][referenced_column];

                    Dictionary<string, DbForeignInfo> loc12 = null;
                    DbForeignInfo loc13 = null;
                    if (!fkColumns.TryGetValue(table_id, out loc12))
                        fkColumns.Add(table_id, loc12 = new Dictionary<string, DbForeignInfo>(StringComparer.CurrentCultureIgnoreCase));
                    if (!loc12.TryGetValue(fk_id, out loc13))
                        loc12.Add(fk_id, loc13 = new DbForeignInfo { Table = loc2[table_id], ReferencedTable = loc10 });
                    loc13.Columns.Add(loc9);
                    loc13.ReferencedColumns.Add(loc11);
                }
                foreach (var table_id in fkColumns.Keys)
                    foreach (var fk in fkColumns[table_id])
                        loc2[table_id].ForeignsDict.Add(fk.Key, fk.Value);
            }

            foreach (var table_id in loc3.Keys)
            {
                foreach (var loc5 in loc3[table_id].Values)
                {
                    loc2[table_id].Columns.Add(loc5);
                    if (loc5.IsIdentity) loc2[table_id].Identitys.Add(loc5);
                    if (loc5.IsPrimary) loc2[table_id].Primarys.Add(loc5);
                }
            }
            foreach (var loc4 in loc2.Values)
            {
                //if (loc4.Primarys.Count == 0 && loc4.UniquesDict.Count > 0)
                //{
                //    foreach (var loc5 in loc4.UniquesDict.First().Value.Columns)
                //    {
                //        loc5.Column.IsPrimary = true;
                //        loc4.Primarys.Add(loc5.Column);
                //    }
                //}
                loc4.Primarys.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
                loc4.Columns.Sort((c1, c2) =>
                {
                    int compare = c2.IsPrimary.CompareTo(c1.IsPrimary);
                    if (compare == 0)
                    {
                        bool b1 = loc4.ForeignsDict.Values.Where(fk => fk.Columns.Where(c3 => c3.Name == c1.Name).Any()).Any();
                        bool b2 = loc4.ForeignsDict.Values.Where(fk => fk.Columns.Where(c3 => c3.Name == c2.Name).Any()).Any();
                        compare = b2.CompareTo(b1);
                    }
                    if (compare == 0) compare = c1.Name.CompareTo(c2.Name);
                    return compare;
                });
                loc1.Add(loc4);
            }
            loc1.Sort((t1, t2) =>
            {
                var ret = t1.Schema.CompareTo(t2.Schema);
                if (ret == 0) ret = t1.Name.CompareTo(t2.Name);
                return ret;
            });

            loc2.Clear();
            loc3.Clear();
            return loc1;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            return new List<DbEnumInfo>();
        }
    }
}