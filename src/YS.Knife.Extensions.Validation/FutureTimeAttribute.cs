using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class FutureTimeAttribute : ValidationAttribute
    {
        public FutureTimeAttribute()
        {
            ErrorMessage = "只能是将来的时间";
        }

        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime < DateTime.Now)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
