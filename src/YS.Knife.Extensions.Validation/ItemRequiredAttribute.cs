using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public class ItemRequiredAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (base.IsValid(item) == false)
                    {
                        return false;
                    }
                }
            }
            return base.IsValid(value);
        }
    }
}
