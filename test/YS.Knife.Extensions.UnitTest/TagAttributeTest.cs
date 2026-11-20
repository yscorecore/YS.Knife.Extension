using System.Reflection;

namespace YS.Knife.Extensions.UnitTest
{
    public class TagAttributeTest
    {
        [Fact]
        public void ShouldReturnNullWhenTypeHasNoTags()
        {
            TagAttribute.GetTags(typeof(NoTagClass)).Should().BeNull();
        }
        [Fact]
        public void ShouldGetTagsFromType()
        {
            var tags = TagAttribute.GetTags(typeof(TaggedClass));
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["category"] = "user",
                ["level"] = "1"
            });
        }
        [Fact]
        public void ShouldGetTagsFromBaseTypeAndOverrideByDerivedType()
        {
            var tags = TagAttribute.GetTags(typeof(DerivedClass));
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["category"] = "derived",
                ["base-only"] = "b",
                ["level"] = "2"
            });
        }
        [Fact]
        public void ShouldGetTagsFromProperty()
        {
            var property = typeof(PropertyTaggedClass).GetProperty(nameof(PropertyTaggedClass.Name));
            var tags = TagAttribute.GetTags(property);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["sensitive"] = "true"
            });
        }
        [Fact]
        public void ShouldGetTagsFromMethod()
        {
            var method = typeof(MethodTaggedClass).GetMethod(nameof(MethodTaggedClass.Run));
            var tags = TagAttribute.GetTags(method);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["role"] = "admin"
            });
        }
        [Fact]
        public void ShouldGetTagsFromBaseMethodAndOverrideByDerivedMethod()
        {
            var method = typeof(DerivedMethodClass).GetMethod(nameof(DerivedMethodClass.Run));
            var tags = TagAttribute.GetTags(method);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["role"] = "user",
                ["keep"] = "yes"
            });
        }
        [Fact]
        public void ShouldGetTagsFromAssembly()
        {
            var assembly = typeof(TagAttributeTest).Assembly;
            var tags = TagAttribute.GetTags(assembly);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["asm-tag"] = "asm-value"
            });
        }
        [Fact]
        public void ShouldSupportNonStringTagValue()
        {
            var tags = TagAttribute.GetTags(typeof(NonStringTagClass));
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["order"] = 10,
                ["enabled"] = true,
                ["ratio"] = 0.5
            });
        }
        [Fact]
        public void ShouldThrowWhenDuplicateKeyOnSameTarget()
        {
            Action action = () => TagAttribute.GetTags(typeof(DuplicateTagClass));
            action.Should().Throw<InvalidOperationException>().WithMessage("*Duplicate tag key*");
        }

        [Fact]
        public void ShouldInheritAllBaseTagsWhenDerivedTypeHasNoTags()
        {
            var tags = TagAttribute.GetTags(typeof(EmptyDerivedClass));
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["category"] = "base",
                ["base-only"] = "b",
                ["level"] = "1"
            });
        }
        [Fact]
        public void ShouldOverrideTagsAcrossMultiLevelInheritanceChain()
        {
            var tags = TagAttribute.GetTags(typeof(GrandChildClass));
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["from-base"] = "base",
                ["from-child"] = "grand-child",
                ["from-grand-child"] = "gc"
            });
        }
        [Fact]
        public void ShouldGetTagsFromBasePropertyAndOverrideByDerivedProperty()
        {
            var property = typeof(DerivedPropertyClass).GetProperty(nameof(DerivedPropertyClass.Name));
            var tags = TagAttribute.GetTags(property);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["sensitive"] = "false",
                ["keep"] = "yes"
            });
        }
        [Fact]
        public void ShouldInheritBasePropertyTagsWhenOverrideHasNoTags()
        {
            var property = typeof(EmptyOverridePropertyClass).GetProperty(nameof(EmptyOverridePropertyClass.Name));
            var tags = TagAttribute.GetTags(property);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["sensitive"] = "true",
                ["keep"] = "yes"
            });
        }
        [Fact]
        public void ShouldInheritBaseMethodTagsWhenOverrideHasNoTags()
        {
            var method = typeof(EmptyOverrideMethodClass).GetMethod(nameof(EmptyOverrideMethodClass.Run));
            var tags = TagAttribute.GetTags(method);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["role"] = "admin",
                ["keep"] = "yes"
            });
        }
        [Fact]
        public void ShouldNotInheritBaseMethodTagsWhenHiddenByNew()
        {
            var method = typeof(NewHiddenMethodClass).GetMethod(nameof(NewHiddenMethodClass.Run));
            var tags = TagAttribute.GetTags(method);
            tags.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["role"] = "guest"
            });
        }

        [Tag("category", "user")]
        [Tag("level", "1")]
        private class TaggedClass { }
        [Tag("category", "base")]
        [Tag("base-only", "b")]
        [Tag("level", "1")]
        private class BaseClass { }
        [Tag("category", "derived")]
        [Tag("level", "2")]
        private class DerivedClass : BaseClass { }
        private class NoTagClass { }
        private class PropertyTaggedClass
        {
            [Tag("sensitive", "true")]
            public string Name { get; set; }
        }
        private class MethodTaggedClass
        {
            [Tag("role", "admin")]
            public void Run() { }
        }
        private class BaseMethodClass
        {
            [Tag("role", "admin")]
            [Tag("keep", "yes")]
            public virtual void Run() { }
        }
        private class DerivedMethodClass : BaseMethodClass
        {
            [Tag("role", "user")]
            public override void Run() { }
        }
        [Tag("dup", "a")]
        [Tag("dup", "b")]
        private class DuplicateTagClass { }
        [Tag("order", 10)]
        [Tag("enabled", true)]
        [Tag("ratio", 0.5)]
        private class NonStringTagClass { }
        private class EmptyDerivedClass : BaseClass { }
        [Tag("from-base", "base")]
        [Tag("from-child", "base")]
        private class GrandBaseClass { }
        [Tag("from-child", "child")]
        private class ChildClass : GrandBaseClass { }
        [Tag("from-child", "grand-child")]
        [Tag("from-grand-child", "gc")]
        private class GrandChildClass : ChildClass { }
        private class BasePropertyClass
        {
            [Tag("sensitive", "true")]
            [Tag("keep", "yes")]
            public virtual string Name { get; set; }
        }
        private class DerivedPropertyClass : BasePropertyClass
        {
            [Tag("sensitive", "false")]
            public override string Name { get; set; }
        }
        private class EmptyOverridePropertyClass : BasePropertyClass
        {
            public override string Name { get; set; }
        }
        private class EmptyOverrideMethodClass : BaseMethodClass
        {
            public override void Run() { }
        }
        private class NewHiddenMethodClass : BaseMethodClass
        {
            [Tag("role", "guest")]
            public new void Run() { }
        }
    }
}
