using System.ComponentModel.DataAnnotations;

namespace EnumCodeDemo
{
    public enum TestEnum
    {
        [Display(Name = "Value 1", Description = "This is value 1", Order = 1)]
        Value1 = 1,
        [Display(Name = "Value 2", Description = "This is value 2", Order = 1)]
        Value2 = 2,
    }
}
