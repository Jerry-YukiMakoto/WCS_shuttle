using System;

namespace Mirle.Extensions
{
    [Obsolete]
    public static class EnumExtensions
    {
        [Obsolete]
        public static T ToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        [Obsolete]
        public static T ToEnum<T>(this int enumValue)
        {
            return (T)Enum.ToObject(typeof(T), enumValue);
        }
    }
}
