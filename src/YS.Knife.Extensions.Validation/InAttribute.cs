using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
       AllowMultiple = false)]
    public class NotInAttribute : ValidationAttribute
    {
        public NotInAttribute(params object[] items)
        {
            Items = items;
            ErrorMessage = "不允许包含指定的项";
        }

        public object[] Items { get; }

        public override bool IsValid(object value)
        {
            return !Items.Contains(value);
        }
    }
}
