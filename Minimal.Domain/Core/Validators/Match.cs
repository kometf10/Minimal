using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Validators
{
    public static class Match
    {
        public static bool IsEnglish(string input)
        {
            var pattent = new Regex("^[a-zA-Z ]*$", RegexOptions.Compiled);

            return input != null && pattent.IsMatch(input);
        }

        public static bool IsNumber(string input)
        {
            var pattent = new Regex("^[0-9]*$", RegexOptions.Compiled);

            return input != null && pattent.IsMatch(input);
        }

        public static bool MaxLen(this string input, int length)
        {
            return input != null && input.Length <= length;
        }

        public static bool MinLen(this string input, int length)
        {
            return input != null && input.Length >= length;
        }

        public static bool EqualLen(this string input, int length)
        {
            return input != null && input.Length == length;
        }

        public static bool PositiveInt(int? input)
        {
            return input != null && input > 0;
        }

        public static bool PositiveInt(int input)
        {
            return input > 0;
        }

        public static bool PositiveOrZeroInt(int? input)
        {
            return input != null && input >= 0;
        }

        public static bool PositiveOrZeroInt(int input)
        {
            return input >= 0;
        }

        public static bool PositiveDeciaml(decimal? input)
        {
            return input != null && input > 0;
        }

        public static bool PositiveDeciaml(decimal input)
        {
            return input > 0;
        }

        public static bool IsPhone(string input)
        {
            return IsNumber(input) && input.MaxLen(10);
        }

        public static bool IsMobile(string input)
        {
            return IsNumber(input) && (input.EqualLen(10) || input.EqualLen(11));
        }

        public static bool IsNationalNumber(string input)
        {
            return IsNumber(input) && input.EqualLen(10);
        }

        public static bool IsUserName(string input)
        {
            var pattent = new Regex("^[a-zA-Z0-9_.]*$", RegexOptions.Compiled);

            return input != null && pattent.IsMatch(input);
        }

        public static bool IsSalary(decimal? input)
        {
            return input != null && input > 0;
        }

        public static bool IsMony(decimal? input) => input != null && input >= 0;
        public static bool IsMony(double? input) => input != null && input >= 0;

    }
}
