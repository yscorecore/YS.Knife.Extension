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
    }
}
