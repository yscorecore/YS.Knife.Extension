using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xunit;
using YS.Knife.AI.Impl.Default;

namespace YS.Knife.AI.Impl.Default.UnitTest
{
    // 测试用的类定义（必须是 public 且定义在外部）

    public class SimpleTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ClassWithDescriptionAttribute
    {
        [Description("Name of the person")]
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ClassWithDisplayAttribute
    {
        [Display(Name = "FullName", Description = "The full name", Order = 1)]
        public string Name { get; set; }
    }

    public class ClassWithNestedType
    {
        public string Title { get; set; }
        public NestedType Nested { get; set; }
    }

    public class NestedType
    {
        public int Id { get; set; }
        public string Data { get; set; }
    }

    public class ClassWithGenericType
    {
        public System.Collections.Generic.List<NestedType> Items { get; set; }
    }

    public class ClassWithArrayType
    {
        public NestedType[] Items { get; set; }
    }

    public class ClassWithNullableType
    {
        public int? NullableInt { get; set; }
        public DateTime? NullableDate { get; set; }
    }

    public struct TestStruct
    {
        public double Value { get; set; }
        public string Label { get; set; }
    }

    public class ClassWithSystemTypes
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public System.Collections.Generic.List<string> StringList { get; set; }
        public System.DateTime DateProperty { get; set; }
    }

    public class ClassDefinitionGeneratorTest
    {
        private readonly ClassDefinitionGenerator _generator = new ClassDefinitionGenerator();

        [Fact]
        public void GetClassDefinition_WithSimpleClass_GeneratesCorrectDefinition()
        {
            // Arrange
            var type = typeof(SimpleTestClass);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public class SimpleTestClass", result);
            Assert.Contains("public string Name { get; set; }", result);
            Assert.Contains("public int Age { get; set; }", result);
        }

        [Fact]
        public void GetClassDefinition_WithDescriptionAttribute_GeneratesAttribute()
        {
            // Arrange
            var type = typeof(ClassWithDescriptionAttribute);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("[Description(\"Name of the person\")]", result);
        }

        [Fact]
        public void GetClassDefinition_WithDisplayAttribute_GeneratesAttribute()
        {
            // Arrange
            var type = typeof(ClassWithDisplayAttribute);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("[Display(", result);
            Assert.Contains("Name = \"FullName\"", result);
            Assert.Contains("Description = \"The full name\"", result);
            Assert.Contains("Order = 1", result);
        }

        [Fact]
        public void GetClassDefinition_WithNestedTypeDependency_GeneratesAllTypes()
        {
            // Arrange
            var type = typeof(ClassWithNestedType);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public class ClassWithNestedType", result);
            Assert.Contains("public class NestedType", result);
            Assert.Contains("public NestedType Nested { get; set; }", result);
        }

        [Fact]
        public void GetClassDefinition_WithGenericType_GeneratesCorrectGenericSyntax()
        {
            // Arrange
            var type = typeof(ClassWithGenericType);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public List<NestedType>", result);
            Assert.Contains("public class NestedType", result);
        }

        [Fact]
        public void GetClassDefinition_WithArrayType_GeneratesCorrectArraySyntax()
        {
            // Arrange
            var type = typeof(ClassWithArrayType);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public NestedType[] Items { get; set; }", result);
        }

        [Fact]
        public void GetClassDefinition_WithNullableType_HandlesNullable()
        {
            // Arrange
            var type = typeof(ClassWithNullableType);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public int? NullableInt { get; set; }", result);
            Assert.Contains("public DateTime? NullableDate { get; set; }", result);
        }

        [Fact]
        public void GetClassDefinition_WithStructType_GeneratesStructDefinition()
        {
            // Arrange
            var type = typeof(TestStruct);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.Contains("public struct TestStruct", result);
            Assert.Contains("public double Value { get; set; }", result);
        }

        [Fact]
        public void GetClassDefinition_WithSystemTypes_IgnoresSystemTypes()
        {
            // Arrange
            var type = typeof(ClassWithSystemTypes);

            // Act
            var result = _generator.GetClassDefinition(type);

            // Assert
            Assert.DoesNotContain("public class String", result);
            Assert.DoesNotContain("public class Int32", result);
            Assert.DoesNotContain("public class List", result);
        }
    }
}