using System.Diagnostics;
using System.Reflection;
using Bogus.Platform;
using FluentAssertions.Common;
using YS.Knife.FileStorage.SystemArgument.Default;

namespace YS.Knife.FileStorage.Api.AspnetCore.UnitTest
{
    public class TemplatePlaceholderTest
    {
        [Fact]
        public void ShouldFillTemplateWithNoPlaceHolder()
        {
            FillTemplate("abc.txt").Should().Be("abc.txt");
        }
        [Fact]
        public void ShouldThrowExceptionWhenDynamicNameTemplateContainsInvalidFunction()
        {
            Assert.Throws<Exception>(() =>
            {
                FillTemplate("{aaa}");
            });
        }

        [Theory]
        [InlineData("{guid}", @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$")]
        [InlineData("{guid:N}", @"^[0-9a-f]{32}$")]
        [InlineData("upload_{guid:N}", @"^upload_[0-9a-f]{32}$")]
        [InlineData("start/{guid}/end", @"^start/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/end$")]
        [InlineData("start/{guid:N}/end", @"^start/[0-9a-f]{32}/end$")]
        [InlineData("{now}", @"^\d{17}$")]
        [InlineData("{utcnow}", @"^\d{17}$")]
        [InlineData("{random}", @"^\d{10}$")]
        [InlineData("{seconds}", @"^\d{10}$")]
        [InlineData("{milliseconds}", @"^\d{13}$")]

        public void ShouldCreateNameWhenContainsValidFunction(string templateName, string matchRegex)
        {
            var name = FillTemplate(templateName);
            name.Should().NotBeNull().And.MatchRegex(matchRegex);

        }
        [Theory]
        [InlineData("{guid}", @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$")]
        public void ShouldCreateNameWhenContainsValidFunctionAndWithExtension(string templateName, string matchRegex)
        {
            var name = FillTemplate(templateName);
            name.Should().NotBeNull().And.MatchRegex(matchRegex);


        }
        [Fact]
        public void TestmplexTemplate()
        {
            var template = "message_files/{guid}/{now:yyyyMMdd}/{guid}";
            var name = FillTemplate(template);
            name.Should().HaveLength("message_files/c602970f-f579-45c5-bb05-a058f9a3438d/20251204/0f349d5c-5eb1-4dd6-ac62-0d6e66e00695".Length);
        }

        string FillTemplate(string placeHolder)
        {
            var systemArgs = typeof(NowArgumentProvider).GetAssembly().GetTypes()
                .Where(p => typeof(ISystemArgProvider).IsAssignableFrom(p) && p.GetConstructor(Type.EmptyTypes) != null && p.IsDefined(typeof(DictionaryKeyAttribute)))
                .ToDictionary(p => p.GetCustomAttribute<DictionaryKeyAttribute>().Key, p => Activator.CreateInstance(p) as ISystemArgProvider);
            var userArgs = new Dictionary<string, string>();
            return TemplatePlaceholder.Instance.FillPlaceholder(placeHolder, userArgs, systemArgs); ;
        }


    }
}
