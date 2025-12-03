using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Extensions.UnitTest
{
    public class EnumExtensionsTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenGetChineseNameButNotEnumValue()
        {
            Action action = () => 1.GetDisplayName();
            action.Should().Throw<ArgumentException>().Which.Message.Should().Be("should be a enum type.");
        }

        [Fact]
        public void ShouldGetChineseNameWhenDefineDisplayAttribute()
        {
            Test.Field.GetDisplayName().Should().Be("测试");
            Test.Field2.GetDisplayName().Should().Be("测试2");
        }

        [Fact]
        public void ShouldGetChineseNameWhenNoDisplayAttribute()
        {
            Test.Field3.GetDisplayName().Should().Be("Field3");
        }

        [Fact]
        public void ShouldGetChineseNameWhenUseFlagEnum()
        {
            var flagEnum = FlagEnum.Field1 | FlagEnum.Field2;
            flagEnum.GetDisplayName().Should().Be("测试,测试2");


        }
        [Fact]
        public void ShouldGetChineseNameWhenUseFlagEnumAndOneNoDisplayAttribute()
        {
            var flagEnum2 = FlagEnum.Field1 | FlagEnum.Field3;
            flagEnum2.GetDisplayName().Should().Be("测试,Field3");
        }


        public enum Test
        {
            [Display(Name = "测试")]
            Field,
            [Display(Name = "测试2")]
            Field2,

            Field3
        }

        [Flags]
        public enum FlagEnum
        {
            [Display(Name ="测试")]
            Field1 = 1,
            [Display(Name = "测试2")]
            Field2 = 2,
            Field3 = 4,
            Field4 = 8,
        }
    }
}
