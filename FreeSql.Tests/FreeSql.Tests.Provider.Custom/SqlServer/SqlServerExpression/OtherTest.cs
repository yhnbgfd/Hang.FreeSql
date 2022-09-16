using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Custom.SqlServerExpression
{
    [Collection("SqlServerCollection")]
    public class OtherTest
    {

        ISelect<TableAllType> select => g.sqlserver.Select<TableAllType>();

        [Fact]
        public void Div()
        {
            var t1 = select.Where(a => a.testFieldInt / 3 > 3).Limit(10).ToList();
            var t2 = select.Where(a => a.testFieldLong / 3 > 3).Limit(10).ToList();
            var t3 = select.Where(a => a.testFieldShort / 3 > 3).Limit(10).ToList();

            var t4 = select.Where(a => a.testFieldInt / 3.0 > 3).Limit(10).ToList();
            var t5 = select.Where(a => a.testFieldLong / 3.0 > 3).Limit(10).ToList();
            var t6 = select.Where(a => a.testFieldShort / 3.0 > 3).Limit(10).ToList();

            var t7 = select.Where(a => a.testFieldDouble / 3 > 3).Limit(10).ToList();
            var t8 = select.Where(a => a.testFieldDecimal / 3 > 3).Limit(10).ToList();
            var t9 = select.Where(a => a.testFieldFloat / 3 > 3).Limit(10).ToList();
        }

        [Fact]
        public void Boolean()
        {
            var t1 = select.Where(a => a.testFieldBool == true).ToList();
            var t2 = select.Where(a => a.testFieldBool != true).ToList();
            var t3 = select.Where(a => a.testFieldBool == false).ToList();
            var t4 = select.Where(a => !a.testFieldBool).ToList();
            var t5 = select.Where(a => a.testFieldBool).ToList();
            var t51 = select.WhereCascade(a => a.testFieldBool).Limit(10).ToList();

            var t11 = select.Where(a => a.testFieldBoolNullable == true).ToList();
            var t22 = select.Where(a => a.testFieldBoolNullable != true).ToList();
            var t33 = select.Where(a => a.testFieldBoolNullable == false).ToList();
            var t44 = select.Where(a => !a.testFieldBoolNullable.Value).ToList();
            var t55 = select.Where(a => a.testFieldBoolNullable.Value).ToList();

            var t111 = select.Where(a => a.testFieldBool == true && a.Id > 0).ToList();
            var t222 = select.Where(a => a.testFieldBool != true && a.Id > 0).ToList();
            var t333 = select.Where(a => a.testFieldBool == false && a.Id > 0).ToList();
            var t444 = select.Where(a => !a.testFieldBool && a.Id > 0).ToList();
            var t555 = select.Where(a => a.testFieldBool && a.Id > 0).ToList();

            var t1111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0).ToList();
            var t2222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0).ToList();
            var t3333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0).ToList();
            var t4444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0).ToList();
            var t5555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0).ToList();

            var t11111 = select.Where(a => a.testFieldBool == true && a.Id > 0 && a.testFieldBool == true).ToList();
            var t22222 = select.Where(a => a.testFieldBool != true && a.Id > 0 && a.testFieldBool != true).ToList();
            var t33333 = select.Where(a => a.testFieldBool == false && a.Id > 0 && a.testFieldBool == false).ToList();
            var t44444 = select.Where(a => !a.testFieldBool && a.Id > 0 && !a.testFieldBool).ToList();
            var t55555 = select.Where(a => a.testFieldBool && a.Id > 0 && a.testFieldBool).ToList();

            var t111111 = select.Where(a => a.testFieldBoolNullable == true && a.Id > 0 && a.testFieldBoolNullable == true).ToList();
            var t222222 = select.Where(a => a.testFieldBoolNullable != true && a.Id > 0 && a.testFieldBoolNullable != true).ToList();
            var t333333 = select.Where(a => a.testFieldBoolNullable == false && a.Id > 0 && a.testFieldBoolNullable == false).ToList();
            var t444444 = select.Where(a => !a.testFieldBoolNullable.Value && a.Id > 0 && !a.testFieldBoolNullable.Value).ToList();
            var t555555 = select.Where(a => a.testFieldBoolNullable.Value && a.Id > 0 && a.testFieldBoolNullable.Value).ToList();
        }

        [Fact]
        public void Array()
        {
            IEnumerable<int> testlinqlist = new List<int>(new[] { 1, 2, 3 });
            var testlinq = select.Where(a => testlinqlist.Contains(a.testFieldInt)).ToList();

            //in not in
            var sql111 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql112 = select.Where(a => new[] { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql113 = select.Where(a => !new[] { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var inarray = new[] { 1, 2, 3 };
            var sql1111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToList();
            var sql1122 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToList();
            var sql1133 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToList();

            //in not in
            var sql11111 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();
            var sql11222 = select.Where(a => new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt) == false).ToList();
            var sql11333 = select.Where(a => !new List<int>() { 1, 2, 3 }.Contains(a.testFieldInt)).ToList();

            var sql11111a = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();
            var sql11222b = select.Where(a => new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt) == false).ToList();
            var sql11333c = select.Where(a => !new List<int>(new[] { 1, 2, 3 }).Contains(a.testFieldInt)).ToList();

            var inarray2 = new List<int>() { 1, 2, 3 };
            var sql111111 = select.Where(a => inarray.Contains(a.testFieldInt)).ToList();
            var sql112222 = select.Where(a => inarray.Contains(a.testFieldInt) == false).ToList();
            var sql113333 = select.Where(a => !inarray.Contains(a.testFieldInt)).ToList();

            var inarray2n = Enumerable.Range(1, 3333).ToArray();
            var sql1111111 = select.Where(a => inarray2n.Contains(a.testFieldInt)).ToList();
            var sql1122222 = select.Where(a => inarray2n.Contains(a.testFieldInt) == false).ToList();
            var sql1133333 = select.Where(a => !inarray2n.Contains(a.testFieldInt)).ToList();

            //nvarchar
            IEnumerable<string> stringlinqlist = new List<string>(new[] { "a1", "a2", "a3" });
            var ntestlinq = select.Where(a => stringlinqlist.Contains(a.testFieldString)).ToList();

            //in not in
            var nsql111 = select.Where(a => new[] { "a1", "a2", "a3" }.Contains(a.testFieldString)).ToList();
            var nsql112 = select.Where(a => new[] { "a1", "a2", "a3" }.Contains(a.testFieldString) == false).ToList();
            var nsql113 = select.Where(a => !new[] { "a1", "a2", "a3" }.Contains(a.testFieldString)).ToList();

            var ninarray = new[] { "a1", "a2", "a3" };
            var nsql1111 = select.Where(a => ninarray.Contains(a.testFieldString)).ToList();
            var nsql1122 = select.Where(a => ninarray.Contains(a.testFieldString) == false).ToList();
            var nsql1133 = select.Where(a => !ninarray.Contains(a.testFieldString)).ToList();

            //in not in
            var nsql11111 = select.Where(a => new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldString)).ToList();
            var nsql11222 = select.Where(a => new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldString) == false).ToList();
            var nsql11333 = select.Where(a => !new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldString)).ToList();

            var nsql11111a = select.Where(a => new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldString)).ToList();
            var nsql11222b = select.Where(a => new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldString) == false).ToList();
            var nsql11333c = select.Where(a => !new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldString)).ToList();

            var ninarray2 = new List<string>() { "a1", "a2", "a3" };
            var nsql111111 = select.Where(a => ninarray2.Contains(a.testFieldString)).ToList();
            var nsql112222 = select.Where(a => ninarray2.Contains(a.testFieldString) == false).ToList();
            var nsql113333 = select.Where(a => !ninarray2.Contains(a.testFieldString)).ToList();

            var ninarray2n = Enumerable.Range(1, 3333).Select(a => "testnvarchar" + a).ToArray();
            var nsql1111111 = select.Where(a => ninarray2n.Contains(a.testFieldString)).ToList();
            var nsql1122222 = select.Where(a => ninarray2n.Contains(a.testFieldString) == false).ToList();
            var nsql1133333 = select.Where(a => !ninarray2n.Contains(a.testFieldString)).ToList();

            //varchar
            IEnumerable<string> vstringlinqlist = new List<string>(new[] { "a1", "a2", "a3" });
            var vtestlinq = select.Where(a => vstringlinqlist.Contains(a.testFieldStringVarchar)).ToList();

            //in not in
            var vsql111 = select.Where(a => new[] { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar)).ToList();
            var vsql112 = select.Where(a => new[] { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql113 = select.Where(a => !new[] { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar)).ToList();

            var vinarray = new[] { "a1", "a2", "a3" };
            var vsql1111 = select.Where(a => ninarray.Contains(a.testFieldStringVarchar)).ToList();
            var vsql1122 = select.Where(a => ninarray.Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql1133 = select.Where(a => !ninarray.Contains(a.testFieldStringVarchar)).ToList();

            //in not in
            var vsql11111 = select.Where(a => new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar)).ToList();
            var vsql11222 = select.Where(a => new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql11333 = select.Where(a => !new List<string>() { "a1", "a2", "a3" }.Contains(a.testFieldStringVarchar)).ToList();

            var vsql11111a = select.Where(a => new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldStringVarchar)).ToList();
            var vsql11222b = select.Where(a => new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql11333c = select.Where(a => !new List<string>(new[] { "a1", "a2", "a3" }).Contains(a.testFieldStringVarchar)).ToList();

            var vinarray2 = new List<string>() { "a1", "a2", "a3" };
            var vsql111111 = select.Where(a => ninarray2.Contains(a.testFieldStringVarchar)).ToList();
            var vsql112222 = select.Where(a => ninarray2.Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql113333 = select.Where(a => !ninarray2.Contains(a.testFieldStringVarchar)).ToList();

            var vinarray2n = Enumerable.Range(1, 3333).Select(a => "testvarchar" + a).ToArray();
            var vsql1111111 = select.Where(a => vinarray2n.Contains(a.testFieldStringVarchar)).ToList();
            var vsql1122222 = select.Where(a => vinarray2n.Contains(a.testFieldStringVarchar) == false).ToList();
            var vsql1133333 = select.Where(a => !vinarray2n.Contains(a.testFieldStringVarchar)).ToList();
        }

        [Table(Name = "tb_alltype")]
        class TableAllType
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            [Column(Name = "testFieldBool1111")]
            public bool testFieldBool { get; set; }
            public sbyte testFieldSByte { get; set; }
            public short testFieldShort { get; set; }
            public int testFieldInt { get; set; }
            public long testFieldLong { get; set; }
            public byte testFieldByte { get; set; }
            public ushort testFieldUShort { get; set; }
            public uint testFieldUInt { get; set; }
            public ulong testFieldULong { get; set; }
            public double testFieldDouble { get; set; }
            public float testFieldFloat { get; set; }
            public decimal testFieldDecimal { get; set; }
            public TimeSpan testFieldTimeSpan { get; set; }
            public DateTime testFieldDateTime { get; set; }
            public DateTimeOffset testFieldDateTimeOffset { get; set; }
            public byte[] testFieldBytes { get; set; }
            public string testFieldString { get; set; }

            [Column(DbType = "varchar(255)")]
            public string testFieldStringVarchar { get; set; }
            public Guid testFieldGuid { get; set; }

            public bool? testFieldBoolNullable { get; set; }
            public sbyte? testFieldSByteNullable { get; set; }
            public short? testFieldShortNullable { get; set; }
            public int? testFieldIntNullable { get; set; }
            public long? testFielLongNullable { get; set; }
            public byte? testFieldByteNullable { get; set; }
            public ushort? testFieldUShortNullable { get; set; }
            public uint? testFieldUIntNullable { get; set; }
            public ulong? testFieldULongNullable { get; set; }
            public double? testFieldDoubleNullable { get; set; }
            public float? testFieldFloatNullable { get; set; }
            public decimal? testFieldDecimalNullable { get; set; }
            public TimeSpan? testFieldTimeSpanNullable { get; set; }
            public DateTime? testFieldDateTimeNullable { get; set; }
            public DateTimeOffset? testFieldDateTimeNullableOffset { get; set; }
            public Guid? testFieldGuidNullable { get; set; }

            public TableAllTypeEnumType1 testFieldEnum1 { get; set; }
            public TableAllTypeEnumType1? testFieldEnum1Nullable { get; set; }
            public TableAllTypeEnumType2 testFieldEnum2 { get; set; }
            public TableAllTypeEnumType2? testFieldEnum2Nullable { get; set; }
        }

        public enum TableAllTypeEnumType1 { e1, e2, e3, e5 }
        [Flags] public enum TableAllTypeEnumType2 { f1, f2, f3 }
    }
}
