using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObject
    {
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
            if ( type.IsSerializable )
                return FormatterServices.GetSerializableMembers ( type );

            var fields       = type.GetFields ( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            var serializable = 0;
            for ( var index = 0; index < fields.Length; index++ )
                if ( ( fields [ index ].Attributes & FieldAttributes.NotSerialized ) != FieldAttributes.NotSerialized )
                    serializable++;

            if ( serializable == fields.Length )
                return fields;

            var serializableFields = new FieldInfo [ serializable ];

            serializable = 0;
            for ( var index = 0; index < fields.Length; index++ )
                if ( ( fields [ index ].Attributes & FieldAttributes.NotSerialized ) != FieldAttributes.NotSerialized )
                    serializableFields [ serializable++ ] = fields [ index ];

            return serializableFields;
        }
    }
}