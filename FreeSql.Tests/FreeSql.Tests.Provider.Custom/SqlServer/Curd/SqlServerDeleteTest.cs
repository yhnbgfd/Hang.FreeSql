using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.Custom.SqlServer
{
    [Collection("SqlServerCollection")]
    public class SqlServerDeleteTest
    {
        IDelete<Topic> delete => g.sqlserver.Delete<Topic>(); //��������

        [Table(Name = "tb_topic22211")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public TestTypeInfo Type { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.sqlserver.Delete<Topic>().ToSql());
            var sql = g.sqlserver.Delete<Topic>(new[] { 1, 2 }).ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] IN (1,2))", sql);

            sql = g.sqlserver.Delete<Topic>(new Topic { Id = 1, Title = "test" }).ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] = 1)", sql);

            sql = g.sqlserver.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] IN (1,2))", sql);

            sql = g.sqlserver.Delete<Topic>(new { id = 1 }).ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] = 1)", sql);

            sql = g.sqlserver.Delete<MultiPkTopic>(new[] { new { Id1 = 1, Id2 = 10 }, new { Id1 = 2, Id2 = 20 } }).ToSql();
            Assert.Equal("DELETE FROM [MultiPkTopic] WHERE ([Id1] = 1 AND [Id2] = 10 OR [Id1] = 2 AND [Id2] = 20)", sql);
        }
        class MultiPkTopic
        {
            [Column(IsPrimary = true)]
            public int Id1 { get; set; }
            [Column(IsPrimary = true)]
            public int Id2 { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Where()
        {
            var sql = delete.Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] = 1)", sql);

            sql = delete.Where("id = @id", new { id = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE (id = @id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = delete.Where(item).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });

            sql = delete.Where(items).ToSql().Replace("\r\n", "");
            Assert.Equal("DELETE FROM [tb_topic22211] WHERE ([Id] IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

            var id = g.sqlserver.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteIdentity();
            Assert.Equal(1, delete.Where(a => a.Id == id).ExecuteAffrows());
        }
        [Fact]
        public void ExecuteDeleted()
        {

            var item = g.sqlserver.Insert<Topic>(new Topic { Title = "xxxx", CreateTime = DateTime.Now }).ExecuteInserted();
            Assert.Equal(item[0].Id, delete.Where(a => a.Id == item[0].Id).ExecuteDeleted()[0].Id);

            var items = Enumerable.Range(0, 301).Select(a => new Topic { Title = "xxxx" + a, CreateTime = DateTime.Now }).ToArray();
            var itemsInserted = g.sqlserver.Insert(items).ExecuteInserted();
            Assert.Equal(items.First().Title, itemsInserted[0].Title);

            Assert.Equal(itemsInserted[0].Id, delete.Where(a => a.Id == itemsInserted[0].Id).ExecuteDeleted()[0].Id);
        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.sqlserver.Delete<Topic>().ToSql());
            var sql = g.sqlserver.Delete<Topic>(new[] { 1, 2 }).AsTable(a => "tb_topic22211AsTable").ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211AsTable] WHERE ([Id] IN (1,2))", sql);

            sql = g.sqlserver.Delete<Topic>(new Topic { Id = 1, Title = "test" }).AsTable(a => "tb_topic22211AsTable").ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211AsTable] WHERE ([Id] = 1)", sql);

            sql = g.sqlserver.Delete<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).AsTable(a => "tb_topic22211AsTable").ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211AsTable] WHERE ([Id] IN (1,2))", sql);

            sql = g.sqlserver.Delete<Topic>(new { id = 1 }).AsTable(a => "tb_topic22211AsTable").ToSql();
            Assert.Equal("DELETE FROM [tb_topic22211AsTable] WHERE ([Id] = 1)", sql);
        }
    }
}
