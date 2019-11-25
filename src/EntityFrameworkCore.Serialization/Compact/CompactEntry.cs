using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Compact
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class CompactEntry
    {
        public string      EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        public CompactPropertyEntry [ ] Properties         { get; set; }
        public CompactPropertyEntry [ ] ModifiedProperties { get; set; }
        public int                  [ ] LoadedCollections  { get; set; }

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", Properties.Select ( p => p.Value ) ) }";
    }
}