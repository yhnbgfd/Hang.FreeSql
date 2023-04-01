﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.QuestDb.Curd
{
    class QuestDbInsert<T1> : Internal.CommonProvider.InsertProvider<T1> where T1 : class
    {
        public QuestDbInsert(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
            : base(orm, commonUtils, commonExpression)
        {
        }

        internal IFreeSql InternalOrm => _orm;
        internal TableInfo InternalTable => _table;
        internal DbParameter[] InternalParams => _params;
        internal DbConnection InternalConnection => _connection;
        internal DbTransaction InternalTransaction => _transaction;
        internal CommonUtils InternalCommonUtils => _commonUtils;
        internal CommonExpression InternalCommonExpression => _commonExpression;
        internal List<T1> InternalSource => _source;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalClearData() => ClearData();

        private int InternelExecuteAffrows()
        {
            //如果设置了RestAPI的Url则走HTTP
            var sql = ToSql();
            var execAsync = RestAPIExtension.ExecAsync(sql).GetAwaiter().GetResult();
            var resultHash = new Hashtable();
            try
            {
                resultHash = JsonConvert.DeserializeObject<Hashtable>(execAsync);
            }
            catch
            {
                if (execAsync.Contains("401"))
                {
                    throw new Exception("请确认new FreeSqlBuilder().UseQuestDbRestAPI()中设置的用户名密码是否正确.");
                }
            }
            var ddl = resultHash["ddl"]?.ToString();
            return ddl?.ToLower() == "ok" ? 1 : 0;
        }

        public override int ExecuteAffrows()
        {
            if (string.IsNullOrWhiteSpace(RestAPIExtension.BaseUrl))
            {
                return base.SplitExecuteAffrows(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
            }
            return InternelExecuteAffrows();
        }

        public override long ExecuteIdentity() => base.SplitExecuteIdentity(
            _batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        public override List<T1> ExecuteInserted() => base.SplitExecuteInserted(
            _batchValuesLimit > 0 ? _batchValuesLimit : 5000, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        protected override long RawExecuteIdentity()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
            if (identCols.Any() == false)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                try
                {
                    ret = _orm.Ado.ExecuteNonQuery(_connection, _transaction, CommandType.Text, sql, _commandTimeout,
                        _params);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }

                return 0;
            }

            sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name));
            before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            try
            {
                long.TryParse(
                    string.Concat(_orm.Ado.ExecuteScalar(_connection, _transaction, CommandType.Text, sql,
                        _commandTimeout, _params)), out ret);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

        protected override List<T1> RawExecuteInserted()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ")
                    .Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text,
                    sql, _commandTimeout, _params);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(RestAPIExtension.BaseUrl))
            {
                return base.SplitExecuteAffrowsAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                    _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);
            }
            return Task.FromResult(InternelExecuteAffrows());
        }
        

        public override Task<long> ExecuteIdentityAsync(CancellationToken cancellationToken = default) =>
            base.SplitExecuteIdentityAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        public override Task<List<T1>> ExecuteInsertedAsync(CancellationToken cancellationToken = default) =>
            base.SplitExecuteInsertedAsync(_batchValuesLimit > 0 ? _batchValuesLimit : 5000,
                _batchParameterLimit > 0 ? _batchParameterLimit : 3000, cancellationToken);

        async protected override Task<long> RawExecuteIdentityAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return 0;

            long ret = 0;
            Exception exception = null;
            Aop.CurdBeforeEventArgs before = null;

            var identCols = _table.Columns.Where(a => a.Value.Attribute.IsIdentity == true);
            if (identCols.Any() == false)
            {
                before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
                try
                {
                    ret = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql,
                        _commandTimeout, _params, cancellationToken);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw ex;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }

                return 0;
            }

            sql = string.Concat(sql, " RETURNING ", _commonUtils.QuoteSqlName(identCols.First().Value.Attribute.Name));
            before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            try
            {
                long.TryParse(
                    string.Concat(await _orm.Ado.ExecuteScalarAsync(_connection, _transaction, CommandType.Text, sql,
                        _commandTimeout, _params, cancellationToken)), out ret);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }

        async protected override Task<List<T1>> RawExecuteInsertedAsync(CancellationToken cancellationToken = default)
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.RereadColumn(col, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ")
                    .Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }

            sql = sb.ToString();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Insert, sql, _params);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction,
                    CommandType.Text, sql, _commandTimeout, _params, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }

            return ret;
        }
#endif
    }
}