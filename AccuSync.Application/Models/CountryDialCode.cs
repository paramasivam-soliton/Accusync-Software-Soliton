using System.Collections.Generic;
using System.Linq;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Represents a country's phone dial code, display format, and example number.
    /// Used by the phone number input field and its associated
    /// <see cref="AccuSync.Application.Helpers.PhoneNumberFormatter"/>.
    /// </summary>
    // TODO: Format, Example, and the country list are duplicated in PhoneNumberFormatter.
    //       Drive the formatter from this model so there's a single source of truth.
    public class CountryDialCode
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Flag { get; set; }
        public string DialCode { get; set; }
        public string Format { get; set; }
        public string Example { get; set; }

        // TODO: GetCountries() allocates a new list on every call. Cache the list
        //       in a static field — country data doesn't change at runtime.
        public static List<CountryDialCode> GetCountries()
        {
            return new List<CountryDialCode>
            {
                new CountryDialCode { Code = "US", Name = "United States", Flag = "US", DialCode = "+1", Format = "(XXX) XXX-XXXX", Example = "(555) 123-4567" },
                new CountryDialCode { Code = "CA", Name = "Canada", Flag = "CA", DialCode = "+1", Format = "(XXX) XXX-XXXX", Example = "(416) 555-1234" },
                new CountryDialCode { Code = "MX", Name = "Mexico", Flag = "MX", DialCode = "+52", Format = "XX XXXX XXXX", Example = "55 1234 5678" },
                new CountryDialCode { Code = "GB", Name = "United Kingdom", Flag = "GB", DialCode = "+44", Format = "XXXX XXX XXXX", Example = "0207 123 4567" },
                new CountryDialCode { Code = "FR", Name = "France", Flag = "FR", DialCode = "+33", Format = "X XX XX XX XX", Example = "1 23 45 67 89" },
                new CountryDialCode { Code = "DE", Name = "Germany", Flag = "DE", DialCode = "+49", Format = "XXX XXXXXXX", Example = "030 12345678" },
                new CountryDialCode { Code = "JP", Name = "Japan", Flag = "JP", DialCode = "+81", Format = "XX-XXXX-XXXX", Example = "03-1234-5678" },
                new CountryDialCode { Code = "CN", Name = "China (中国)", Flag = "CN", DialCode = "+86", Format = "XXX XXXX XXXX", Example = "138 0013 8000" },
                new CountryDialCode { Code = "IN", Name = "India (भारत)", Flag = "IN", DialCode = "+91", Format = "XXXXX XXXXX", Example = "98765 43210" },
                new CountryDialCode { Code = "AU", Name = "Australia", Flag = "AU", DialCode = "+61", Format = "XXXX XXX XXX", Example = "0412 345 678" },
                new CountryDialCode { Code = "BR", Name = "Brazil", Flag = "BR", DialCode = "+55", Format = "XX XXXXX-XXXX", Example = "11 98765-4321" },
                new CountryDialCode { Code = "IT", Name = "Italy", Flag = "IT", DialCode = "+39", Format = "XXX XXX XXXX", Example = "06 1234 5678" },
                new CountryDialCode { Code = "ES", Name = "Spain", Flag = "ES", DialCode = "+34", Format = "XXX XX XX XX", Example = "912 34 56 78" },
                new CountryDialCode { Code = "RU", Name = "Russia", Flag = "RU", DialCode = "+7", Format = "XXX XXX-XX-XX", Example = "495 123-45-67" },
                new CountryDialCode { Code = "KR", Name = "South Korea", Flag = "KR", DialCode = "+82", Format = "XX-XXXX-XXXX", Example = "02-1234-5678" }
            };
        }

        // TODO: Both lookups call GetCountries() twice on a miss (once for FirstOrDefault,
        //       once for the fallback First). Store the list in a local variable.
        public static CountryDialCode GetByDialCode(string dialCode)
        {
            return GetCountries().FirstOrDefault(c => c.DialCode == dialCode) ?? GetCountries().First();
        }

        public static CountryDialCode GetByCode(string code)
        {
            return GetCountries().FirstOrDefault(c => c.Code == code) ?? GetCountries().First();
        }

        /// <summary>
        /// Compact display string for the dial-code ComboBox (e.g., <c>"US +1"</c>).
        /// </summary>
        public string DisplayText => $"{Flag} {DialCode}";
    }
}