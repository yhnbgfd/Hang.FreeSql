﻿using FreeSql.DatabaseModel;
using FreeSql.Internal;
using FreeSql.Internal.Model;
using Newtonsoft.Json.Linq;
using Npgsql.LegacyPostgis;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeSql.PostgreSQL
{
    class PostgreSQLDbFirst : IDbFirst
    {
        IFreeSql _orm;
        protected CommonUtils _commonUtils;
        protected CommonExpression _commonExpression;
        public PostgreSQLDbFirst(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression)
        {
            _orm = orm;
            _commonUtils = commonUtils;
            _commonExpression = commonExpression;
        }

        public bool IsPg10 => ServerVersion >= 10;
        public int ServerVersion
        {
            get
            {
                if (_ServerVersionValue == 0 && _orm.Ado.MasterPool != null)
                    using (var conn = _orm.Ado.MasterPool.Get())
                    {
                        try
                        {
                            _ServerVersionValue = ParsePgVersion(conn.Value.ServerVersion, 10, 0).Item2;
                        }
                        catch
                        {
                            _ServerVersionValue = 9;
                        }
                    }
                return _ServerVersionValue;
            }
        }
        int _ServerVersionValue = 0;

        public int GetDbType(DbColumnInfo column) => (int)GetNpgsqlDbType(column);
        NpgsqlDbType GetNpgsqlDbType(DbColumnInfo column)
        {
            var dbtype = column.DbTypeText;
            var isarray = dbtype?.EndsWith("[]") == true;
            if (isarray) dbtype = dbtype.Remove(dbtype.Length - 2);
            NpgsqlDbType ret = NpgsqlDbType.Unknown;
            switch (dbtype?.ToLower().TrimStart('_'))
            {
                case "smallint":
                case "int2": ret = NpgsqlDbType.Smallint; break;
                case "integer":
                case "int4": ret = NpgsqlDbType.Integer; break;
                case "bigint":
                case "int8": ret = NpgsqlDbType.Bigint; break;
                case "numeric": ret = NpgsqlDbType.Numeric; break;
                case "real":
                case "float4": ret = NpgsqlDbType.Real; break;
                case "double precision":
                case "float8": ret = NpgsqlDbType.Double; break;
                case "money": ret = NpgsqlDbType.Money; break;

                case "bpchar": ret = NpgsqlDbType.Char; break;
                case "character varying":
                case "varchar": ret = NpgsqlDbType.Varchar; break;
                case "text": ret = NpgsqlDbType.Text; break;

                case "timestamp": ret = NpgsqlDbType.Timestamp; break;
                case "timestamptz": ret = NpgsqlDbType.TimestampTz; break;
                case "date": ret = NpgsqlDbType.Date; break;
                case "time": ret = NpgsqlDbType.Time; break;
                case "timetz": ret = NpgsqlDbType.TimeTz; break;
                case "interval": ret = NpgsqlDbType.Interval; break;

                case "bool": ret = NpgsqlDbType.Boolean; break;
                case "bytea": ret = NpgsqlDbType.Bytea; break;
                case "bit": ret = NpgsqlDbType.Bit; break;
                case "varbit": ret = NpgsqlDbType.Varbit; break;

                case "point": ret = NpgsqlDbType.Point; break;
                case "line": ret = NpgsqlDbType.Line; break;
                case "lseg": ret = NpgsqlDbType.LSeg; break;
                case "box": ret = NpgsqlDbType.Box; break;
                case "path": ret = NpgsqlDbType.Path; break;
                case "polygon": ret = NpgsqlDbType.Polygon; break;
                case "circle": ret = NpgsqlDbType.Circle; break;

                case "cidr": ret = NpgsqlDbType.Cidr; break;
                case "inet": ret = NpgsqlDbType.Inet; break;
                case "macaddr": ret = NpgsqlDbType.MacAddr; break;

                case "json": ret = NpgsqlDbType.Json; break;
                case "jsonb": ret = NpgsqlDbType.Jsonb; break;
                case "uuid": ret = NpgsqlDbType.Uuid; break;

                case "int4range": ret = NpgsqlDbType.Range | NpgsqlDbType.Integer; break;
                case "int8range": ret = NpgsqlDbType.Range | NpgsqlDbType.Bigint; break;
                case "numrange": ret = NpgsqlDbType.Range | NpgsqlDbType.Numeric; break;
                case "tsrange": ret = NpgsqlDbType.Range | NpgsqlDbType.Timestamp; break;
                case "tstzrange": ret = NpgsqlDbType.Range | NpgsqlDbType.TimestampTz; break;
                case "daterange": ret = NpgsqlDbType.Range | NpgsqlDbType.Date; break;

                case "hstore": ret = NpgsqlDbType.Hstore; break;
                case "geometry": ret = NpgsqlDbType.Geometry; break;
            }
            return isarray ? (ret | NpgsqlDbType.Array) : ret;
        }

        static readonly Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)> _dicDbToCs = new Dictionary<int, (string csConvert, string csParse, string csStringify, string csType, Type csTypeInfo, Type csNullableTypeInfo, string csTypeValue, string dataReaderMethod)>() {
                { (int)NpgsqlDbType.Smallint, ("(short?)", "short.Parse({0})", "{0}.ToString()", "short?", typeof(short), typeof(short?), "{0}.Value", "GetInt16") },
                { (int)NpgsqlDbType.Integer, ("(int?)", "int.Parse({0})", "{0}.ToString()", "int?", typeof(int), typeof(int?), "{0}.Value", "GetInt32") },
                { (int)NpgsqlDbType.Bigint, ("(long?)", "long.Parse({0})", "{0}.ToString()", "long?", typeof(long), typeof(long?), "{0}.Value", "GetInt64") },
                { (int)NpgsqlDbType.Numeric, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },
                { (int)NpgsqlDbType.Real, ("(float?)", "float.Parse({0})", "{0}.ToString()", "float?", typeof(float), typeof(float?), "{0}.Value", "GetFloat") },
                { (int)NpgsqlDbType.Double, ("(double?)", "double.Parse({0})", "{0}.ToString()", "double?", typeof(double), typeof(double?), "{0}.Value", "GetDouble") },
                { (int)NpgsqlDbType.Money, ("(decimal?)", "decimal.Parse({0})", "{0}.ToString()", "decimal?", typeof(decimal), typeof(decimal?), "{0}.Value", "GetDecimal") },

                { (int)NpgsqlDbType.Char, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)NpgsqlDbType.Varchar, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },
                { (int)NpgsqlDbType.Text, ("", "{0}.Replace(StringifySplit, \"|\")", "{0}.Replace(\"|\", StringifySplit)", "string", typeof(string), typeof(string), "{0}", "GetString") },

                { (int)NpgsqlDbType.Timestamp,  ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)NpgsqlDbType.TimestampTz,  ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)NpgsqlDbType.Date,  ("(DateTime?)", "new DateTime(long.Parse({0}))", "{0}.Ticks.ToString()", "DateTime?", typeof(DateTime), typeof(DateTime?), "{0}.Value", "GetDateTime") },
                { (int)NpgsqlDbType.Time, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)NpgsqlDbType.TimeTz, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },
                { (int)NpgsqlDbType.Interval, ("(TimeSpan?)", "TimeSpan.Parse(double.Parse({0}))", "{0}.Ticks.ToString()", "TimeSpan?", typeof(TimeSpan), typeof(TimeSpan?), "{0}.Value", "GetValue") },

                { (int)NpgsqlDbType.Boolean, ("(bool?)", "{0} == \"1\"", "{0} == true ? \"1\" : \"0\"", "bool?", typeof(bool), typeof(bool?), "{0}.Value", "GetBoolean") },
                { (int)NpgsqlDbType.Bytea, ("(byte[])", "Convert.FromBase64String({0})", "Convert.ToBase64String({0})", "byte[]", typeof(byte[]), typeof(byte[]), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Bit, ("(BitArray)", "{0}.ToBitArray()", "{0}.To1010()", "BitArray", typeof(BitArray), typeof(BitArray), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Varbit, ("(BitArray)", "{0}.ToBitArray()", "{0}.To1010()", "BitArray", typeof(BitArray), typeof(BitArray), "{0}", "GetValue") },

                { (int)NpgsqlDbType.Point, ("(NpgsqlPoint?)", "NpgsqlPoint.Parse({0})", "{0}.ToString()", "NpgsqlPoint", typeof(NpgsqlPoint), typeof(NpgsqlPoint?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Line, ("(NpgsqlLine?)", "NpgsqlLine.Parse({0})", "{0}.ToString()", "NpgsqlLine", typeof(NpgsqlLine), typeof(NpgsqlLine?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.LSeg, ("(NpgsqlLSeg?)", "NpgsqlLSeg.Parse({0})", "{0}.ToString()", "NpgsqlLSeg", typeof(NpgsqlLSeg), typeof(NpgsqlLSeg?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Box, ("(NpgsqlBox?)", "NpgsqlBox.Parse({0})", "{0}.ToString()", "NpgsqlBox", typeof(NpgsqlBox), typeof(NpgsqlBox?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Path, ("(NpgsqlPath?)", "NpgsqlPath.Parse({0})", "{0}.ToString()", "NpgsqlPath", typeof(NpgsqlPath), typeof(NpgsqlPath?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Polygon, ("(NpgsqlPolygon?)", "NpgsqlPolygon.Parse({0})", "{0}.ToString()", "NpgsqlPolygon", typeof(NpgsqlPolygon), typeof(NpgsqlPolygon?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Circle, ("(NpgsqlCircle?)", "NpgsqlCircle.Parse({0})", "{0}.ToString()", "NpgsqlCircle", typeof(NpgsqlCircle), typeof(NpgsqlCircle?), "{0}", "GetValue") },

                { (int)NpgsqlDbType.Cidr, ("((IPAddress, int)?)", "(IPAddress, int)({0})", "{0}.ToString()", "(IPAddress, int)", typeof((IPAddress, int)), typeof((IPAddress, int)?), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Inet, ("(IPAddress)", "IPAddress.Parse({0})", "{0}.ToString()", "IPAddress", typeof(IPAddress), typeof(IPAddress), "{0}", "GetValue") },
                { (int)NpgsqlDbType.MacAddr, ("(PhysicalAddress?)", "PhysicalAddress.Parse({0})", "{0}.ToString()", "PhysicalAddress", typeof(PhysicalAddress), typeof(PhysicalAddress), "{0}", "GetValue") },

                { (int)NpgsqlDbType.Json, ("(JToken)", "JToken.Parse({0})", "{0}.ToString()", "JToken", typeof(JToken), typeof(JToken), "{0}", "GetString") },
                { (int)NpgsqlDbType.Jsonb, ("(JToken)", "JToken.Parse({0})", "{0}.ToString()", "JToken", typeof(JToken), typeof(JToken), "{0}", "GetString") },
                { (int)NpgsqlDbType.Uuid, ("(Guid?)", "Guid.Parse({0})", "{0}.ToString()", "Guid", typeof(Guid), typeof(Guid?), "{0}", "GetString") },

                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Integer), ("(NpgsqlRange<int>?)", "{0}.ToNpgsqlRange<int>()", "{0}.ToString()", "NpgsqlRange<int>", typeof(NpgsqlRange<int>), typeof(NpgsqlRange<int>?), "{0}", "GetString") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Bigint), ("(NpgsqlRange<long>?)", "{0}.ToNpgsqlRange<long>()", "{0}.ToString()", "NpgsqlRange<long>", typeof(NpgsqlRange<long>), typeof(NpgsqlRange<long>?), "{0}", "GetString") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Numeric), ("(NpgsqlRange<decimal>?)", "{0}.ToNpgsqlRange<decimal>()", "{0}.ToString()", "NpgsqlRange<decimal>", typeof(NpgsqlRange<decimal>), typeof(NpgsqlRange<decimal>?), "{0}", "GetString") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Timestamp), ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()", "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?), "{0}", "GetString") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.TimestampTz), ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()", "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?), "{0}", "GetString") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Date), ("(NpgsqlRange<DateTime>?)", "{0}.ToNpgsqlRange<DateTime>()", "{0}.ToString()", "NpgsqlRange<DateTime>", typeof(NpgsqlRange<DateTime>), typeof(NpgsqlRange<DateTime>?), "{0}", "GetString") },

                { (int)NpgsqlDbType.Hstore, ("(Dictionary<string, string>)", "JsonConvert.DeserializeObject<Dictionary<string, string>>({0})", "JsonConvert.SerializeObject({0})", "Dictionary<string, string>", typeof(Dictionary<string, string>), typeof(Dictionary<string, string>), "{0}", "GetValue") },
                { (int)NpgsqlDbType.Geometry, ("(PostgisGeometry)", "JsonConvert.DeserializeObject<PostgisGeometry>({0})", "JsonConvert.SerializeObject({0})", "PostgisGeometry", typeof(PostgisGeometry), typeof(PostgisGeometry), "{0}", "GetValue") },

				/*** array ***/

				{ (int)(NpgsqlDbType.Smallint | NpgsqlDbType.Array), ("(short[])", "JsonConvert.DeserializeObject<short[]>({0})", "JsonConvert.SerializeObject({0})", "short[]", typeof(short[]), typeof(short[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Integer | NpgsqlDbType.Array), ("(int[])", "JsonConvert.DeserializeObject<int[]>({0})", "JsonConvert.SerializeObject({0})", "int[]", typeof(int[]), typeof(int[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Bigint | NpgsqlDbType.Array), ("(long[])", "JsonConvert.DeserializeObject<long[]>({0})", "JsonConvert.SerializeObject({0})", "long[]", typeof(long[]), typeof(long[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Numeric | NpgsqlDbType.Array), ("(decimal[])", "JsonConvert.DeserializeObject<decimal[]>({0})", "JsonConvert.SerializeObject({0})", "decimal[]", typeof(decimal[]), typeof(decimal[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Real | NpgsqlDbType.Array), ("(float[])", "JsonConvert.DeserializeObject<float[]>({0})", "JsonConvert.SerializeObject({0})", "float[]", typeof(float[]), typeof(float[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Double | NpgsqlDbType.Array), ("(double[])", "JsonConvert.DeserializeObject<double[]>({0})", "JsonConvert.SerializeObject({0})", "double[]", typeof(double[]), typeof(double[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Money | NpgsqlDbType.Array), ("(decimal[])", "JsonConvert.DeserializeObject<decimal[]>({0})", "JsonConvert.SerializeObject({0})", "decimal[]", typeof(decimal[]), typeof(decimal[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Char | NpgsqlDbType.Array), ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Varchar | NpgsqlDbType.Array), ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Text | NpgsqlDbType.Array), ("(string[])", "JsonConvert.DeserializeObject<string[]>({0})", "JsonConvert.SerializeObject({0})", "string[]", typeof(string[]), typeof(string[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Timestamp | NpgsqlDbType.Array), ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.TimestampTz | NpgsqlDbType.Array), ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Date | NpgsqlDbType.Array), ("(DateTime[])", "JsonConvert.DeserializeObject<DateTime[]>({0})", "JsonConvert.SerializeObject({0})", "DateTime[]", typeof(DateTime[]), typeof(DateTime[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Time | NpgsqlDbType.Array), ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.TimeTz | NpgsqlDbType.Array), ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Interval | NpgsqlDbType.Array), ("(TimeSpan[])", "JsonConvert.DeserializeObject<TimeSpan[]>({0})", "JsonConvert.SerializeObject({0})", "TimeSpan[]", typeof(TimeSpan[]), typeof(TimeSpan[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Boolean | NpgsqlDbType.Array), ("(bool[])", "JsonConvert.DeserializeObject<bool[]>({0})", "JsonConvert.SerializeObject({0})", "bool[]", typeof(bool[]), typeof(bool[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Bytea | NpgsqlDbType.Array), ("(byte[][])", "JsonConvert.DeserializeObject<byte[][]>({0})", "JsonConvert.SerializeObject({0})", "byte[][]", typeof(byte[][]), typeof(byte[][]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Bit | NpgsqlDbType.Array), ("(BitArray[])", "JsonConvert.DeserializeObject<BitArray[]>({0})", "JsonConvert.SerializeObject({0})", "BitArray[]", typeof(BitArray[]), typeof(BitArray[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Varbit | NpgsqlDbType.Array), ("(BitArray[])", "JsonConvert.DeserializeObject<BitArray[]>({0})", "JsonConvert.SerializeObject({0})", "BitArray[]", typeof(BitArray[]), typeof(BitArray[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Point | NpgsqlDbType.Array), ("(NpgsqlPoint[])", "JsonConvert.DeserializeObject<NpgsqlPoint[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlPoint[]", typeof(NpgsqlPoint[]), typeof(NpgsqlPoint[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Line | NpgsqlDbType.Array), ("(NpgsqlLine[])", "JsonConvert.DeserializeObject<BNpgsqlLineitArray[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlLine[]", typeof(NpgsqlLine[]), typeof(NpgsqlLine[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.LSeg | NpgsqlDbType.Array), ("(NpgsqlLSeg[])", "JsonConvert.DeserializeObject<NpgsqlLSeg[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlLSeg[]", typeof(NpgsqlLSeg[]), typeof(NpgsqlLSeg[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Box | NpgsqlDbType.Array), ("(NpgsqlBox[])", "JsonConvert.DeserializeObject<NpgsqlBox[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlBox[]", typeof(NpgsqlBox[]), typeof(NpgsqlBox[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Path | NpgsqlDbType.Array), ("(NpgsqlPath[])", "JsonConvert.DeserializeObject<NpgsqlPath[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlPath[]", typeof(NpgsqlPath[]), typeof(NpgsqlPath[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Polygon | NpgsqlDbType.Array), ("(NpgsqlPolygon[])", "JsonConvert.DeserializeObject<NpgsqlPolygon[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlPolygon[]", typeof(NpgsqlPolygon[]), typeof(NpgsqlPolygon[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Circle | NpgsqlDbType.Array), ("(NpgsqlCircle[])", "JsonConvert.DeserializeObject<NpgsqlCircle[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlCircle[]", typeof(NpgsqlCircle[]), typeof(NpgsqlCircle[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Cidr | NpgsqlDbType.Array), ("((IPAddress, int)[])", "JsonConvert.DeserializeObject<(IPAddress, int)[]>({0})", "JsonConvert.SerializeObject({0})", "(IPAddress, int)[]", typeof((IPAddress, int)[]), typeof((IPAddress, int)[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Inet | NpgsqlDbType.Array), ("(IPAddress[])", "JsonConvert.DeserializeObject<IPAddress[]>({0})", "JsonConvert.SerializeObject({0})", "IPAddress[]", typeof(IPAddress[]), typeof(IPAddress[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.MacAddr | NpgsqlDbType.Array), ("(PhysicalAddress[])", "JsonConvert.DeserializeObject<PhysicalAddress[]>({0})", "JsonConvert.SerializeObject({0})", "PhysicalAddress[]", typeof(PhysicalAddress[]), typeof(PhysicalAddress[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Json | NpgsqlDbType.Array), ("(JToken[])", "JsonConvert.DeserializeObject<JToken[]>({0})", "JsonConvert.SerializeObject({0})", "JToken[]", typeof(JToken[]), typeof(JToken[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Jsonb | NpgsqlDbType.Array), ("(JToken[])", "JsonConvert.DeserializeObject<JToken[]>({0})", "JsonConvert.SerializeObject({0})", "JToken[]", typeof(JToken[]), typeof(JToken[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Uuid | NpgsqlDbType.Array), ("(Guid[])", "JsonConvert.DeserializeObject<Guid[]>({0})", "JsonConvert.SerializeObject({0})", "Guid[]", typeof(Guid[]), typeof(Guid[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Integer | NpgsqlDbType.Array), ("(NpgsqlRange<int>[])", "JsonConvert.DeserializeObject<NpgsqlRange<int>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<int>[]", typeof(NpgsqlRange<int>[]), typeof(NpgsqlRange<int>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Bigint | NpgsqlDbType.Array), ("(NpgsqlRange<long>[])", "JsonConvert.DeserializeObject<NpgsqlRange<long>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<long>[]", typeof(NpgsqlRange<long>[]), typeof(NpgsqlRange<long>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Numeric | NpgsqlDbType.Array), ("(NpgsqlRange<decimal>[])", "JsonConvert.DeserializeObject<NpgsqlRange<decimal>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<decimal>[]", typeof(NpgsqlRange<decimal>[]), typeof(NpgsqlRange<decimal>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Timestamp | NpgsqlDbType.Array), ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]", typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.TimestampTz | NpgsqlDbType.Array), ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]", typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Range | NpgsqlDbType.Date | NpgsqlDbType.Array), ("(NpgsqlRange<DateTime>[])", "JsonConvert.DeserializeObject<NpgsqlRange<DateTime>[]>({0})", "JsonConvert.SerializeObject({0})", "NpgsqlRange<DateTime>[]", typeof(NpgsqlRange<DateTime>[]), typeof(NpgsqlRange<DateTime>[]), "{0}", "GetValue") },

                { (int)(NpgsqlDbType.Hstore | NpgsqlDbType.Array), ("(Dictionary<string, string>[])", "JsonConvert.DeserializeObject<Dictionary<string, string>[]>({0})", "JsonConvert.SerializeObject({0})", "Dictionary<string, string>[]", typeof(Dictionary<string, string>[]), typeof(Dictionary<string, string>[]), "{0}", "GetValue") },
                { (int)(NpgsqlDbType.Geometry | NpgsqlDbType.Array), ("(PostgisGeometry[])", "JsonConvert.DeserializeObject<PostgisGeometry[]>({0})", "JsonConvert.SerializeObject({0})", "PostgisGeometry[]", typeof(PostgisGeometry[]), typeof(PostgisGeometry[]), "{0}", "GetValue") },
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
            var sql = @" select datname from pg_database where datname not in ('template1', 'template0')";
            var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
            return ds.Select(a => a.FirstOrDefault()?.ToString()).ToList();
        }

        public bool ExistsTable(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var tbname = _commonUtils.SplitTableName(name);
            if (tbname?.Length == 1) tbname = new[] { "public", tbname[0] };
            if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
            var sql = $" select 1 from pg_tables a inner join pg_namespace b on b.nspname = a.schemaname where {(ignoreCase ? "lower(b.nspname)" : "b.nspname")}={_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(a.tablename)" : "a.tablename")}={_commonUtils.FormatSql("{0}", tbname[1])}";
            return string.Concat(_orm.Ado.ExecuteScalar(CommandType.Text, sql)) == "1";
        }

        public DbTableInfo GetTableByName(string name, bool ignoreCase = true) => GetTables(null, name, ignoreCase)?.FirstOrDefault();
        public List<DbTableInfo> GetTablesByDatabase(params string[] database) => GetTables(database, null, false);

        public List<DbTableInfo> GetTables(string[] database, string tablename, bool ignoreCase)
        {
            var olddatabase = "";
            var is96 = true;
            using (var conn = _orm.Ado.MasterPool.Get(TimeSpan.FromSeconds(5)))
            {
                olddatabase = conn.Value.Database;
                is96 = ParsePgVersion(conn.Value.ServerVersion, 9, 6).Item1;
            }
            string[] tbname = null;
            string[] dbs = database == null || database.Any() == false ? new[] { olddatabase } : database;
            if (string.IsNullOrEmpty(tablename) == false)
            {
                tbname = _commonUtils.SplitTableName(tablename);
                if (tbname?.Length == 1) tbname = new[] { "public", tbname[0] };
                if (ignoreCase) tbname = tbname.Select(a => a.ToLower()).ToArray();
                dbs = new[] { olddatabase };
            }
            var tables = new List<DbTableInfo>();

            foreach (var db in dbs)
            {
                if (string.IsNullOrEmpty(db) || string.Compare(db, olddatabase, true) != 0) continue;

                var loc1 = new List<DbTableInfo>();
                var loc2 = new Dictionary<string, DbTableInfo>();
                var loc3 = new Dictionary<string, Dictionary<string, DbColumnInfo>>();

                var sql = $@"
{(tbname == null ? "" : $"select * from (")}select
b.nspname || '.' || a.tablename,
a.schemaname,
a.tablename ,
d.description,
'TABLE'
from pg_tables a
inner join pg_namespace b on b.nspname = a.schemaname
inner join pg_class c on c.relnamespace = b.oid and c.relname = a.tablename
left join pg_description d on d.objoid = c.oid and objsubid = 0
where a.schemaname not in ('pg_catalog', 'information_schema', 'topology')
and b.nspname || '.' || a.tablename not in ('public.spatial_ref_sys')

union all

select
b.nspname || '.' || a.relname,
b.nspname,
a.relname,
d.description,
'VIEW'
from pg_class a
inner join pg_namespace b on b.oid = a.relnamespace
left join pg_description d on d.objoid = a.oid and objsubid = 0
where b.nspname not in ('pg_catalog', 'information_schema') and a.relkind in ('m','v') 
and b.nspname || '.' || a.relname not in ('public.geography_columns','public.geometry_columns','public.raster_columns','public.raster_overviews')
{(tbname == null ? "" : $") ft_dbf where {(ignoreCase ? "lower(schemaname)" : "schemaname")}={_commonUtils.FormatSql("{0}", tbname[0])} and {(ignoreCase ? "lower(tablename)" : "tablename")}={_commonUtils.FormatSql("{0}", tbname[1])}")}";
                var ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var loc6 = new List<string[]>();
                var loc66 = new List<string[]>();
                var loc6_1000 = new List<string>();
                var loc66_1000 = new List<string>();
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var owner = string.Concat(row[1]);
                    var table = string.Concat(row[2]);
                    var comment = string.Concat(row[3]);
                    Enum.TryParse<DbTableType>(string.Concat(row[4]), out var type);
                    loc2.Add(object_id, new DbTableInfo { Id = object_id.ToString(), Schema = owner, Name = table, Comment = comment, Type = type });
                    loc3.Add(object_id, new Dictionary<string, DbColumnInfo>());
                    switch (type)
                    {
                        case DbTableType.VIEW:
                        case DbTableType.TABLE:
                            loc6_1000.Add(object_id);
                            if (loc6_1000.Count >= 500)
                            {
                                loc6.Add(loc6_1000.ToArray());
                                loc6_1000.Clear();
                            }
                            break;
                        case DbTableType.StoreProcedure:
                            loc66_1000.Add(object_id);
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
ns.nspname || '.' || c.relname as id, 
a.attname,
t.typname,
case when a.atttypmod > 0 and a.atttypmod < 32767 then a.atttypmod - 4 else a.attlen end len,
case when t.typelem = 0 then t.typname else t2.typname end,
case when a.attnotnull then 0 else 1 end as is_nullable,
--e.adsrc as is_identity, pg12以下
(select pg_get_expr(adbin, adrelid) from pg_attrdef where adrelid = e.adrelid and adnum = e.adnum limit 1) is_identity,
d.description as comment,
a.attndims,
case when t.typelem = 0 then t.typtype else t2.typtype end,
ns2.nspname,
a.attnum{(IsPg10 ? ", a.attidentity" : "")}
from pg_class c
inner join pg_attribute a on a.attnum > 0 and a.attrelid = c.oid
inner join pg_type t on t.oid = a.atttypid
left join pg_type t2 on t2.oid = t.typelem
left join pg_description d on d.objoid = a.attrelid and d.objsubid = a.attnum
left join pg_attrdef e on e.adrelid = a.attrelid and e.adnum = a.attnum
inner join pg_namespace ns on ns.oid = c.relnamespace
inner join pg_namespace ns2 on ns2.oid = t.typnamespace
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || c.relname")}";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var type = string.Concat(row[2]);
                    var max_length = int.Parse(string.Concat(row[3]));
                    var sqlType = string.Concat(row[4]);
                    var is_nullable = string.Concat(row[5]) == "1";
                    var is_identity = string.Concat(row[6]).StartsWith(@"nextval('") && (string.Concat(row[6]).EndsWith(@"'::regclass)") || string.Concat(row[6]).EndsWith(@"')"))
                        || IsPg10 && new[] { "a", "d" }.Contains(string.Concat(row[12])); //pg10 GENERATED { BY DEFAULT | AWAYS } AS IDENTITY
                    var comment = string.Concat(row[7]);
                    var defaultValue = string.Concat(row[6]);
                    int attndims = int.Parse(string.Concat(row[8]));
                    string typtype = string.Concat(row[9]);
                    string owner = string.Concat(row[10]);
                    int attnum = int.Parse(string.Concat(row[11]));
                    switch (sqlType.ToLower())
                    {
                        case "bool": case "name": case "bit": case "varbit": case "bpchar": case "varchar": case "bytea": case "text": case "uuid": break;
                        default: max_length *= 8; break;
                    }
                    if (max_length <= 0) max_length = -1;
                    if (type.StartsWith("_"))
                    {
                        type = type.Substring(1);
                        if (attndims == 0) attndims++;
                    }
                    if (sqlType.StartsWith("_")) sqlType = sqlType.Substring(1);
                    if (max_length > 0)
                    {
                        switch (sqlType.ToLower())
                        {
                            //case "numeric": sqlType += $"({max_length})"; break;
                            case "bpchar": case "varchar": case "bytea": case "bit": case "varbit": sqlType += $"({max_length})"; break;
                        }
                    }
                    if (attndims > 0) type += "[]";

                    loc3[object_id].Add(column, new DbColumnInfo
                    {
                        Name = column,
                        MaxLength = max_length,
                        IsIdentity = is_identity,
                        IsNullable = is_nullable,
                        IsPrimary = false,
                        DbTypeText = type,
                        DbTypeTextFull = sqlType,
                        Table = loc2[object_id],
                        Comment = comment,
                        DefaultValue = defaultValue,
                        Position = attnum
                    });
                    loc3[object_id][column].DbType = this.GetDbType(loc3[object_id][column]);
                    loc3[object_id][column].CsType = this.GetCsTypeInfo(loc3[object_id][column]);
                }

                sql = $@"
select
ns.nspname || '.' || d.relname as table_id, 
c.attname,
b.relname as index_id,
case when a.indisunique then 1 else 0 end IsUnique,
case when a.indisprimary then 1 else 0 end IsPrimary,
case when a.indisclustered then 0 else 1 end IsClustered,
{(is96 ? "case when pg_index_column_has_property(b.oid, c.attnum, 'desc') = 't' then 1 else 0 end" : "0")} IsDesc,
a.indkey::text,
c.attnum
from pg_index a
inner join pg_class b on b.oid = a.indexrelid
inner join pg_attribute c on c.attnum > 0 and c.attrelid = b.oid
inner join pg_namespace ns on ns.oid = b.relnamespace
inner join pg_class d on d.oid = a.indrelid
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || d.relname")}
";
                ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                if (ds == null) return loc1;

                var indexColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
                var uniqueColumns = new Dictionary<string, Dictionary<string, DbIndexInfo>>();
                foreach (object[] row in ds)
                {
                    var object_id = string.Concat(row[0]);
                    var column = string.Concat(row[1]);
                    var index_id = string.Concat(row[2]);
                    var is_unique = string.Concat(row[3]) == "1";
                    var is_primary_key = string.Concat(row[4]) == "1";
                    var is_clustered = string.Concat(row[5]) == "1";
                    var is_desc = string.Concat(row[6]) == "1";
                    var inkey = string.Concat(row[7]).Split(' ');
                    var attnum = int.Parse(string.Concat(row[8]));
                    attnum = int.Parse(inkey[attnum - 1]);
                    //foreach (string tc in loc3[object_id].Keys) //bug: https://github.com/2881099/FreeSql.Wiki.VuePress/issues/9
                    //{
                    //    if (loc3[object_id][tc].DbTypeText.EndsWith("[]"))
                    //    {
                    //        column = tc;
                    //        break;
                    //    }
                    //}
                    if (loc3.ContainsKey(object_id) == false || loc3[object_id].ContainsKey(column) == false) continue;
                    var loc9 = loc3[object_id][column];
                    if (loc9.IsPrimary == false && is_primary_key) loc9.IsPrimary = is_primary_key;

                    Dictionary<string, DbIndexInfo> loc10 = null;
                    DbIndexInfo loc11 = null;
                    if (!indexColumns.TryGetValue(object_id, out loc10))
                        indexColumns.Add(object_id, loc10 = new Dictionary<string, DbIndexInfo>());
                    if (!loc10.TryGetValue(index_id, out loc11))
                        loc10.Add(index_id, loc11 = new DbIndexInfo());
                    loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                    if (is_unique && !is_primary_key)
                    {
                        if (!uniqueColumns.TryGetValue(object_id, out loc10))
                            uniqueColumns.Add(object_id, loc10 = new Dictionary<string, DbIndexInfo>());
                        if (!loc10.TryGetValue(index_id, out loc11))
                            loc10.Add(index_id, loc11 = new DbIndexInfo());
                        loc11.Columns.Add(new DbIndexColumnInfo { Column = loc9, IsDesc = is_desc });
                    }
                }
                foreach (var object_id in indexColumns.Keys)
                {
                    foreach (var column in indexColumns[object_id])
                        loc2[object_id].IndexesDict.Add(column.Key, column.Value);
                }
                foreach (var object_id in uniqueColumns.Keys)
                {
                    foreach (var column in uniqueColumns[object_id])
                    {
                        column.Value.Columns.Sort((c1, c2) => c1.Column.Name.CompareTo(c2.Column.Name));
                        loc2[object_id].UniquesDict.Add(column.Key, column.Value);
                    }
                }

                if (tbname == null)
                {
                    sql = $@"
select
ns.nspname || '.' || b.relname as table_id, 
array(select attname from pg_attribute where attrelid = a.conrelid and attnum = any(a.conkey)) as column_name,
a.conname as FKId,
ns2.nspname || '.' || c.relname as ref_table_id, 
1 as IsForeignKey,
array(select attname from pg_attribute where attrelid = a.confrelid and attnum = any(a.confkey)) as ref_column,
null ref_sln,
null ref_table
from  pg_constraint a
inner join pg_class b on b.oid = a.conrelid
inner join pg_class c on c.oid = a.confrelid
inner join pg_namespace ns on ns.oid = b.relnamespace
inner join pg_namespace ns2 on ns2.oid = c.relnamespace
where {loc8.ToString().Replace("a.table_name", "ns.nspname || '.' || b.relname")}
";
                    ds = _orm.Ado.ExecuteArray(CommandType.Text, sql);
                    if (ds == null) return loc1;

                    var fkColumns = new Dictionary<string, Dictionary<string, DbForeignInfo>>();
                    foreach (object[] row in ds)
                    {
                        var table_id = string.Concat(row[0]);
                        var column = row[1] as string[];
                        var fk_id = string.Concat(row[2]);
                        var ref_table_id = string.Concat(row[3]);
                        var is_foreign_key = string.Concat(row[4]) == "1";
                        var referenced_column = row[5] as string[];
                        var referenced_db = string.Concat(row[6]);
                        var referenced_table = string.Concat(row[7]);

                        if (loc2.ContainsKey(ref_table_id) == false) continue;

                        Dictionary<string, DbForeignInfo> loc12 = null;
                        DbForeignInfo loc13 = null;
                        if (!fkColumns.TryGetValue(table_id, out loc12))
                            fkColumns.Add(table_id, loc12 = new Dictionary<string, DbForeignInfo>());
                        if (!loc12.TryGetValue(fk_id, out loc13))
                            loc12.Add(fk_id, loc13 = new DbForeignInfo { Table = loc2[table_id], ReferencedTable = loc2[ref_table_id] });

                        for (int a = 0; a < column.Length; a++)
                        {
                            loc13.Columns.Add(loc3[table_id][column[a]]);
                            loc13.ReferencedColumns.Add(loc3[ref_table_id][referenced_column[a]]);
                        }
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
                tables.AddRange(loc1);
            }
            return tables;
        }

        public List<DbEnumInfo> GetEnumsByDatabase(params string[] database)
        {
            if (database == null || database.Length == 0) return new List<DbEnumInfo>();
            var drs = _orm.Ado.Query<(string name, string label)>(CommandType.Text, _commonUtils.FormatSql(@"
select
ns.nspname || '.' || a.typname,
b.enumlabel
from pg_type a
inner join pg_enum b on b.enumtypid = a.oid
inner join pg_namespace ns on ns.oid = a.typnamespace
where a.typtype = 'e' and ns.nspname in (SELECT ""schema_name"" FROM information_schema.schemata where catalog_name in {0})", database));
            var ret = new Dictionary<string, Dictionary<string, string>>();
            foreach (var dr in drs)
            {
                if (ret.TryGetValue(dr.name, out var labels) == false) ret.Add(dr.name, labels = new Dictionary<string, string>());
                var key = dr.label;
                if (Regex.IsMatch(key, @"^[\u0391-\uFFE5a-zA-Z_\$][\u0391-\uFFE5a-zA-Z_\$\d]*$") == false)
                    key = $"Unkown{ret[dr.name].Count + 1}";
                if (labels.ContainsKey(key) == false) labels.Add(key, dr.label);
            }
            return ret.Select(a => new DbEnumInfo { Name = a.Key, Labels = a.Value }).ToList();
        }

        public static NativeTuple<bool, int, int> ParsePgVersion(string versionString, int v1, int v2)
        {
            int[] version = new int[] { 0, 0 };
            var vmatch = Regex.Match(versionString, @"(\d+)\.(\d+)");
            if (vmatch.Success)
            {
                version[0] = int.Parse(vmatch.Groups[1].Value);
                version[1] = int.Parse(vmatch.Groups[2].Value);
            }
            else
            {
                vmatch = Regex.Match(versionString, @"(\d+)");
                version[0] = int.Parse(vmatch.Groups[1].Value);
            }
            if (version[0] > v1) return NativeTuple.Create(true, version[0], version[1]);
            if (version[0] == v1 && version[1] >= v2) return NativeTuple.Create(true, version[0], version[1]);
            return NativeTuple.Create(false, version[0], version[1]);
        }
    }
}