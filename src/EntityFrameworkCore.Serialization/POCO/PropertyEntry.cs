using System;
using System.Diagnostics;

namespace EntityFrameworkCore.Serialization.POCO
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class PropertyEntry
    {
        public string Name  { get; set; }
        public object Value { get; set; }

        private string DebuggerDisplay ( ) => $"{ Name }: { Value }";
    }
}