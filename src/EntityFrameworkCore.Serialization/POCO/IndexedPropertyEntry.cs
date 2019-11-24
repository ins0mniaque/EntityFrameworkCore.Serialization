using System;
using System.Diagnostics;

namespace EntityFrameworkCore.Serialization.POCO
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class IndexedPropertyEntry
    {
        public int    Index { get; set; }
        public object Value { get; set; }

        private string DebuggerDisplay ( ) => $"{ Index }: { Value }";
    }
}