using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    public class LessThanAttribute : ValidationAttribute
    {
        private const string ErrorMessageStringDefault = "{0}必须小于{1}";
        private const string ErrorMessageStringDefault2 = "{0}必须小于等于{1}";
        public string OtherPropertyName { get; private set; }

        public LessThanAttribute(string otherPropertyName)
        {
            OtherPropertyName = otherPropertyName;
        }
        public bool AllowEquals { get; set; } = false;

        public string FormatErrorMessage(string name, ValidationContext context)
        {
            var otherPropertyName = GetDisplayNameForProperty(context.ObjectType.GetProperty(OtherPropertyName));
            if (AllowEquals)
            {
                return string.Format(CultureInfo.CurrentCulture, ErrorMessage ?? ErrorMessageStringDefault2, name, otherPropertyName);
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, ErrorMessage ?? ErrorMessageStringDefault, name, otherPropertyName);
            }

        }

        private string GetDisplayNameForProperty(PropertyInfo property)
        {
            IEnumerable<Attribute> attributes = CustomAttributeExtensions.GetCustomAttributes(property, true);
            foreach (Attribute attribute in attributes)
            {
                if (attribute is DisplayAttribute display)
                {
                    return display.GetName();
                }
            }

            return OtherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(OtherPropertyName);

            if (property == null)
            {
                return new ValidationResult($"Property {OtherPropertyName} is not exists.");
            }


            var otherValue = property.GetValue(validationContext.ObjectInstance);
            var result = Comparer.DefaultInvariant.Compare(value, otherValue);

            if (result > 0)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, validationContext));
            }
            else if (result == 0)
            {
                if (!AllowEquals)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, validationContext));
                }
            }

            return ValidationResult.Success;
        }
    }
}
