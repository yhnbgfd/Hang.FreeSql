﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using Xunit;

namespace FreeSql.Tests.OracleOledb
{
    public class OracleOledbUpdateTest
    {
        IUpdate<Topic> update => g.oracle.Update<Topic>();

        [Table(Name = "tb_topic")]
        class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int? Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        [Fact]
        public void Dywhere()
        {
            Assert.Null(g.oracle.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='test' \r\nWHERE (\"ID\" IN (1,2))", g.oracle.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.oracle.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='test1' \r\nWHERE (\"ID\" IN (1,2))", g.oracle.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").ToSql());
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.oracle.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").ToSql());
        }

        [Fact]
        public void SetSource()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = :p_0, \"TITLE\" = :p_1, \"CREATETIME\" = :p_2 WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            items[0].Clicks = null;

            sql = update.SetSource(items).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = CASE \"ID\" WHEN 1 THEN :p_0 WHEN 2 THEN :p_1 WHEN 3 THEN :p_2 WHEN 4 THEN :p_3 WHEN 5 THEN :p_4 WHEN 6 THEN :p_5 WHEN 7 THEN :p_6 WHEN 8 THEN :p_7 WHEN 9 THEN :p_8 WHEN 10 THEN :p_9 END, \"TITLE\" = CASE \"ID\" WHEN 1 THEN :p_10 WHEN 2 THEN :p_11 WHEN 3 THEN :p_12 WHEN 4 THEN :p_13 WHEN 5 THEN :p_14 WHEN 6 THEN :p_15 WHEN 7 THEN :p_16 WHEN 8 THEN :p_17 WHEN 9 THEN :p_18 WHEN 10 THEN :p_19 END, \"CREATETIME\" = CASE \"ID\" WHEN 1 THEN :p_20 WHEN 2 THEN :p_21 WHEN 3 THEN :p_22 WHEN 4 THEN :p_23 WHEN 5 THEN :p_24 WHEN 6 THEN :p_25 WHEN 7 THEN :p_26 WHEN 8 THEN :p_27 WHEN 9 THEN :p_28 WHEN 10 THEN :p_29 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"TITLE\" = CASE \"ID\" WHEN 1 THEN :p_0 WHEN 2 THEN :p_1 WHEN 3 THEN :p_2 WHEN 4 THEN :p_3 WHEN 5 THEN :p_4 WHEN 6 THEN :p_5 WHEN 7 THEN :p_6 WHEN 8 THEN :p_7 WHEN 9 THEN :p_8 WHEN 10 THEN :p_9 END WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = update.SetSource(items).Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CREATETIME\" = :p_0 WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);

            sql = g.oracle.Update<ts_source_mpk>().SetSource(new[] {
                new ts_source_mpk { id1 = 1, id2 = 7, xx = "a1" },
                new ts_source_mpk { id1 = 1, id2 = 8, xx = "b122" }
            }).NoneParameter().ToSql().Replace("\r\n", "");

            var uuids = new[]
            {
                new tssi01{tint = 1, title = "title01"},
                new tssi01{tint = 2, title = "title02"},
                new tssi01{tint = 3, title = "title03"},
            };
            g.oracle.Insert(uuids).ExecuteAffrows();
            g.oracle.Update<tssi01>().SetSource(uuids).ExecuteAffrows();

            var tssi01tb = g.oracle.DbFirst.GetTableByName("tssi01");
        }
        public class ts_source_mpk
        {
            [Column(IsPrimary = true)]
            public int id1 { get; set; }
            [Column(IsPrimary = true)]
            public int id2 { get; set; }
            public string xx { get; set; }
        }
        [Fact]
        public void SetSourceIgnore()
        {
            Assert.Equal("UPDATE \"TSSI01\" SET \"TINT\" = 10 WHERE (\"ID\" = '00000000-0000-0000-0000-000000000000')",
                g.oracle.Update<tssi01>().NoneParameter()
                    .SetSourceIgnore(new tssi01 { id = Guid.Empty, tint = 10 }, col => col == null).ToSql().Replace("\r\n", ""));
        }
        public class tssi01
        {
            [Column(CanUpdate = false, DbType = "nvarchar2(36)")]
            public Guid id { get; set; }
            public int tint { get; set; }
            public string title { get; set; }
        }
        [Fact]
        public void IgnoreColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).IgnoreColumns(a => new { a.Clicks, a.CreateTime }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"TITLE\" = :p_0 WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void UpdateColumns()
        {
            var sql = update.SetSource(new Topic { Id = 1, Title = "newtitle" }).UpdateColumns(a => a.Title).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"TITLE\" = :p_0 WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void Set()
        {
            var sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"TITLE\" = :p_0 WHERE (\"ID\" = 1)", sql);

            sql = update.Where(a => a.Id == 1).Set(a => a.Title, "newtitle").Set(a => a.CreateTime, new DateTime(2020, 1, 1)).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"TITLE\" = :p_0, \"CREATETIME\" = :p_1 WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = trunc(nvl(\"CLICKS\", 0) * 10 / 1) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id - 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"ID\" = (\"ID\" - 10) WHERE (\"ID\" = 1)", sql);

            int incrv = 10;
            sql = update.Set(a => a.Clicks * incrv / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = trunc(nvl(\"CLICKS\", 0) * 10 / 1) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id - incrv).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"ID\" = (\"ID\" - 10) WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks == a.Clicks * 10 / 1).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = trunc(\"CLICKS\" * 10 / 1) WHERE (\"ID\" = 1)", sql);

            var dt2000 = DateTime.Parse("2000-01-01");
            sql = update.Set(a => a.Clicks == (a.CreateTime > dt2000 ? 1 : 2)).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = case when \"CREATETIME\" > to_timestamp('2000-01-01 00:00:00.000000','YYYY-MM-DD HH24:MI:SS.FF6') then 1 else 2 end WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Id == 10).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"ID\" = 10 WHERE (\"ID\" = 1)", sql);

            sql = update.Set(a => a.Clicks == null).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = NULL WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void SetRaw()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("clicks = clicks + :incrClick", new { incrClick = 1 }).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET clicks = clicks + :incrClick WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void SetDto()
        {
            var sql = update.SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = :p_0, \"TITLE\" = :p_1 WHERE (\"ID\" = 1)", sql);
            sql = update.NoneParameter().SetDto(new { clicks = 1, title = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = 1, \"TITLE\" = 'xxx' WHERE (\"ID\" = 1)", sql);

            sql = update.SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = :p_0, \"TITLE\" = :p_1 WHERE (\"ID\" = 1)", sql);
            sql = update.NoneParameter().SetDto(new Dictionary<string, object> { ["clicks"] = 1, ["title"] = "xxx" }).Where(a => a.Id == 1).ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET \"CLICKS\" = 1, \"TITLE\" = 'xxx' WHERE (\"ID\" = 1)", sql);
        }
        [Fact]
        public void Where()
        {
            var sql = update.Where(a => a.Id == 1).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='newtitle' WHERE (\"ID\" = 1)", sql);

            sql = update.Where("id = :id", new { id = 1 }).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='newtitle' WHERE (id = :id)", sql);

            var item = new Topic { Id = 1, Title = "newtitle" };
            sql = update.Where(item).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='newtitle' WHERE (\"ID\" = 1)", sql);

            var items = new List<Topic>();
            for (var a = 0; a < 10; a++) items.Add(new Topic { Id = a + 1, Title = $"newtitle{a}", Clicks = a * 100 });
            sql = update.Where(items).SetRaw("title='newtitle'").ToSql().Replace("\r\n", "");
            Assert.Equal("UPDATE \"TB_TOPIC\" SET title='newtitle' WHERE (\"ID\" IN (1,2,3,4,5,6,7,8,9,10))", sql);
        }
        [Fact]
        public void ExecuteAffrows()
        {

        }
        [Fact]
        public void ExecuteUpdated()
        {

        }

        [Fact]
        public void AsTable()
        {
            Assert.Null(g.oracle.Update<Topic>().ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test' \r\nWHERE (\"ID\" IN (1,2))", g.oracle.Update<Topic>(new[] { 1, 2 }).SetRaw("title='test'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.oracle.Update<Topic>(new Topic { Id = 1, Title = "test" }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" IN (1,2))", g.oracle.Update<Topic>(new[] { new Topic { Id = 1, Title = "test" }, new Topic { Id = 2, Title = "test" } }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
            Assert.Equal("UPDATE \"TB_TOPICASTABLE\" SET title='test1' \r\nWHERE (\"ID\" = 1)", g.oracle.Update<Topic>(new { id = 1 }).SetRaw("title='test1'").AsTable(a => "tb_topicAsTable").ToSql());
        }
    }
}
