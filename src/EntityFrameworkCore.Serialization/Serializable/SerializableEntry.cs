using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Serializable
{
    [ DebuggerDisplay ( "{DebuggerDisplay(),nq}" ), Serializable ]
    public class SerializableEntry
    {
        public string?     EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        #pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary < string, object? >? Properties         { get; set; }
        public Dictionary < string, object? >? ModifiedProperties { get; set; }
        public HashSet    < string >?          NavigationState    { get; set; }
        #pragma warning restore CA2227 // Collection properties should be read only

        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { string.Join ( ", ", Properties?.Select ( property => property.Value ) ) }";
    }
}