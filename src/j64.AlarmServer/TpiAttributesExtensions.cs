using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    public class CommandValueAttribute : Attribute
    {
        public string CommandValue { get; protected set; }
        public int NumBytes { get; protected set; }

        public CommandValueAttribute(string value)
        {
            CommandValue = value;
        }

        public CommandValueAttribute(string value, int numBytes)
        {
            CommandValue = value;
            NumBytes = numBytes;
        }
    }

    public static class EnumExtensionClass
    {
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo.GetCustomAttributes(
                typeof(CommandValueAttribute), false) as CommandValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].CommandValue : null;
        }

        public static int GetNumberOfBytes(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo.GetCustomAttributes(typeof(CommandValueAttribute), false) as CommandValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].NumBytes : 0;
        }
    }
}
