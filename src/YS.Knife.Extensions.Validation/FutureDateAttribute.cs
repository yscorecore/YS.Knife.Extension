using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            ErrorMessage = "只能是将来的日期";
        }
        public bool AllowToday { get; set; } = true;

        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                if (AllowToday)
                {
                    if (dateTime < DateTime.Today)
                    {
                        return false;
                    }
                }
                else
                {
                    if (dateTime <= DateTime.Today)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
