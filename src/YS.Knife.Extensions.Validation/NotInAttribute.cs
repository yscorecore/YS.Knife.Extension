using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
       AllowMultiple = false)]
    public class InAttribute : ValidationAttribute
    {
        public InAttribute(params object[] items)
        {
            Items = items;
            ErrorMessage = "不在指定的项里面";
        }

        public object[] Items { get; }

        public override bool IsValid(object value)
        {
            return Items.Contains(value);
        }
    }
}
