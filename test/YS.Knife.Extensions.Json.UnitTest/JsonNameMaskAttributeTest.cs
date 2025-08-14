using System.Text.Json.Serialization;

namespace YS.Knife.Extensions.Json.UnitTest
{
    public class JsonNameMaskAttributeTest
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("张", "张")]
        [InlineData("张三", "张*")]
        [InlineData("赵田孙", "赵*孙")]
        [InlineData("欧阳一二", "欧阳*二")]
        [InlineData("离离原上草", "离离**草")]
        public void ShouldMaskPropertyName(string name, string expected)
        {
            var user = new User() { Name = name };
            var maskText = System.Text.Json.JsonSerializer.Serialize(user);
            var newUser = System.Text.Json.JsonSerializer.Deserialize<User>(maskText);
            newUser.Name.Should().Be(expected);
        }

        class User
        {
            [JsonNameMask]
            public string Name { get; set; }

        }
    }
}
