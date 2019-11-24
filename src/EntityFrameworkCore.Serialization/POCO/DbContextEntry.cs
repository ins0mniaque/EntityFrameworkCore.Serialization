﻿using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.POCO
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class DbContextEntry
    {
        public string      EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        public PropertyEntry [ ] PrimaryKey         { get; set; }
        public PropertyEntry [ ] ConcurrencyToken   { get; set; }
        public PropertyEntry [ ] Properties         { get; set; }
        public PropertyEntry [ ] ModifiedProperties { get; set; }

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", PrimaryKey.Select ( pk => pk.Value ) ) }";
    }
}