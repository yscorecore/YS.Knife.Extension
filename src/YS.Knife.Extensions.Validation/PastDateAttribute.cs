using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
     AllowMultiple = false)]
    public class PastDateAttribute : ValidationAttribute
    {
        public PastDateAttribute()
        {
            ErrorMessage = "只能是过去的日期";
        }
        public bool AllowToday { get; set; } = true;
        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                if (AllowToday)
                {
                    if (dateTime > DateTime.Today)
                    {
                        return false;
                    }
                }
                else
                {
                    if (dateTime >= DateTime.Today)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }
}
