using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YS.Knife.EFCore.UnitTest
{
    [AutoConstructor]
    public partial class JsonContentAttributeTest : BaseTest
    {
        #region GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary
        [Fact]
        public void Should_GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary()
        {
            var model = GetModel<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Context>();
            var entity = model.GetEntityTypes().First();
            var property = entity.GetProperty(nameof(GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Entity.Value));
            property.GetColumnType().Should().Be("TEXT");
            property.GetValueConverter().Should().NotBeNull();
            property.GetValueComparer().Should().NotBeNull();
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Context : BaseContext
        {
            public GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Context(DbContextOptions<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Context> options) : base(options)
            {
            }
            public DbSet<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Entity> TestEntities { get; set; }
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnDictionary_Entity
        {
            public int Id { get; set; }
            [JsonContent]
            // [Column(TypeName = "TEXT")]
            public Dictionary<string, string> Value { get; set; }
        }
        #endregion


        #region GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList
        [Fact]
        public void Should_GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList()
        {
            var model = GetModel<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Context>();
            var entity = model.GetEntityTypes().First();
            var property = entity.GetProperty(nameof(GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Entity.Value));
            property.GetColumnType().Should().Be("TEXT");
            property.GetValueConverter().Should().NotBeNull();
            property.GetValueComparer().Should().NotBeNull();
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Context : BaseContext
        {
            public GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Context(DbContextOptions<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Context> options) : base(options)
            {
            }
            public DbSet<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Entity> TestEntities { get; set; }
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnList_Entity
        {
            public int Id { get; set; }
            [JsonContent]
            // [Column(TypeName = "TEXT")]
            public List<object> Value { get; set; }
        }
        #endregion


        #region GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject
        [Fact]
        public void Should_GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject()
        {
            var model = GetModel<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Context>();
            var entity = model.GetEntityTypes().First();
            var property = entity.GetProperty(nameof(GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Entity.Value));
            property.GetColumnType().Should().Be("TEXT");
            property.GetValueConverter().Should().NotBeNull();
            property.GetValueComparer().Should().NotBeNull();
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Context : BaseContext
        {
            public GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Context(DbContextOptions<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Context> options) : base(options)
            {
            }
            public DbSet<GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Entity> TestEntities { get; set; }
        }

        private class GetValueConvertAndValueComparer_When_DefineJsonContentAttribute_OnObject_Entity
        {
            public int Id { get; set; }
            [JsonContent]
            // [Column(TypeName = "TEXT")]
            public SafeOrderConfig Value { get; set; }
        }

        public class SafeOrderConfig
        {
            public int Period { get; set; }

            public int[] Values { get; set; }
        }
        #endregion


        #region NamingPolicy
        [Fact]
        public void Should_Use_Specified_NamingPolicy_When_Serialize_JsonContent()
        {
            var model = GetModel<NamingPolicy_Context>();
            var entity = model.GetEntityTypes().First();

            Convert(nameof(NamingPolicy_Entity.CamelValue), new NamingPolicy_Config { UserName = "foo", UserAge = 18 })
                .Should().Be("{\"userName\":\"foo\",\"userAge\":18}");
            Convert(nameof(NamingPolicy_Entity.PascalValue), new LowerCamelCaseConfig { userName = "foo", userAge = 18 })
                .Should().Be("{\"UserName\":\"foo\",\"UserAge\":18}");
            Convert(nameof(NamingPolicy_Entity.SnakeValue), new NamingPolicy_Config { UserName = "foo", UserAge = 18 })
                .Should().Be("{\"user_name\":\"foo\",\"user_age\":18}");
            Convert(nameof(NamingPolicy_Entity.DefaultValue), new NamingPolicy_Config { UserName = "foo", UserAge = 18 })
                .Should().Be("{\"UserName\":\"foo\",\"UserAge\":18}");

            string Convert(string propertyName, object value)
            {
                return (string)entity.GetProperty(propertyName).GetValueConverter().ConvertToProvider(value);
            }
        }

        private class NamingPolicy_Context : BaseContext
        {
            public NamingPolicy_Context(DbContextOptions<NamingPolicy_Context> options) : base(options)
            {
            }
            public DbSet<NamingPolicy_Entity> TestEntities { get; set; }
        }

        private class NamingPolicy_Entity
        {
            public int Id { get; set; }
            [JsonContent(NamingPolicy = JsonPropertyNamingPolicy.CamelCase)]
            public NamingPolicy_Config CamelValue { get; set; }
            [JsonContent(NamingPolicy = JsonPropertyNamingPolicy.PascalCase)]
            public LowerCamelCaseConfig PascalValue { get; set; }
            [JsonContent(NamingPolicy = JsonPropertyNamingPolicy.SnakeCaseLower)]
            public NamingPolicy_Config SnakeValue { get; set; }
            [JsonContent]
            public NamingPolicy_Config DefaultValue { get; set; }
        }

        public class NamingPolicy_Config
        {
            public string UserName { get; set; }
            public int UserAge { get; set; }
        }

        public class LowerCamelCaseConfig
        {
            public string userName { get; set; }
            public int userAge { get; set; }
        }
        #endregion
    }
}
