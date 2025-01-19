using System.Reflection;
using System.Runtime.CompilerServices;

namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    internal static class AnonymousTypeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymous(this Type type)
        {
            if (type.IsClass && type.IsSealed && type.Attributes.HasFlag(TypeAttributes.NotPublic))
            {
                var attributes = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsAnonymousType<T>(this T instance)
        {
            return instance != null && IsAnonymous(instance.GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymous"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T? GetAnonymousProperty<T>(this object anonymous, string propertyName)
        {
            if (anonymous.GetAnonymousProperty(propertyName) is T tVal)
                return tVal;
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anonymous"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object? GetAnonymousProperty(this object anonymous, string propertyName)
        {
            var type = anonymous?.GetType();
            if (type == null || string.IsNullOrEmpty(propertyName) || !type.IsAnonymous())
                return default;

            var property = type.GetProperty(propertyName);

            if (property == null)
                return default;

            return property.GetValue(propertyName, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anonymous"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetAnonymousProperties(this object anonymous)
        {
            if (anonymous.IsAnonymousType())
                return anonymous.GetType().GetProperties();
            return [];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonymous"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetAnonymousProperty<T>(this object anonymous, string propertyName, T? value)
        {
            var type = anonymous?.GetType();
            if (type == null || string.IsNullOrEmpty(propertyName) || !type.IsAnonymous())
                return;

            var property = type.GetProperty(propertyName);
            if (property == null)
                return;

            property.SetValue(anonymous, value, null);
        }
    }
}
