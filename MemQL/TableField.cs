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

        internal List<object> GetByKey( object key )
        {
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
                        var v = GetByKey(data);
                        if ( v != null )
                        {
                            AddListToResult(q, v );                        }
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
                        var vdata = (int)data;
                        foreach( var kv in equalMapper )
                        {
                            var key = (int)kv.Key;

                            switch (t)
                            {
                                case MatchType.Great:
                                    {
                                        if ( key > vdata )
                                        {
                                            AddListToResult(q, kv.Value);
                                        }
                                    }
                                    break;
                                case MatchType.GreatEqual:
                                    {
                                        if (key >= vdata)
                                        {
                                            AddListToResult(q, kv.Value);
                                        }
                                    }
                                    break;
                                case MatchType.Less:
                                    {
                                        if (key < vdata)
                                        {
                                            AddListToResult(q, kv.Value);
                                        }
                                    }
                                    break;
                                case MatchType.LessEqual:
                                    {
                                        if (key <= vdata)
                                        {
                                            AddListToResult(q, kv.Value);
                                        }
                                    }
                                    break;
                            }
                        }


                    }
                    break;
            }
        }

        bool MatchByIndex( Query q, MatchType t, object data )
        {
            if (etcMapper == null)
                return false;

            unequalData ud;
            if (etcMapper.TryGetValue( data, out ud ))
            {
                var typeList = ud.matchTypeList[(int)t];
                if ( typeList == null )
                {
                    throw new Exception("Match type index not built: " + t.ToString());
                }

                AddListToResult(q, typeList);
                    
            }


            return true;
        }

    }
}
