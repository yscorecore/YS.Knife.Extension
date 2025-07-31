using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class StartsWithAttribute : ValidationAttribute
    {
        public string OtherPropertyName { get; private set; }

        public StartsWithAttribute(string otherPropertyName)
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

            var currentValue = Convert.ToString(value);
            var otherValue = Convert.ToString(property.GetValue(validationContext.ObjectInstance));

            if (!currentValue.StartsWith(otherValue))
            {
                return new ValidationResult($"{validationContext.DisplayName} 前缀必须包含 {OtherPropertyName}");
            }
            else
            {
                if (!AllowEquals && currentValue == otherValue)
                {
                    return new ValidationResult($"{validationContext.DisplayName} 不能等于 {OtherPropertyName}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
