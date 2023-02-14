﻿using FreeSql.DatabaseModel;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Console = Colorful.Console;

namespace FreeSql.Generator
{
    class ConsoleApp
    {
        string ArgsRazorRaw { get; }
        string ArgsRazor { get; }
        bool[] ArgsNameOptions { get; }
        string ArgsNameSpace { get; }
        DataType ArgsDbType { get; }
        string ArgsConnectionString { get; }
        string ArgsFilter { get; }
        string ArgsMatch { get; }
        string ArgsJson { get; }
        string ArgsFileName { get; }
        bool ArgsReadKey { get; }
        internal string ArgsOutput { get; private set; }

        public ConsoleApp(string[] args, ManualResetEvent wait)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var gb2312 = Encoding.GetEncoding("GB2312");
            //if (gb2312 != null)
            //{
            //    try
            //    {
            //        Console.OutputEncoding = gb2312;
            //        Console.InputEncoding = gb2312;
            //    }
            //    catch { }
            //}

            //var ntjson = Assembly.LoadFile(@"C:\Users\28810\Desktop\testfreesql\bin\Debug\netcoreapp2.2\publish\testfreesql.dll");

            //using (var gen = new Generator(new GeneratorOptions()))
            //{
            //    gen.TraceLog = log => Console.WriteFormatted(log + "\r\n", Color.DarkGray);
            //    gen.Build(ArgsOutput, new[] { typeof(ojbk.Entities.AuthRole) }, false);
            //}

            var version = "v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2));
            Console.WriteAscii(" FreeSql", Color.Violet);
            Console.WriteFormatted(@"
  # Github # {0} {1}
", Color.SlateGray,
new Colorful.Formatter("https://github.com/2881099/FreeSql", Color.DeepSkyBlue),
new Colorful.Formatter("v" + string.Join(".", typeof(ConsoleApp).Assembly.GetName().Version.ToString().Split('.').Where((a, b) => b <= 2)), Color.SlateGray));

            ArgsRazorRaw = "1";
            ArgsRazor = RazorContentManager.实体类_特性_cshtml;
            ArgsNameOptions = new[] { false, false, false, false };
            ArgsNameSpace = "MyProject";
            ArgsFilter = "";
            ArgsMatch = "";
            ArgsJson = "Newtonsoft.Json";
            ArgsFileName = "{name}.cs";
            ArgsReadKey = true;
            Action<string> setArgsOutput = value =>
            {
                ArgsOutput = value;
                ArgsOutput = ArgsOutput.Trim().TrimEnd('/', '\\');
                ArgsOutput += ArgsOutput.Contains("\\") ? "\\" : "/";
                if (!Directory.Exists(ArgsOutput))
                    Directory.CreateDirectory(ArgsOutput);
            };
            setArgsOutput(Directory.GetCurrentDirectory());

