using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Extensions.Validation.UnitTest
{
    public class NameRuleAttributeTest
    {
        #region Default rules - only allow ASCII letters (a-zA-Z)

        [Theory]
        [InlineData("abc")]
        [InlineData("ABC")]
        [InlineData("Hello")]
        [InlineData("a")]
        [InlineData("HelloWorld")]
        public void ShouldPass_When_ValidNameWithDefaultRules(string name)
        {
            var attr = new NameRuleAttribute();
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("abc123")]        // digits not allowed by default
        [InlineData("hello_world")]  // underscore not allowed by default
        [InlineData("hello-world")]   // hyphen not allowed by default
        [InlineData("hello world")]   // space not allowed by default
        [InlineData("张三")]           // unicode letters not allowed by default
        [InlineData("café")]          // unicode letters not allowed by default
        [InlineData("東京")]
        [InlineData("Москва")]
        [InlineData("hello@world")]
        [InlineData("hello.world")]
        [InlineData("hello!world")]
        [InlineData("#hashtag")]
        [InlineData("name*")]
        [InlineData("a/b")]
        [InlineData("")]
        public void ShouldFail_When_InvalidNameWithDefaultRules(string name)
        {
            var attr = new NameRuleAttribute();
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Null value

        [Fact]
        public void ShouldPass_When_ValueIsNull()
        {
            var attr = new NameRuleAttribute();
            attr.IsValid(null).Should().BeTrue();
        }

        #endregion

        #region Non-string type

        [Fact]
        public void ShouldFail_When_ValueIsNotString()
        {
            var attr = new NameRuleAttribute();
            attr.IsValid(12345).Should().BeFalse();
        }

        #endregion

        #region AllowDigits

        [Theory]
        [InlineData("abc123")]
        [InlineData("123")]
        [InlineData("a1b2c3")]
        public void ShouldPass_When_DigitsAllowedAndHasDigits(string name)
        {
            var attr = new NameRuleAttribute { AllowDigits = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("abc123")]
        [InlineData("123")]
        public void ShouldFail_When_DigitsNotAllowedAndHasDigits(string name)
        {
            var attr = new NameRuleAttribute { AllowDigits = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowUpperCaseLetters = false

        [Theory]
        [InlineData("abc")]
        [InlineData("abc123")]
        public void ShouldPass_When_UpperCaseNotAllowedAndNoUpperCase(string name)
        {
            var attr = new NameRuleAttribute { AllowUpperCaseLetters = false, AllowDigits = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("ABC")]
        [InlineData("Hello")]
        [InlineData("ABC123")]
        public void ShouldFail_When_UpperCaseNotAllowedAndHasUpperCase(string name)
        {
            var attr = new NameRuleAttribute { AllowUpperCaseLetters = false, AllowDigits = true };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowLowerCaseLetters = false

        [Theory]
        [InlineData("ABC")]
        [InlineData("ABC123")]
        public void ShouldPass_When_LowerCaseNotAllowedAndNoLowerCase(string name)
        {
            var attr = new NameRuleAttribute { AllowLowerCaseLetters = false, AllowDigits = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("Hello")]
        [InlineData("abc123")]
        public void ShouldFail_When_LowerCaseNotAllowedAndHasLowerCase(string name)
        {
            var attr = new NameRuleAttribute { AllowLowerCaseLetters = false, AllowDigits = true };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Both upper and lower case disabled (no ASCII letters)

        [Theory]
        [InlineData("123")]
        [InlineData("123_456")]
        [InlineData("张三")]
        [InlineData("Москва")]
        public void ShouldPass_When_AllAsciiLettersDisabledAndNoAsciiLetters(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowDigits = true,
                AllowUnderscore = true,
                AllowUnicodeLetters = true
            };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("ABC")]
        [InlineData("Hello")]
        [InlineData("123abc")]
        public void ShouldFail_When_AllAsciiLettersDisabledAndHasAsciiLetters(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowDigits = true
            };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowUnicodeLetters

        [Theory]
        [InlineData("张三")]
        [InlineData("Москва")]
        [InlineData("東京")]
        [InlineData("한국어")]
        public void ShouldPass_When_UnicodeLettersAllowedAndHasUnicodeLetters(string name)
        {
            var attr = new NameRuleAttribute { AllowUnicodeLetters = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("张三")]
        [InlineData("Москва")]
        public void ShouldFail_When_UnicodeLettersNotAllowedAndHasUnicodeLetters(string name)
        {
            var attr = new NameRuleAttribute { AllowUnicodeLetters = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Unicode letters with ASCII letters partially disabled

        [Theory]
        [InlineData("张三abc")]
        [InlineData("张三ABC")]
        public void ShouldPass_When_UnicodeAllowedAndAllAsciiAllowed(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = true,
                AllowLowerCaseLetters = true,
                AllowUnicodeLetters = true
            };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("张三")]  // pure Unicode, no ASCII
        public void ShouldPass_When_UnicodeAllowedButAsciiDisabled(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowUnicodeLetters = true
            };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("张三abc")]  // contains excluded ASCII lowercase
        [InlineData("张三ABC")]  // contains excluded ASCII uppercase
        public void ShouldFail_When_UnicodeAllowedButAsciiDisabledAndHasAscii(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowUnicodeLetters = true
            };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowSpace

        [Theory]
        [InlineData("hello world")]
        [InlineData("abc def")]
        public void ShouldPass_When_SpaceAllowedAndHasSpace(string name)
        {
            var attr = new NameRuleAttribute { AllowSpace = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("hello world")]
        public void ShouldFail_When_SpaceNotAllowedAndHasSpace(string name)
        {
            var attr = new NameRuleAttribute { AllowSpace = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowHyphen

        [Theory]
        [InlineData("hello-world")]
        public void ShouldPass_When_HyphenAllowedAndHasHyphen(string name)
        {
            var attr = new NameRuleAttribute { AllowHyphen = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("hello-world")]
        public void ShouldFail_When_HyphenNotAllowedAndHasHyphen(string name)
        {
            var attr = new NameRuleAttribute { AllowHyphen = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowUnderscore

        [Theory]
        [InlineData("hello_world")]
        public void ShouldPass_When_UnderscoreAllowedAndHasUnderscore(string name)
        {
            var attr = new NameRuleAttribute { AllowUnderscore = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("hello_world")]
        public void ShouldFail_When_UnderscoreNotAllowedAndHasUnderscore(string name)
        {
            var attr = new NameRuleAttribute { AllowUnderscore = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region AllowMarks - combining marks for accented characters

        [Theory]
        [InlineData("café")]
        [InlineData("naïve")]
        [InlineData("über")]
        public void ShouldPass_When_MarksAndUnicodeLettersAllowedAndHasAccentedChars(string name)
        {
            var attr = new NameRuleAttribute { AllowUnicodeLetters = true, AllowMarks = true };
            attr.IsValid(name).Should().BeTrue();
        }

        #endregion

        #region MinLength

        [Theory]
        [InlineData("abc")]
        [InlineData("abcde")]
        public void ShouldPass_When_LengthMeetsMinLength(string name)
        {
            var attr = new NameRuleAttribute { MinLength = 3 };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("a")]
        public void ShouldFail_When_LengthLessThanMinLength(string name)
        {
            var attr = new NameRuleAttribute { MinLength = 3 };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region MaxLength

        [Theory]
        [InlineData("abc")]
        [InlineData("abcde")]
        public void ShouldPass_When_LengthWithinMaxLength(string name)
        {
            var attr = new NameRuleAttribute { MaxLength = 5 };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("abcdef")]
        [InlineData("abcdefghij")]
        public void ShouldFail_When_LengthExceedsMaxLength(string name)
        {
            var attr = new NameRuleAttribute { MaxLength = 5 };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Globalization - multi-language support

        [Theory]
        [InlineData("日本語テスト")]       // Japanese
        [InlineData("한국어")]             // Korean Hangul
        [InlineData("Привет")]            // Russian Cyrillic
        [InlineData("مرحبا")]             // Arabic
        [InlineData("सवास्दी")]            // Thai
        [InlineData("नमस्ते")]            // Hindi Devanagari
        [InlineData("Γειά")]              // Greek
        [InlineData("שלום")]              // Hebrew
        public void ShouldPass_When_UnicodeLettersAndMarksEnabled(string name)
        {
            var attr = new NameRuleAttribute { AllowUnicodeLetters = true, AllowMarks = true };
            attr.IsValid(name).Should().BeTrue();
        }

        [Theory]
        [InlineData("日本語テスト")]
        [InlineData("한국어")]
        [InlineData("Привет")]
        public void ShouldFail_When_UnicodeLettersDisabled(string name)
        {
            var attr = new NameRuleAttribute { AllowUnicodeLetters = false };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Combination - digits and underscore only (no letters)

        [Fact]
        public void ShouldPass_When_DigitsAndUnderscoreOnly()
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowUnicodeLetters = false,
                AllowMarks = false,
                AllowDigits = true,
                AllowUnderscore = true
            };
            attr.IsValid("123_456").Should().BeTrue();
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("Hello")]
        public void ShouldFail_When_AllLettersDisabled(string name)
        {
            var attr = new NameRuleAttribute
            {
                AllowUpperCaseLetters = false,
                AllowLowerCaseLetters = false,
                AllowUnicodeLetters = false,
                AllowMarks = false,
                AllowDigits = true,
                AllowUnderscore = true
            };
            attr.IsValid(name).Should().BeFalse();
        }

        #endregion

        #region Combination - length limits with unicode

        [Fact]
        public void ShouldPass_When_LengthBetweenMinAndMax()
        {
            var attr = new NameRuleAttribute { MinLength = 2, MaxLength = 10, AllowUnicodeLetters = true };
            attr.IsValid("张三").Should().BeTrue();
            attr.IsValid("hello").Should().BeTrue();
            attr.IsValid("张三丰abc").Should().BeTrue();
            attr.IsValid("Привет").Should().BeTrue();
        }

        [Fact]
        public void ShouldFail_When_LengthOutOfRange()
        {
            var attr = new NameRuleAttribute { MinLength = 2, MaxLength = 5 };
            attr.IsValid("a").Should().BeFalse();
            attr.IsValid("abcdefgh").Should().BeFalse();
        }

        #endregion
    }
}
