using System;
using System.Collections.Generic;

namespace MemQL
{
    class unequalData
    {
        public List<object>[] matchTypeList = new List<object>[(int)MatchType.MAX];
    }

    internal class TableField
    {
        Dictionary<object, List<object>> equalMapper = new Dictionary<object,List<object>>();

        Dictionary<object, unequalData> etcMapper;

        Type fieldType;

        internal Type FieldType
        {
            get { return fieldType; }
        }

        public TableField( Type fd )
        {
            fieldType = fd;
        }

        internal void Add(object data, object refRecord)
        {
            List<object> recordList;
            if ( !equalMapper.TryGetValue(data, out recordList) )
            {
                recordList = new List<object>();
                equalMapper.Add(data, recordList);
            }

            recordList.Add(refRecord);
        }

        internal void AddIndexData(MatchType t, int key, List<object> list)
        {
            if ( etcMapper == null )
            {
                etcMapper = new Dictionary<object, unequalData>();
            }

            unequalData ud;
            if (!etcMapper.TryGetValue(key, out ud))
            {
                ud = new unequalData();
                etcMapper.Add(key, ud);
            }

            ud.matchTypeList[(int)t] = list;
        }

        public int KeyCount
        {
            get
            {
                return equalMapper.Count;
            }
        }

        internal List<object> GetByKey( object key, Type keyType )
        {
            // 如果字段是枚举, 将key(用户输入)转为枚举
            if ( keyType.IsEnum )
            {
                key = Enum.ToObject(keyType, key);
            }

            List<object> ret;
            if (equalMapper.TryGetValue( key, out ret ))
            {
                return ret;
            }

            return null;
        }

        void AddListToResult( Query q, List<object> list )
        {
            foreach( var v in list )
            {
                q.Add(v);
            }
        }

        internal void All(Query q)
        {
            foreach( var kv in equalMapper )
            {
                AddListToResult(q, kv.Value);
            }
        }

        // 向结果集添加符合条件的数据
        internal void Match( Query q, MatchType t, object data )
        {
            switch (t)
            {
                case MatchType.Equal:
                    {
                        var v = GetByKey(data, this.fieldType );
                        if ( v != null )
                        {
                            AddListToResult(q, v );
                        }
                    }
                    break;
                case MatchType.NotEqual:
                    {
                        if ( !MatchByIndex( q, t, data ))
                        {
                            foreach( var kv in equalMapper )
                            {
                                if (kv.Key != data )
                                {
                                    AddListToResult(q, kv.Value);
                                }
                            }
                        }
                    }
                    break;
                default:
                    {
                        // 使用索引过的数据
                        if (MatchByIndex( q, t, data ))
                        {
                            return;
                        }

                        // 暴力匹配                        
                        foreach( var kv in equalMapper )
                        {
                            if (Compare(t, kv.Key, data))
                            {
                                AddListToResult(q, kv.Value);
                            }
                        }


                    }
                    break;
            }
        }

        static bool Compare( MatchType t, object tabData, object userExpect )
        {
            if (tabData is Int32)
            {
                var tabDataT = (Int32)tabData;
                var userExpectT = (Int32)userExpect;
                
                switch( t )
                {
                    case MatchType.Less:
                        return tabDataT < userExpectT;
                    case MatchType.LessEqual:
                        return tabDataT <= userExpectT;
                    case MatchType.Great:
                        return tabDataT > userExpectT;
                    case MatchType.GreatEqual:
                        return tabDataT >= userExpectT;
                }
            }
            else if (tabData is Int64)
            {
                var tabDataT = (Int64)tabData;
                var userExpectT = (Int64)userExpect;

                switch (t)
                {
                    case MatchType.Less:
                        return tabDataT < userExpectT;
                    case MatchType.LessEqual:
                        return tabDataT <= userExpectT;
                    case MatchType.Great:
                        return tabDataT > userExpectT;
                    case MatchType.GreatEqual:
                        return tabDataT >= userExpectT;
                }
            }
            else if ( tabData.GetType().IsEnum )
            {
                var tabDataT = (Int32)tabData;
                var userExpectT = (Int32)userExpect;

                switch (t)
                {
                    case MatchType.Less:
                        return tabDataT < userExpectT;
                    case MatchType.LessEqual:
                        return tabDataT <= userExpectT;
                    case MatchType.Great:
                        return tabDataT > userExpectT;
                    case MatchType.GreatEqual:
                        return tabDataT >= userExpectT;
                }

            }

            return false;
        }

        bool MatchByIndex( Query q, MatchType t, object data )
        {
            if (etcMapper == null)
                return false;

            // 字段是枚举, 需要将外部的枚举转为整形
            if ( fieldType.IsEnum )
            {
                data = Convert.ToInt32(data);
            }

            unequalData ud;
            if (etcMapper.TryGetValue( data, out ud ))
            {
                var typeList = ud.matchTypeList[(int)t];
                if ( typeList == null )
                {
                    throw new Exception("Match type index not built: " + t.ToString());
                }

                AddListToResult(q, typeList);
                return true;                    
            }


            return false;
        }

    }
}