            string args0 = args[0].Trim().ToLower();
            if (args[0] == "?" || args0 == "--help" || args0 == "-help")
            {

                Console.WriteFormatted(@"
    {0}

    更新工具：dotnet tool update -g FreeSql.Generator


  # 快速开始 #

  > {1} {2} 1 {3} 0,0,0,0 {4} MyProject {5} ""MySql,Data Source=127.0.0.1;...""

     -Razor 1                  * 选择模板：实体类+特性
     -Razor 2                  * 选择模板：实体类+特性+导航属性
     -Razor ""d:\diy.cshtml""    * 自定义模板文件，如乱码请修改为UTF8(不带BOM)编码格式

     -NameOptions              * 4个布尔值对应：
                                 首字母大写
                                 首字母大写，其他小写
                                 全部小写
                                 下划线转驼峰

     -NameSpace                * 命名空间

     -DB ""{6},data source=127.0.0.1;port=3306;user id=root;password=root;initial catalog=数据库;charset=utf8;sslmode=none;max pool size=2""
     -DB ""{7},data source=.;integrated security=True;initial catalog=数据库;pooling=true;max pool size=2""
     -DB ""{8},host=192.168.164.10;port=5432;username=postgres;password=123456;database=数据库;pooling=true;maximum pool size=2""
     -DB ""{9},user id=user1;password=123456;data source=//127.0.0.1:1521/XE;pooling=true;max pool size=2""
     -DB ""{10},data source=document.db""
     -DB ""{14},database=localhost:D:\fbdata\EXAMPLES.fdb;user=sysdba;password=123456;max pool size=2""
     -DB ""{11},server=127.0.0.1;port=5236;user id=2user;password=123456789;database=2user;poolsize=2""
     -DB ""{12},server=127.0.0.1;port=54321;uid=USER2;pwd=123456789;database=数据库""
     -DB ""{13},host=192.168.164.10;port=2003;database=数据库;username=SYSDBA;password=szoscar55;maxpoolsize=2""
                               * {11}(达梦数据库)、{12}(人大金仓数据库)、{13}(神舟通用数据库)

     -Filter                   Table+View+StoreProcedure
                               默认生成：表+视图+存储过程
                               如果不想生成视图和存储过程 -Filter View+StoreProcedure

     -Match                    表名或正则表达式，只生成匹配的表，如：dbo\.TB_.+
     -Json                     NTJ、STJ、NONE
                               Newtonsoft.Json、System.Text.Json、不生成

     -FileName                 文件名，默认：{name}.cs
     -Output                   保存路径，默认为当前 shell 所在目录
                               {15}

", Color.SlateGray,
new Colorful.Formatter("FreeSql 快速生成数据库的实体类", Color.SlateGray),
new Colorful.Formatter("FreeSql.Generator", Color.White),
new Colorful.Formatter("-Razor", Color.ForestGreen),
new Colorful.Formatter("-NameOptions", Color.ForestGreen),
new Colorful.Formatter("-NameSpace", Color.ForestGreen),
new Colorful.Formatter("-DB", Color.ForestGreen),
new Colorful.Formatter("MySql", Color.Yellow),
new Colorful.Formatter("SqlServer", Color.Yellow),
new Colorful.Formatter("PostgreSQL", Color.Yellow),
new Colorful.Formatter("Oracle", Color.Yellow),
new Colorful.Formatter("Sqlite", Color.Yellow),
new Colorful.Formatter("Dameng", Color.Yellow),
new Colorful.Formatter("KingbaseES", Color.Yellow),
new Colorful.Formatter("ShenTong", Color.Yellow),
new Colorful.Formatter("Firebird", Color.Yellow),
new Colorful.Formatter("推荐在实体类目录创建 gen.bat，双击它重新所有实体类", Color.ForestGreen)
);
                wait.Set();
                return;
            }
            for (int a = 0; a < args.Length; a++)
            {
                switch (args[a].Trim().ToLower())
                {
                    case "-razor":
                        ArgsRazorRaw = args[a + 1].Trim();
                        switch (ArgsRazorRaw)
                        {
                            case "1": ArgsRazor = RazorContentManager.实体类_特性_cshtml; break;
                            case "2": ArgsRazor = RazorContentManager.实体类_特性_导航属性_cshtml; break;
                            default: ArgsRazor = File.ReadAllText(args[a + 1], Encoding.UTF8); break;
                        }
                        a++;
                        break;

                    case "-nameoptions":
                        ArgsNameOptions = args[a + 1].Split(',').Select(opt => opt == "1").ToArray();
                        if (ArgsNameOptions.Length != 4) throw new ArgumentException(CoreStrings.S_NameOptions_Incorrect);
                        a++;
                        break;
                    case "-namespace":
                        ArgsNameSpace = args[a + 1];
                        a++;
                        break;
                    case "-db":
                        var dbargs = args[a + 1].Split(',', 2);
                        if (dbargs.Length != 2) throw new ArgumentException(CoreStrings.S_DB_ParameterError);

                        switch (dbargs[0].Trim().ToLower())
                        {
                            case "mysql": ArgsDbType = DataType.MySql; break;
                            case "sqlserver": ArgsDbType = DataType.SqlServer; break;
                            case "postgresql": ArgsDbType = DataType.PostgreSQL; break;
                            case "oracle": ArgsDbType = DataType.Oracle; break;
                            case "sqlite": ArgsDbType = DataType.Sqlite; break;
                            case "firebird": ArgsDbType = DataType.Firebird; break;
                            case "dameng": ArgsDbType = DataType.Dameng; break;
                            case "kingbasees": ArgsDbType = DataType.KingbaseES; break;
                            case "shentong": ArgsDbType = DataType.ShenTong; break;
                            case "clickhouse": ArgsDbType = DataType.ClickHouse; break;
                            default: throw new ArgumentException(CoreStrings.S_DB_ParameterError_UnsupportedType(dbargs[0]));
                        }
                        ArgsConnectionString = dbargs[1].Trim();
                        a++;
                        break;
                    case "-filter":
                        ArgsFilter = args[a + 1];
                        a++;
                        break;
                    case "-match":
                        ArgsMatch = args[a + 1];
                        if (Regex.IsMatch("", ArgsMatch)) { } //throw
                        a++;
                        break;
                    case "-json":
                        switch(args[a + 1].Trim().ToLower())
                        {
                            case "none":
                                ArgsJson = "";
                                break;
                            case "stj":
                                ArgsJson = "System.Text.Json";
                                break;
                        }
                        a++;
                        break;
                    case "-filename":
                        ArgsFileName = args[a + 1];
                        a++;
                        break;
                    case "-readkey":
                        ArgsReadKey = args[a + 1].Trim() == "1";
                        a++;
                        break;
                    case "-output":
                        setArgsOutput(args[a + 1]);
                        a++;
                        break;
                    default:
                        throw new ArgumentException(CoreStrings.S_WrongParameter(args[a]));
                }
            }

