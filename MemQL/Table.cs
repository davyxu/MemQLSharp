using System;
using System.Collections.Generic;
using System.Text;

namespace MemQL
{
    public class Table
    {
        Dictionary<string, TableField> fieldByName = new Dictionary<string,TableField>();
        List<TableField> fields = new List<TableField>();

        public Table( Type userStructType )
        {
            // 遍历表格结构体字段, 生成字段
            foreach( var fd in userStructType.GetFields() )
            {
                var tf = new TableField(fd.FieldType);
                fields.Add(tf);
                fieldByName.Add(fd.Name, tf );
            }
        }

        // 添加一行数据
        public void AddRecord( object record )
        {
            var recordType = record.GetType();
            int index = 0;

            // 根据字段进行索引
            foreach (var fd in recordType.GetFields())
            {
                var recordField = fields[index];

                // 结构体中数值
                var key = fd.GetValue(record);

                // 将数值添加到字段索引中, 同一个值可能有多个, 引用记录集合
                recordField.Add(key, record);
                index++;
            }
        }

        internal TableField FieldByName( string name )
        {
            TableField tf;
            if (fieldByName.TryGetValue( name, out tf ) )
            {
                return tf;
            }

            return null;
        }

        internal TableField FieldByIndex( int index )
        {
            return fields[index];
        }

        public int FieldCount
        {
            get { return fields.Count; }
        }

        public void GenFieldIndex( string name, string matchTypeStr, int begin, int end)
        {
            if (FieldCount == 0)
                return;

            if (FieldByIndex(0).KeyCount == 0 )
            {
                throw new Exception("Require table data to gen index");
            }

            var field = FieldByName(name);
            if (fields == null )
            {
                throw new Exception("Field not found:" + name);
            }

            var matchType = MatchTypeHelper.Parse(matchTypeStr);
            if (matchType == MatchType.Unknown)
            {
                throw new Exception("Unknown match type: " + matchTypeStr.ToString());
            }

            // 遍历实际访问的数值
            for( int i = begin; i <= end;i++)
            {
                var indexList = new List<object>();

                switch( matchType )
                {
                    case MatchType.NotEqual:
                        {
                            for (int j = i;j<=end;j++)
                            {
                                if (j == i)
                                    continue;

                                var list = field.GetByKey(j, field.FieldType);
                                indexList.AddRange(list);
                            }                            
                        }
                        break;
                    case MatchType.Great:
                        {                            
                            // 大于当前值的所有列表合并
                            for (int j = i + 1; j <= end; j++)
                            {
                                var list = field.GetByKey(j, field.FieldType);
                                indexList.AddRange(list);
                            }
                         
                        }
                        break;
                    case MatchType.GreatEqual:
                        {                            
                            // 大于等于当前值的所有列表合并
                            for (int j = i; j <= end; j++)
                            {
                                var list = field.GetByKey(j, field.FieldType);
                                indexList.AddRange(list);
                            }                         
                        }
                        break;
                    case MatchType.Less:
                        {                               
                            for (int j = begin; j < i; j++)
                            {
                                var list = field.GetByKey(j, field.FieldType);
                                indexList.AddRange(list);
                            }
                        }
                        break;
                    case MatchType.LessEqual:
                        {                            
                            for (int j = begin; j <= i; j++)
                            {
                                var list = field.GetByKey(j, field.FieldType);
                                indexList.AddRange(list);
                            }
                        }
                        break;
                }

                field.AddIndexData(matchType, i, indexList);
            }
                
        }

    }
}
