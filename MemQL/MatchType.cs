
using System.Collections.Generic;
namespace MemQL
{
    public enum MatchType
    {
        Unknown = 0,
        Equal,
        NotEqual,
        Great,
        GreatEqual,
        Less,
        LessEqual,
        MAX,
    }

    class MatchTypeHelper
    {
        static Dictionary<string, MatchType> str2type = new Dictionary<string, MatchType>
        {
            {"==", MatchType.Equal},
            {"!=", MatchType.NotEqual},
            {">", MatchType.Great},
            {">=", MatchType.GreatEqual},
            {"<", MatchType.Less},
            {"<=", MatchType.LessEqual},
        };        

        public MatchTypeHelper( )
        {

        }

        public static MatchType Parse( string str )
        {
            MatchType ret;
            if ( str2type.TryGetValue(str, out ret))
            {
                return ret;
            }

            return MatchType.Unknown;
        }
    }
}
