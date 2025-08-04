using System.ComponentModel.DataAnnotations;

namespace EnumCodeDemo
{
    public enum TestEnum
    {
        [Display(Name = "值1", Description = "值1的描述", Order = 1)]
        Value1 = 1,
        [Display(Name = "值2", Description = "值2的描述", Order = 2)]
        Value2 = 2,
    }
}
