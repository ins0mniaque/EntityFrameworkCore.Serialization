using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Dictionary
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class Entry
    {
        public string      EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        public Dictionary < string, object? > Properties         { get; set; }
        public Dictionary < string, object? > ModifiedProperties { get; set; }
        public HashSet    < string >          NavigationState    { get; set; }

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", Properties?.Select ( property => property.Value ) ) }";
    }
}