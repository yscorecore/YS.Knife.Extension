using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    public class LessThanAttribute : ValidationAttribute
    {
        public string OtherPropertyName { get; private set; }

        public LessThanAttribute(string otherPropertyName)
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

            if (result > 0)
            {
                return new ValidationResult($"{validationContext.DisplayName} 必须小于属性 {OtherPropertyName}");
            }
            else if (result == 0)
            {
                if (!AllowEquals)
                {
                    return new ValidationResult($"{validationContext.DisplayName} 必须小于或等于属性 {OtherPropertyName}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