            if (string.IsNullOrEmpty(ArgsConnectionString)) throw new ArgumentException(CoreStrings.S_DB_Parameter_Error_NoConnectionString);

            RazorEngine.Engine.Razor = RazorEngineService.Create(new RazorEngine.Configuration.TemplateServiceConfiguration
            {
                EncodedStringFactory = new RazorEngine.Text.RawStringFactory() // Raw string encoding.
            });
            var razorId = Guid.NewGuid().ToString("N");
            RazorEngine.Engine.Razor.Compile(ArgsRazor, razorId);

            var outputCounter = 0;
            using (IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(ArgsDbType, ArgsConnectionString)
                .UseAutoSyncStructure(false)
                .UseMonitorCommand(cmd => Console.WriteFormatted(cmd.CommandText + "\r\n", Color.SlateGray))
                .Build())
            {
                List<DbTableInfo> tables = new List<DbTableInfo>();
                if (string.IsNullOrEmpty(ArgsMatch) == false)
                {
                    try
                    {
                        var matchTable = fsql.DbFirst.GetTableByName(ArgsMatch);
                        if (matchTable != null) tables.Add(matchTable);
                    }
                    catch { }
                }
                if (tables.Any() == false)
                    tables = fsql.DbFirst.GetTablesByDatabase();
                var outputTables = tables;

                //开始生成操作
                foreach (var table in outputTables)
                {
                    if (string.IsNullOrEmpty(ArgsMatch) == false)
                    {
                        if (Regex.IsMatch($"{table.Schema}.{table.Name}".TrimStart('.'), ArgsMatch) == false) continue;
                    }
                    switch (table.Type)
                    {
                        case DatabaseModel.DbTableType.TABLE:
                            if (ArgsFilter.Contains("Table", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteFormatted(" Ignore Table -> " + table.Name + "\r\n", Color.DarkSlateGray);
                                continue;
                            }
                            break;
                        case DatabaseModel.DbTableType.VIEW:
                            if (ArgsFilter.Contains("View", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteFormatted(" Ignore View -> " + table.Name + "\r\n", Color.DarkSlateGray);
                                continue;
                            }
                            break;
                        case DatabaseModel.DbTableType.StoreProcedure:
                            if (ArgsFilter.Contains("StoreProcedure", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteFormatted(" Ignore StoreProcedure -> " + table.Name + "\r\n", Color.DarkSlateGray);
                                continue;
                            }
                            break;
                    }
                    var sw = new StringWriter();
                    var model = new RazorModel(fsql, ArgsNameSpace, ArgsNameOptions, tables, table);
                    RazorEngine.Engine.Razor.Run(razorId, sw, null, model);

                    StringBuilder plus = new StringBuilder();
                    //plus.AppendLine("//------------------------------------------------------------------------------");
                    //plus.AppendLine("// <auto-generated>");
                    //plus.AppendLine("//     此代码由工具 FreeSql.Generator 生成。");
                    //plus.AppendLine("//     运行时版本:" + Environment.Version.ToString());
                    //plus.AppendLine("//     Website: https://github.com/2881099/FreeSql");
                    //plus.AppendLine("//     对此文件的更改可能会导致不正确的行为，并且如果");
                    //plus.AppendLine("//     重新生成代码，这些更改将会丢失。");
                    //plus.AppendLine("// </auto-generated>");
                    //plus.AppendLine("//------------------------------------------------------------------------------");
                    plus.Append(sw.ToString());
                    plus.AppendLine();

                    var outputFile = $"{ArgsOutput}{ArgsFileName.Replace("{name}", model.GetCsName(table.Name))}";
                    File.WriteAllText(outputFile, plus.ToString(), Encoding.UTF8);
                    switch (table.Type)
                    {
                        case DatabaseModel.DbTableType.TABLE:
                            Console.WriteFormatted(" OUT Table -> " + outputFile + "\r\n", Color.DeepSkyBlue);
                            break;
                        case DatabaseModel.DbTableType.VIEW:
                            Console.WriteFormatted(" OUT View -> " + outputFile + "\r\n", Color.DeepSkyBlue);
                            break;
                        case DatabaseModel.DbTableType.StoreProcedure:
                            Console.WriteFormatted(" OUT StoreProcedure -> " + outputFile + "\r\n", Color.DeepSkyBlue);
                            break;
                    }
                    ++outputCounter;
                }
            }

            var rebuildBat = ArgsOutput + "__重新生成.bat";
            if (File.Exists(rebuildBat) == false)
            {
                var razorCshtml = ArgsOutput + "__razor.cshtml.txt";
                if (File.Exists(razorCshtml) == false)
                {
                    File.WriteAllText(razorCshtml, ArgsRazor, Encoding.UTF8);
                    Console.WriteFormatted(" OUT -> " + razorCshtml + "    (以后) 编辑它自定义模板生成\r\n", Color.Magenta);
                    ++outputCounter;
                }

                File.WriteAllText(rebuildBat, $@"
FreeSql.Generator -Razor ""__razor.cshtml.txt"" -NameOptions {string.Join(",", ArgsNameOptions.Select(a => a ? 1 : 0))} -NameSpace {ArgsNameSpace} -DB ""{ArgsDbType},{ArgsConnectionString}""{(string.IsNullOrEmpty(ArgsFilter) ? "" : $" -Filter \"{ArgsFilter}\"")}{(string.IsNullOrEmpty(ArgsMatch) ? "" : $" -Match \"{ArgsMatch}\"")} -FileName ""{ArgsFileName}""
", Encoding.UTF8);
                Console.WriteFormatted(" OUT -> " + rebuildBat + "    (以后) 双击它重新生成实体\r\n", Color.Magenta);
                ++outputCounter;
            }

            Console.WriteFormatted($"\r\n[{DateTime.Now.ToString("MM-dd HH:mm:ss")}] 生成完毕，总共生成了 {outputCounter} 个文件，目录：\"{ArgsOutput}\"\r\n", Color.DarkGreen);

            if (ArgsReadKey)
                Console.ReadKey();
            wait.Set();
        }
    }
}

