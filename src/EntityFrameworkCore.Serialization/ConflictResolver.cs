using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public enum ConflictResolution
    {
        KeepExistingEntry,
        OverwriteExistingEntry
    }

    public delegate ConflictResolution ResolveConflict ( EntityEntry existingEntry, EntityState updatedState, IDictionary < IProperty, object? > updatedEntry );

    public static class ConflictResolver
    {
        public static ResolveConflict KeepExistingEntries { get; } = (_, __, ___)   => ConflictResolution.KeepExistingEntry;
        public static ResolveConflict KeepModifiedEntries { get; } = (entry, _, __) => entry.State == EntityState.Unchanged ? ConflictResolution.OverwriteExistingEntry :
                                                                                                                              ConflictResolution.KeepExistingEntry;

        public static ResolveConflict OverwriteExistingEntries { get; } = (_, __, ___) => ConflictResolution.OverwriteExistingEntry;
    }
}