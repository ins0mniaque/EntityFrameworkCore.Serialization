using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Diagnostics
{
    [ Serializable ]
    public partial class EntityEntryData
    {
        public string?     EntityType  { get; set; }
        public EntityState EntityState { get; set; }

        #pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary < string, object? >? Properties         { get; set; }
        public Dictionary < string, object? >? ModifiedProperties { get; set; }
        public HashSet    < string >?          NavigationState    { get; set; }
        #pragma warning restore CA2227 // Collection properties should be read only
    }
}