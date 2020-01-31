using System;
using System.ComponentModel;
using System.Linq;

namespace GooglePlayStoreApi
{
    public static class EnumExtensions
    {
        private static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
            return attributes.FirstOrDefault();
        }

        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class CountryCodeAttribute : Attribute
        {
            public string CountryCode { get; private set; }

            public CountryCodeAttribute(string countryCode)
            {
                CountryCode = countryCode;
            }
        }

        public static string GetCountryCode(this Enum value) => value.GetAttribute<CountryCodeAttribute>()?.CountryCode ?? value.ToString();

        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class CountryAttribute : Attribute
        {
            public string Country { get; private set; }

            public CountryAttribute(string country)
            {
                Country = country;
            }
        }

        public static string GetCountry(this Enum value) => value.GetAttribute<CountryAttribute>()?.Country ?? value.ToString();
    }
}
