using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YS.Knife.Hosting;

namespace YS.Knife.Metadata.Impl.Mvc.IntegrationTest
{
    public class MetadataServiceTest : KnifeWebHost
    {
        [Fact]
        public void ShouldGetMetadataService()
        {
            this.GetService<IMetadataService>()
                .Should().NotBeNull();
        }
        #region  PropertyPath
        [Fact]
        public async Task Should_GetMetadataInfo_PropertyPath()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_PropertyPath");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.PropertyPath.Should().Be("name");
        }
        [Metadata("GetMetadataInfo_PropertyPath")]
        internal class User_GetMetadataInfo_PropertyPath
        {
            public string Name { get; set; }
        }
        [Fact]
        public async Task Should_GetMetadataInfo_PropertyPath_CustomName()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_PropertyPath_CustomName");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.PropertyPath.Should().Be("name1");
        }
        [Metadata("GetMetadataInfo_PropertyPath_CustomName")]
        internal class User_GetMetadataInfo_PropertyPath_CustomName
        {
            [JsonPropertyName("name1")]
            public string Name { get; set; }
        }

        #endregion

        #region DisplayName
        [Fact]
        public async Task Should_GetMetadataInfo_DisplayName()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("User_GetMetadataInfo_DisplayName");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DisplayName.Should().Be("Name");
        }
        [Metadata("User_GetMetadataInfo_DisplayName")]
        class User_GetMetadataInfo_DisplayName
        {
            public string Name { get; set; }
        }
        [Fact]
        public async Task Should_GetMetadataInfo_DisplayName_When_Define_DisplayNameAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("User_GetMetadataInfo_DisplayName_When_Define_DisplayNameAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DisplayName.Should().Be("用户名");
        }
        [Metadata("User_GetMetadataInfo_DisplayName_When_Define_DisplayNameAttribute")]
        class User_GetMetadataInfo_DisplayName_When_Define_DisplayNameAttribute
        {
            [DisplayName("用户名")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_GetMetadataInfo_DisplayName_When_Define_DisplayAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_DisplayName_When_Define_DisplayAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DisplayName.Should().Be("用户名");
        }


        [Metadata("GetMetadataInfo_DisplayName_When_Define_DisplayAttribute")]
        class User_GetMetadataInfo_DisplayName_When_Define_DisplayAttribute
        {
            [Display(Name = "用户名")]
            public string Name { get; set; }
        }
        [Fact]
        public async Task Should_GetMetadataInfo_DisplayName_From_DisplayAttribute_When_Define_DisplayAttribute_And_DisplayNameAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("User_GetMetadataInfo_DisplayName_From_DisplayAttribute_When_Define_DisplayAttribute_And_DisplayNameAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DisplayName.Should().Be("from Display");
        }


        [Metadata("User_GetMetadataInfo_DisplayName_From_DisplayAttribute_When_Define_DisplayAttribute_And_DisplayNameAttribute")]
        class User_GetMetadataInfo_DisplayName_From_DisplayAttribute_When_Define_DisplayAttribute_And_DisplayNameAttribute
        {
            [DisplayName("from DisplayName")]
            [Display(Name = "from Display")]
            public string Name { get; set; }
        }



        #endregion

        #region Description
        [Fact]
        public async Task Should_GetMetadataInfo_Description_Null_When_No_Define_Any_Attribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_Description_Null_No_Define_Any_Attribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.Description.Should().BeNull();
        }
        [Metadata("GetMetadataInfo_Description_Null_No_Define_Any_Attribute")]
        internal class User_GetMetadataInfo_Description_Null_No_Define_Any_Attribute
        {
            [JsonPropertyName("name1")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_GetMetadataInfo_Description_When_Define_DescriptionAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_Description_When_Define_DescriptionAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.Description.Should().Be("desc1");
        }
        [Metadata("GetMetadataInfo_Description_When_Define_DescriptionAttribute")]
        internal class User_GetMetadataInfo_Description_When_Define_DescriptionAttribute
        {
            [Description("desc1")]
            public string Name { get; set; }
        }
        [Fact]
        public async Task Should_GetMetadataInfo_Description_When_Define_DisplayAttribute_WithDescriptionProperty()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_Description_When_Define_DisplayAttribute_WithDescriptionProperty");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.Description.Should().Be("desc1");
        }
        [Metadata("GetMetadataInfo_Description_When_Define_DisplayAttribute_WithDescriptionProperty")]
        internal class User_GetMetadataInfo_Description_When_Define_DisplayAttribute_WithDescriptionProperty
        {
            [Display(Description = "desc1")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_GetMetadataInfo_Description_FromDisplayAttribute_When_Define_DisplayAttribute_And_DescriptionAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_Description_FromDisplayAttribute_When_Define_DisplayAttribute_And_DescriptionAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.Description.Should().Be("from Display");
        }
        [Metadata("GetMetadataInfo_Description_FromDisplayAttribute_When_Define_DisplayAttribute_And_DescriptionAttribute")]
        internal class User_GetMetadataInfo_Description_FromDisplayAttribute_When_Define_DisplayAttribute_And_DescriptionAttribute
        {
            [Description("from Description")]
            [Display(Description = "from Display")]
            public string Name { get; set; }
        }
        #endregion

        #region NestedClass
        [Fact]
        public async Task Should_GetMetadataInfo_PropertyPath_FromNestedClass()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_PropertyPath_ByNestedClass");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.PropertyPath.Should().Be("class.name");
        }
        [Metadata("GetMetadataInfo_PropertyPath_ByNestedClass")]
        class User_GetMetadataInfo_PropertyPath_ByNestedClass
        {
            public NestedClass_GetMetadataInfo_PropertyPath_ByNestedClass Class { get; set; }
        }
        class NestedClass_GetMetadataInfo_PropertyPath_ByNestedClass
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task Should_GetMetadataInfo_ByNestedClass_WithLoop()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_ByNestedClass_WithLoop");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.PropertyPath.Should().Be("name");
        }
        [Metadata("GetMetadataInfo_ByNestedClass_WithLoop")]
        class User_GetMetadataInfo_PropertyPath_ByNestedClass_WithLoop
        {
            public string Name { get; set; }
            public User_GetMetadataInfo_PropertyPath_ByNestedClass_WithLoop Nested { get; set; }
        }

        #endregion

        #region DataSource

        [Fact]
        public async Task Should_GetMetadataInfo_DataSource_From_EditorDataSourceAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_DataSource_From_EditorDataSourceAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DataSource.Should().BeEquivalentTo(new DataSourceInfo
            {
                Value = "abc",
                Type = SourceType.DataSource
            });
        }
        [Metadata("GetMetadataInfo_DataSource_From_EditorDataSourceAttribute")]
        internal class User_GetMetadataInfo_DataSource_From_EditorDataSourceAttribute
        {
            [EditorDataSource("abc")]
            public string Name { get; set; }
        }
        #endregion

        #region EnumDataSource
        [Fact]
        public async Task Should_GetMetadataInfo_DataSource_From_EditorEnumSourceAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_DataSource_From_EditorEnumSourceAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DataSource.Should().BeEquivalentTo(new DataSourceInfo
            {
                Value = "abc",
                Type = SourceType.Enum
            });
        }
        [Metadata("GetMetadataInfo_DataSource_From_EditorEnumSourceAttribute")]
        internal class User_GetMetadataInfo_DataSource_From_EditorEnumSourceAttribute
        {
            [EditorEnumSource("abc")]
            public string Name { get; set; }
        }
        #endregion

        #region ConstDataSource
        [Fact]
        public async Task Should_GetMetadataInfo_DataSource_From_EditorConstantSourceAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_DataSource_From_EditorConstantSourceAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.DataSource.Should().NotBeNull();
            firstColumn.DataSource.Type.Should().Be(SourceType.Constant);
            firstColumn.DataSource.Value.Should().BeEquivalentTo(new object[] { "a", "b" });
        }
        [Metadata("GetMetadataInfo_DataSource_From_EditorConstantSourceAttribute")]
        internal class User_GetMetadataInfo_DataSource_From_EditorConstantSourceAttribute
        {
            [EditorConstantSource("a", "b")]
            public string Name { get; set; }
        }
        #endregion

        #region QueryFilter
        [Fact]
        public async Task Should_GetMetadataInfo_QueryFilter_From_QueryFilterAttribute()
        {
            var service = this.GetService<IMetadataService>();
            var info = await service.GetMetadataInfo("GetMetadataInfo_QueryFilter_From_QueryFilterAttribute");
            info.Columns.Count.Should().Be(1);
            var firstColumn = info.Columns.First();
            firstColumn.QueryFilter.Should().NotBeNull();
            firstColumn.QueryFilter.Should().BeEquivalentTo(new QueryFilterInfo
            {
                Operator = Operators.Equal,
                DefaultValue = "abc"
            });
        }
        [Metadata("GetMetadataInfo_QueryFilter_From_QueryFilterAttribute")]
        internal class User_GetMetadataInfo_QueryFilter_From_QueryFilterAttribute
        {
            [QueryFilter(Operators.Equal, DefaultValue = "abc")]
            public string Name { get; set; }
        }
        #endregion
    }
}
