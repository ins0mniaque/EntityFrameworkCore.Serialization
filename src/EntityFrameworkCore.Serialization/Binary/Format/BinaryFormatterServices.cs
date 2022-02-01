using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public static class BinaryFormatterServices
    {
        public static object? [ ] GetObjectData ( this MemberInfo [ ] members, object value )
        {
            return FormatterServices.GetObjectData ( value, members );
        }

        public static object SetObjectData ( this MemberInfo [ ] members, object value, object? [ ] data )
        {
            return FormatterServices.PopulateObjectMembers ( value, members, data );
        }

        public static object GetUninitializedObject ( this Type type )
        {
            if ( type == null )
                throw new ArgumentNullException ( nameof ( type ) );

            try
            {
                return FormatterServices.GetUninitializedObject ( type );
            }
            catch ( Exception exception )
            {
                throw new SerializationException ( $"Could not create an object of type { type.FullName }", exception );
            }
        }

        public static Type GetSerializableType ( this MemberInfo member )
        {
            return member switch
            {
                FieldInfo    field    => field   .FieldType,
                PropertyInfo property => property.PropertyType,
                _                     => throw new NotSupportedException ( )
            };
        }

        public static MemberInfo [ ] GetSerializableMembers ( this Type type )
        {
            if ( type == null )
                throw new ArgumentNullException ( nameof ( type ) );

            if ( type.IsSerializable )
                return FormatterServices.GetSerializableMembers ( type );

            var typeFields = type.GetFields ( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            var parentType = type.BaseType;

            if ( parentType != null && ! parentType.HasBaseClass ( ) && typeFields.AreAllSerializable ( ) )
                return typeFields;

            var fields = new List < MemberInfo > ( typeFields.Length );

            foreach ( var field in typeFields )
                if ( field.IsSerializable ( ) )
                    fields.Add ( field );

            while ( parentType != null && parentType.HasBaseClass ( ) )
            {
                foreach ( var field in parentType.GetFields ( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly ) )
                    if ( field.IsSerializable ( ) )
                        fields.Add ( field );

                parentType = parentType.BaseType;
            }

            return fields.ToArray ( );
        }

        [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
        private static bool AreAllSerializable ( this FieldInfo [ ] fields )
        {
            foreach ( var field in fields )
                if ( ! field.IsSerializable ( ) )
                    return false;

            return true;
        }

        [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
        private static bool IsSerializable ( this FieldInfo field )
        {
            return ! field.IsNotSerialized && ! typeof ( Delegate ).IsAssignableFrom ( field.FieldType );
        }

        [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
        private static bool HasBaseClass ( this Type type )
        {
            return type != null && type != typeof ( object );
        }
    }
}