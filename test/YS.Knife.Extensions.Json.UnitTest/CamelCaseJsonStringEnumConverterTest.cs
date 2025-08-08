using System.Text.Json.Serialization;

namespace YS.Knife.Extensions.Json.UnitTest
{

    public class CamelCaseJsonStringEnumConverterTest
    {
        [Fact]
        public void ShouldSerializeEnumValueAsCamelCase()
        {
            var obj = new TestClass
            {
                IntValue = 1,
                EnumValue = TestEnum.Value1
            };
            obj.ToJsonText().Should().Be(@"{""IntValue"":1,""EnumValue"":""value1""}");
        }

        [Fact]
        public void ShouldDeSerializeEnumStringValue()
        {
            var json = @"{""IntValue"":1,""EnumValue"":""value1""}";
            var testInstance = json.AsJsonObject<TestClass>();
            var obj = new TestClass
            {
                IntValue = 1,
                EnumValue = TestEnum.Value1
            };
            testInstance.Should().BeEquivalentTo(obj);
        }
        [Fact]
        public void ShouldDeSerializeEnumUpperStringValue()
        {
            var json = @"{""IntValue"":1,""EnumValue"":""Value1""}";
            var testInstance = json.AsJsonObject<TestClass>();
            var obj = new TestClass
            {
                IntValue = 1,
                EnumValue = TestEnum.Value1
            };
            testInstance.Should().BeEquivalentTo(obj);
        }
        [Fact]
        public void ShouldDeSerializeEnumIntStringValue()
        {
            var json = @"{""IntValue"":1,""EnumValue"":""0""}";
            var testInstance = json.AsJsonObject<TestClass>();
            var obj = new TestClass
            {
                IntValue = 1,
                EnumValue = TestEnum.Value1
            };
            testInstance.Should().BeEquivalentTo(obj);
        }
        [Fact]
        public void ShouldDeSerializeEnumIntValue()
        {
            var json = @"{""IntValue"":1,""EnumValue"":0}";
            var testInstance = json.AsJsonObject<TestClass>();
            var obj = new TestClass
            {
                IntValue = 1,
                EnumValue = TestEnum.Value1
            };
            testInstance.Should().BeEquivalentTo(obj);
        }
        class TestClass
        {
            public int IntValue { get; set; }
            [System.Text.Json.Serialization.JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
            public TestEnum EnumValue { get; set; }
        }
        enum TestEnum
        {
            Value1,
            Value2
        }

    }
}
