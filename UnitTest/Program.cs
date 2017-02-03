using System;

namespace UnitTest
{
    class TableDef
    {
        public int Id;
        public int Level;
        public string Name;

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Id, Level, Name);
        }
    }


    class Program
    {
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


        static void Main(string[] args)
        {
            Test2Condition();

            TestSortLimit();

            TestShowAll();

            TestGenIndex();
        }
    }
}
