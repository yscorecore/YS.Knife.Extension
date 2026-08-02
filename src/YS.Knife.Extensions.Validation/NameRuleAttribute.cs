using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class NameRuleAttribute : ValidationAttribute
    {
        private const string DefaultErrorMessage = "{0} contains invalid characters";

        public NameRuleAttribute()
        {
            ErrorMessage = DefaultErrorMessage;
        }

        /// <summary>
        /// Whether to allow uppercase ASCII letters (A-Z), default true
        /// </summary>
        public bool AllowUpperCaseLetters { get; set; } = true;

        /// <summary>
        /// Whether to allow lowercase ASCII letters (a-z), default true
        /// </summary>
        public bool AllowLowerCaseLetters { get; set; } = true;

        /// <summary>
        /// Whether to allow digit characters (Unicode \p{N}), default false
        /// </summary>
        public bool AllowDigits { get; set; } = false;

        /// <summary>
        /// Whether to allow Unicode letter characters (\p{L}), default false.
        /// This covers all languages: Latin, CJK, Cyrillic, Arabic, etc.
        /// </summary>
        public bool AllowUnicodeLetters { get; set; } = false;

        /// <summary>
        /// Whether to allow combining marks (Unicode \p{M}), default false.
        /// Needed for accented characters and some scripts.
        /// </summary>
        public bool AllowMarks { get; set; } = false;

        /// <summary>
        /// Whether to allow underscore '_', default false
        /// </summary>
        public bool AllowUnderscore { get; set; } = false;

        /// <summary>
        /// Whether to allow hyphen '-', default false
        /// </summary>
        public bool AllowHyphen { get; set; } = false;

        /// <summary>
        /// Whether to allow space ' ', default false
        /// </summary>
        public bool AllowSpace { get; set; } = false;

        /// <summary>
        /// Minimum length, default 1
        /// </summary>
        public int MinLength { get; set; } = 1;

        /// <summary>
        /// Maximum length, default 0 means no limit
        /// </summary>
        public int MaxLength { get; set; } = 0;

        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }

            var str = value as string;
            if (str == null)
            {
                return false;
            }

            if (str.Length < MinLength)
            {
                return false;
            }

            if (MaxLength > 0 && str.Length > MaxLength)
            {
                return false;
            }

            var pattern = BuildPattern();
            return Regex.IsMatch(str, pattern);
        }

        private string BuildPattern()
        {
            var charClass = "";

            var allowUpper = AllowUpperCaseLetters;
            var allowLower = AllowLowerCaseLetters;
            var allowAnyAscii = allowUpper || allowLower;

            if (allowAnyAscii && AllowUnicodeLetters)
            {
                charClass += "\\p{L}";
            }
            else if (allowUpper && allowLower)
            {
                charClass += "a-zA-Z";
            }
            else if (allowUpper)
            {
                charClass += "A-Z";
            }
            else if (allowLower)
            {
                charClass += "a-z";
            }
            else if (AllowUnicodeLetters)
            {
                charClass += "\\p{L}";
            }

            if (AllowDigits)
            {
                charClass += "\\p{N}";
            }

            if (AllowMarks)
            {
                charClass += "\\p{M}";
            }

            if (AllowUnderscore)
            {
                charClass += "_";
            }

            if (AllowHyphen)
            {
                charClass += "\\-";
            }

            if (AllowSpace)
            {
                charClass += " ";
            }

            // Compute excluded ASCII letters when Unicode letters are allowed but some ASCII letters are not
            if (AllowUnicodeLetters && (!allowUpper || !allowLower))
            {
                var excluded = "";
                if (!allowUpper) excluded += "A-Z";
                if (!allowLower) excluded += "a-z";
                if (excluded.Length > 0)
                {
                    // Use character class subtraction to exclude specific ASCII letter ranges
                    return $"^[{charClass}-[{excluded}]]+$";
                }
            }

            return $"^[{charClass}]+$";
        }
    }
}
