using System;
using System.Diagnostics;

namespace EntityFrameworkCore.Serialization.Compact
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class CompactPropertyEntry
    {
        public int    Index { get; set; }
        public object Value { get; set; }

        private string DebuggerDisplay ( ) => $"{ Index }: { Value }";
    }
}