using System.Text.Json.Serialization;
using Xunit;
using FluentAssertions;
using System.Text.Json;

namespace YS.Knife.Extensions.Json.UnitTest
{
    public class JsonIgnoreWhenReadAttributeTest
    {
        [Fact]
        public void ShouldIgnorePropertyWhenReading()
        {
            var json = "{\"Id\": 1, \"Name\": \"Test\", \"IgnoreWhenRead\": \"This should be ignored\"}";
            var result = JsonSerializer.Deserialize<TestModel>(json);
            result.Id.Should().Be(1);
            result.Name.Should().Be("Test");
            result.IgnoreWhenRead.Should().BeNull();
        }

        class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            
            [JsonIgnoreWhenRead]
            public string IgnoreWhenRead { get; set; }
        }
    }
}
