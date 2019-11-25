using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using EntityFrameworkCore.Serialization.Internal;

namespace EntityFrameworkCore.Serialization
{
    public static class Deserializer
    {
        public static void Deserialize < TEntry > ( this DbContext context, IEnumerable < TEntry > entries, IDbContextSerializer < TEntry > serializer )
        {
            var finder = new EntityEntryFinder < TEntry > ( context, serializer );
            var pairs  = entries.Select ( entry => new { Entry       = entry,
                                                         EntityEntry = finder.FindOrCreate ( entry ) } )
                                .ToList ( );

            foreach ( var pair in pairs ) serializer.DeserializeProperties         ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeEntityState        ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeModifiedProperties ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeLoadedCollections  ( pair.Entry, pair.EntityEntry );
        }
    }
}