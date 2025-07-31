using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
       AllowMultiple = false)]
    public class ItemUniqueAttribute : ValidationAttribute
    {
        public ItemUniqueAttribute()
        {
            ErrorMessage = "存在重复项";
        }
        public override bool IsValid(object value)
        {
            if (value is Array arr)
            {
                var duplicates = arr.Cast<object>().GroupBy(x => x)
                                      .Where(g => g.Count() > 1)
                                      .Select(g => g.Key);

                if (duplicates.Any())
                {
                    return false;
                }
            }
            else if (value is ICollection lst)
            {
                var duplicates = lst.Cast<object>().GroupBy(x => x)
                                      .Where(g => g.Count() > 1)
                                      .Select(g => g.Key);
                if (duplicates.Any())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
