namespace YS.Knife.Extensions.UnitTest
{
    public class DateTimeExtensionsTest
    {
        [Theory]
        [MemberData(nameof(IsBetweenTestDateTimeData))]
        public void IsBetweenDateTimeTest(DateTime value, DateTime start, DateTime end, bool result)
        {
            value.IsBetween(start, end).Should().Be(result);
        }

        public static IEnumerable<object[]> IsBetweenTestDateTimeData()
        {
            yield return new object[] { DateTime.Parse("2023-2-3"), DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-4"), true };
            yield return new object[] { DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-4"), true };
            yield return new object[] { DateTime.Parse("2023-2-4"), DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-4"), true };
            yield return new object[] { DateTime.Parse("2023-2-5"), DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-4"), false };
            yield return new object[] { DateTime.Parse("2023-1-5"), DateTime.Parse("2023-2-1"), DateTime.Parse("2023-2-4"), false };
        }

        [Theory]
        [InlineData(1, 2, 4, false)]
        [InlineData(2, 2, 4, true)]
        [InlineData(3, 2, 4, true)]
        [InlineData(4, 2, 4, true)]
        [InlineData(5, 2, 4, false)]
        public void IsBetweenIntTest(int value, int start, int end, bool result)
        {
            value.IsBetween(start, end).Should().Be(result);
        }


        [Theory]
        [InlineData(1.1, 1.2, 1.4, false)]
        [InlineData(1.2, 1.2, 1.4, true)]
        [InlineData(1.3, 1.2, 1.4, true)]
        [InlineData(1.4, 1.2, 1.4, true)]
        [InlineData(1.5, 1.2, 1.4, false)]
        public void IsBetweenDecimalTest(decimal value, decimal start, decimal end, bool result)
        {
            value.IsBetween(start, end).Should().Be(result);
        }

    }
}
