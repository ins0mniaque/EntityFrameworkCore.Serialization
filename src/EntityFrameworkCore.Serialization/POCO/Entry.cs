using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.POCO
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class Entry
    {
        public string      EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        public PropertyEntry [ ] Properties         { get; set; }
        public PropertyEntry [ ] ModifiedProperties { get; set; }
        public string        [ ] LoadedCollections  { get; set; }

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", Properties.Select ( pk => pk.Value ) ) }";
    }
}