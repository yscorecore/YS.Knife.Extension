using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GreatThanAttribute : ValidationAttribute
    {
        public string OtherPropertyName { get; private set; }

        public GreatThanAttribute(string otherPropertyName)
        {
            OtherPropertyName = otherPropertyName;
        }
        public bool AllowEquals { get; set; } = false;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(OtherPropertyName);

            if (property == null)
            {
                return new ValidationResult($"属性 {OtherPropertyName} 不存在");
            }

            var otherValue = property.GetValue(validationContext.ObjectInstance);
            var result = Comparer.DefaultInvariant.Compare(value, otherValue);

            if (result < 0)
            {
                return new ValidationResult($"{validationContext.DisplayName} 必须大于属性 {OtherPropertyName}");
            }
            else if (result == 0)
            {
                if (!AllowEquals)
                {
                    return new ValidationResult($"{validationContext.DisplayName} 必须大于等于属性 {OtherPropertyName}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
