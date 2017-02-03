# MemQLSharp

基于内存结构的多条件组合查询器

# 用途

* 内存表格数据查询

* 游戏触发器条件查询
成就表: 定义成就类型, 事件类型, 玩家等级等静态表格数据
通过本系统查出符合条件的集合, 再检查动态数据, 例如: 玩家拥有物品等

# 特性

* 支持结果数量约束(Limit)

* 支持结果排序(SortBy+自定义排序函数)

* 多字段任意组合查询

* 支持构建字段搜索索引, 提高不等匹配(!=, <,>...)查询性能, 从O(N*M)降低到O(1)

# 支持功能的等效SQL语法
select * from tableData where condition1 and condition2... limit count orderby xxx

```csharp
	    static MemQL.Table genTestTable()
        {
            var tabData = new TableDef[]{
                new TableDef{ Id= 6, Level = 20, Name = "kitty" },
                new TableDef{ Id= 1, Level = 50, Name = "hello" },
                new TableDef{ Id= 4, Level = 20, Name = "kitty" },
                new TableDef{ Id= 5, Level = 10, Name = "power" },
                new TableDef{ Id= 3, Level = 20, Name = "hello" },
                new TableDef{ Id= 2, Level = 20, Name = "kitty" },
            };

            var tab = new MemQL.Table( typeof(TableDef) );
            foreach(var v in tabData )
            {
                tab.AddRecord(v);
            }

            return tab;
        }

        // 2条件匹配查询
        static void Test2Condition()
        {
            Console.WriteLine("Test2Condition:");

            var tab = genTestTable();

            foreach (var v in new MemQL.Query(tab).Where("Level", "<", 50).Where("Name", "==", "hello").Result())
            {
                Console.WriteLine(v.ToString());
            }

            // Got  3 20 hello
        }

        // 1条件, 排序和数量限制
        static void TestSortLimit()
        {
            Console.WriteLine("TestSortLimit:");

            var tab = genTestTable();

            foreach (var v in new MemQL.Query(tab).Where("Level", "==", 20).SortBy( delegate ( object a, object b ) {
                var x = a as TableDef;
                var y = b as TableDef;

                return x.Id.CompareTo(y.Id);

            } ).Limit(3).Result())
            {
                Console.WriteLine(v.ToString());
            }

            /*
                Got
                3 20 hello
                4 20 kitty
                6 20 kitty
            */
        }

        // 直接访问结果,无缓存, 效率高, 但不能处理SortBy和Limit
        static void TestShowAll( )
        {
            Console.WriteLine("TestShowAll:");

            var tab = genTestTable();

            new MemQL.Query(tab).VisitRawResult(delegate(object v)
            {
                Console.WriteLine(v.ToString());

                return true;
            });
        }

        static void TestGenIndex()
        {
            Console.WriteLine("TestGenIndex:");

            var tab = genTestTable();

            tab.GenFieldIndex("Id", "!=", 1, 6);

            foreach (var v in new MemQL.Query(tab).Where("Id", "!=", 3).Result())
            {
                Console.WriteLine(v.ToString());
            }

            /* 
               Got
                4 20 kitty
	            5 10 power
	            6 20 kitty
            */
        }

```

# 其他版本

	golang版参见: https://github.com/davyxu/gomemql

# 备注

感觉不错请star, 谢谢!

博客: http://www.cppblog.com/sunicdavy

知乎: http://www.zhihu.com/people/sunicdavy

邮箱: sunicdavy@qq.com
