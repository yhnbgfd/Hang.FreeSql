using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.Custom.MySqlMapType
{
    public class EnumTest
    {
        class EnumTestMap
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(string))]
            public ToStringMapEnum enum_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public ToStringMapEnum? enumnullable_to_string { get; set; }

            [Column(MapType = typeof(int))]
            public ToStringMapEnum enum_to_int { get; set; }
            [Column(MapType = typeof(int?))]
            public ToStringMapEnum? enumnullable_to_int { get; set; }
        }
        public enum ToStringMapEnum { �й���, abc, ��� }
        [Fact]
        public void EnumToString()
        {
            //insert
            var orm = g.mysql;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.�й���, find.enum_to_string);

            item = new EnumTestMap { enum_to_string = ToStringMapEnum.abc };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //update all
            item.enum_to_string = ToStringMapEnum.���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.���, find.enum_to_string);

            item.enum_to_string = ToStringMapEnum.�й���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_string, find.enum_to_string);
            Assert.Equal(ToStringMapEnum.�й���, find.enum_to_string);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.���).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.���, find.enum_to_string);

            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_string);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.�й���).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.���).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_string == ToStringMapEnum.abc).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void EnumNullableToString()
        {
            //insert
            var orm = g.mysql;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            item = new EnumTestMap { enumnullable_to_string = ToStringMapEnum.�й��� };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.�й���, find.enumnullable_to_string);

            //update all
            item.enumnullable_to_string = ToStringMapEnum.���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Equal(ToStringMapEnum.���, find.enumnullable_to_string);

            item.enumnullable_to_string = null;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.���).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_string, find.enumnullable_to_string);
            Assert.Null(find.enumnullable_to_string);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enumnullable_to_string);


            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.abc).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.enumnullable_to_string);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.�й���).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == ToStringMapEnum.���).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void EnumToInt()
        {
            //insert
            var orm = g.mysql;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.�й���, find.enum_to_int);

            item = new EnumTestMap { enum_to_int = ToStringMapEnum.abc };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_int);

            //update all
            item.enum_to_int = ToStringMapEnum.���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.���, find.enum_to_int);

            item.enum_to_int = ToStringMapEnum.�й���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enum_to_int, find.enum_to_int);
            Assert.Equal(ToStringMapEnum.�й���, find.enum_to_int);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_int, ToStringMapEnum.���).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.���, find.enum_to_int);

            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enum_to_int, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enum_to_int);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.�й���).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.���).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enum_to_int == ToStringMapEnum.abc).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void EnumNullableToInt()
        {
            //insert
            var orm = g.mysql;
            var item = new EnumTestMap { };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Null(find.enumnullable_to_int);

            item = new EnumTestMap { enumnullable_to_int = ToStringMapEnum.�й��� };
            Assert.Equal(1, orm.Insert<EnumTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.�й���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Equal(ToStringMapEnum.�й���, find.enumnullable_to_int);

            //update all
            item.enumnullable_to_int = ToStringMapEnum.���;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.���).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Equal(ToStringMapEnum.���, find.enumnullable_to_int);

            item.enumnullable_to_int = null;
            Assert.Equal(1, orm.Update<EnumTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.���).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(item.enumnullable_to_int, find.enumnullable_to_int);
            Assert.Null(find.enumnullable_to_int);

            //update set
            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_int, ToStringMapEnum.abc).ExecuteAffrows());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.abc).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(ToStringMapEnum.abc, find.enumnullable_to_int);


            Assert.Equal(1, orm.Update<EnumTestMap>().Where(a => a.id == item.id).Set(a => a.enumnullable_to_int, null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.abc).First());
            find = orm.Select<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.enumnullable_to_int);

            //delete
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.�й���).ExecuteAffrows());
            Assert.Equal(0, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == ToStringMapEnum.���).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<EnumTestMap>().Where(a => a.id == item.id && a.enumnullable_to_int == null).ExecuteAffrows());
            Assert.Null(orm.Select<EnumTestMap>().Where(a => a.id == item.id).First());
        }
    }
}
