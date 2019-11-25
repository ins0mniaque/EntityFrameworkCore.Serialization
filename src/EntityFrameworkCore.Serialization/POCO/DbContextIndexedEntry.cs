using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.POCO
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class DbContextIndexedEntry
    {
        public string      EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        public IndexedPropertyEntry [ ] PrimaryKey         { get; set; }
        public IndexedPropertyEntry [ ] ConcurrencyToken   { get; set; }
        public IndexedPropertyEntry [ ] Properties         { get; set; }
        public IndexedPropertyEntry [ ] ModifiedProperties { get; set; }
        public int                  [ ] LoadedCollections  { get; set; }

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", PrimaryKey.Select ( pk => pk.Value ) ) }";
    }
}