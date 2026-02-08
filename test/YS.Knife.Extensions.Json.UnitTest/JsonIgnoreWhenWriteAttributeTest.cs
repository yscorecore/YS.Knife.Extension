using System.Text.Json.Serialization;
using Xunit;
using FluentAssertions;
using System.Text.Json;

namespace YS.Knife.Extensions.Json.UnitTest
{
    public class JsonIgnoreWhenWriteAttributeTest
    {
        [Fact]
        public void ShouldIgnorePropertyWhenWriting()
        {
            var model = new TestModel
            {
                Id = 1,
                Name = "Test",
                IgnoreWhenWrite = "This should be ignored"
            };
            
            var json = JsonSerializer.Serialize(model);

            json.Should().Be(@"{""Id"":1,""Name"":""Test"",""IgnoreWhenWrite"":null}");
        }

        class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [JsonIgnoreWhenWrite]
            public string IgnoreWhenWrite { get; set; }
        }
    }
}
