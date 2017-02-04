using System;
using System.Collections.Generic;

namespace MemQL
{
    class mergeData
    {
        public int Count; // data在查询field中重复的次数
        public object Data;
    }

    struct Condition
    {
        public TableField Field;
        public MatchType Type;
        public object Value;
    }

    

    public class Query
    {
        Dictionary<object, mergeData> mergeDataByData = new Dictionary<object,mergeData>();
        List<Condition> conditions = new List<Condition>();
        Dictionary<object, object> result = new Dictionary<object, object>();
        int limit = -1;
        Table tab;
        bool done;        
        Comparison<object> sortor = null;

        public Query( Table tab )
        {
            this.tab = tab;
        }

        public void Reset( )
        {
            done = false;
            sortor = null;
            limit = -1;
            conditions.Clear();
            result.Clear();
            mergeDataByData.Clear();
        }



        public Query Where( string fieldName, string matchTypeStr, object value )
        {
            var matchType = MatchTypeHelper.Parse(matchTypeStr);
            if (matchType == MatchType.Unknown)
            {
                throw new Exception("Unknown match type: " + matchTypeStr.ToString());
            }

            var con = new Condition
            {
                Field = tab.FieldByName( fieldName ),
                Type = matchType, 
                Value= value,
            };

            if ( con.Field == null )
            {
                throw new Exception("Field not found:" + fieldName);
            }

            conditions.Add(con);

            return this;

        }

        public Query Limit( int count )
        {
            if ( count < 0 )
            {
                throw new Exception("Count should > 0");
            }

            this.limit = count;

            return this;
        }        

        public Query SortBy( Comparison<object> callback )
        {
            sortor = callback;

            return this;
        }

        // 添加数据, 自动去重, 生成结果
        internal void Add(object data)
        {
            mergeData md;
            if (!mergeDataByData.TryGetValue(data, out md))
            {
                md = new mergeData();
                md.Data = data;
                mergeDataByData[data] = md;
            }

            md.Count++;

            // 求叉集
            if (md.Count >= conditions.Count)
            {
                result[data] = md.Data;
            }
        }

        void Do( )
        {
            if (done)
                return;

            // 结构体没有字段
            if (tab.FieldCount > 0 )
            {
                // 没有任何条件约束
                if (conditions.Count == 0)
                {
                    // 返回所有
                    tab.FieldByIndex(0).All(this);
                }
                else
                {
                    // 根据条件匹配
                    for( int i = 0;i< conditions.Count;i++)
                    {
                        var con = conditions[i];
                        con.Field.Match(this, con.Type, con.Value);
                    }
                }
            }

            done = true;
        }

        public List<object> Result( )
        {
            Do();
          
            List<object> ret = new List<object>();
                        
            foreach( var kv in result )
            {                                
                ret.Add(kv.Value);
            }

            if (sortor != null )
            {
                ret.Sort(sortor);
            }

            if ( this.limit !=-1 && this.limit < result.Count )
            {
                ret.RemoveRange(this.limit, ret.Count - this.limit );
            }

            return ret;
            
        }

        public delegate bool RawResultCallback(object v);

        public void VisitRawResult( RawResultCallback callback )
        {
            Do();

            if (callback == null)
                return;

            foreach( var kv in result )
            {
                if ( !callback( kv.Value ) )
                {
                    return;
                }
            }
        }
        
    }
}
