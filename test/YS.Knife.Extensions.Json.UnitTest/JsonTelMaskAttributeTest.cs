using System.Text.Json.Serialization;

namespace YS.Knife.Extensions.Json.UnitTest
{
    public class JsonTelMaskAttributeTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("123", "123")]
        [InlineData("1234", "1234")]
        [InlineData("12345", "*2345")]
        [InlineData("123456", "**3456")]
        [InlineData("1234567", "***4567")]
        [InlineData("12345678", "****5678")]
        [InlineData("123456789", "1****6789")]
        [InlineData("029-5512991", "029****2991")]
        [InlineData("13612345678", "136****5678")]
        [InlineData("+86 13612345678", "+86 136****5678")]
        public void ShouldMaskPropertyName(string name, string expected)
        {
            var user = new User() { Tel = name };
            var maskText = System.Text.Json.JsonSerializer.Serialize(user);
            var newUser = System.Text.Json.JsonSerializer.Deserialize<User>(maskText);
            newUser.Tel.Should().Be(expected);
        }

        class User
        {
            [JsonTelMask]
            public string Tel { get; set; }

        }
    }
}
