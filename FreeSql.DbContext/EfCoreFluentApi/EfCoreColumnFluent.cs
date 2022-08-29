﻿using FreeSql.DataAnnotations;

namespace FreeSql.Extensions.EfCoreFluentApi
{
    public class EfCoreColumnFluent
    {
        ColumnFluent _cf;

        internal EfCoreColumnFluent(ColumnFluent tf)
        {
            _cf = tf;
        }

        /// <summary>
        /// 使用 FreeSql FluentApi 方法，当 EFCore FluentApi 方法无法表示的时候使用
        /// </summary>
        /// <returns></returns>
        public ColumnFluent Help() => _cf;

        public EfCoreColumnFluent HasColumnName(string name)
        {
            _cf.Name(name);
            return this;
        }
        public EfCoreColumnFluent HasColumnType(string dbtype)
        {
            _cf.DbType(dbtype);
            return this;
        }
        public EfCoreColumnFluent IsRequired()
        {
            _cf.IsNullable(false);
            return this;
        }
        public EfCoreColumnFluent HasMaxLength(int length)
        {
            _cf.StringLength(length);
            return this;
        }
        public EfCoreColumnFluent HasDefaultValueSql(string sqlValue)
        {
            _cf.InsertValueSql(sqlValue);
            return this;
        }
        public EfCoreColumnFluent IsRowVersion()
        {
            _cf.IsVersion(true);
            return this;
        }
        //public EfCoreColumnFluent HasConversion(Func<object, string> stringify, Func<string, object> parse)
        //{
        //    return this;
        //}
        public EfCoreColumnFluent HasPrecision(int precision, int scale = 0)
        {
            _cf.Precision(precision, scale);
            return this;
        }

    }
}
