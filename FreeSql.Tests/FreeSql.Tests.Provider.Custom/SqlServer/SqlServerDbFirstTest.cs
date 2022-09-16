using FreeSql.DataAnnotations;
using System;
using Xunit;

namespace FreeSql.Tests.Custom.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerDbFirstTest
    {
        [Fact]
        public void GetDatabases()
        {
            var t1 = g.sqlserver.DbFirst.GetDatabases();
        }

        [Fact]
        public void GetTablesByDatabase()
        {
            var t2 = g.sqlserver.DbFirst.GetTablesByDatabase();
            Assert.True(t2.Count > 0);
        }

        [Fact]
        public void GetTableByName()
        {
            var fsql = g.sqlserver;
            var t1 = fsql.DbFirst.GetTableByName("tb_alltype");
            var t2 = fsql.DbFirst.GetTableByName("dbo.tb_alltype");
            Assert.NotNull(t1);
            Assert.NotNull(t2);
            Assert.True(t1.Columns.Count > 0);
            Assert.True(t2.Columns.Count > 0);
            Assert.Equal(t1.Columns.Count, t2.Columns.Count);
            var t3 = fsql.DbFirst.GetTableByName("notexists_tb");
            Assert.Null(t3);
        }

        [Fact]
        public void ExistsTable()
        {
            var fsql = g.sqlserver;
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("dbo.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("test_existstb01", false));
            Assert.False(fsql.DbFirst.ExistsTable("dbo.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01));
            Assert.True(fsql.DbFirst.ExistsTable("test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("dbo.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("Test_existstb01", false));
            Assert.True(fsql.DbFirst.ExistsTable("dbo.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table test_existstb01");

            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.False(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01", false));
            fsql.CodeFirst.SyncStructure(typeof(test_existstb01), "xxxtb.test_existstb01");
            Assert.True(fsql.DbFirst.ExistsTable("xxxtb.test_existstb01"));
            Assert.True(fsql.DbFirst.ExistsTable("xxxtb.Test_existstb01", false));
            fsql.Ado.ExecuteNonQuery("drop table xxxtb.test_existstb01");
        }
        class test_existstb01
        {
            public Guid id { get; set; }
        }
    }
}
