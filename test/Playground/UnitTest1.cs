using System.Net;
using System.Text;
using System.Web;

namespace Playground
{
    [Test("Test1")]
    public class UnitTest1
    {
        [Fact]

        public void Test1()
        {
            typeof(UnitTest1).GetCustomAttributes(true)
                .OfType<TestAttribute>()
                .Select(p => p.Name.Value)
                .Should().Contain("Test1");
        }
    }

    public class TestAttribute : BaseAttribute
    {
        public TestAttribute(string name) : base(name)
        {
        }

    }
    public class BaseAttribute : Attribute
    {
        public BaseAttribute(string name)
        {
            this.Name = new Data { Value = name };
        }
        public Data Name { get; }
    }
    public record Data
    {
        public string Value { get; set; }
    }
}
