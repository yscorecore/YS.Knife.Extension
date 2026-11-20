using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using YS.Knife.Hosting;

namespace YS.Knife.Export.ClosedXml.IntegrationTest
{
    public class NPOIExportTest : KnifeHost
    {
        [Fact]
        public void ShouldGetExportService()
        {
            var service = this.GetService<IExportService>();
            service.Should().NotBeNull();
        }
        private EntityMetadata[] GetExportMetaData()
        {
            return new EntityMetadata[] {
                new EntityMetadata {
                    Name="user",
                    DisplayName ="用户",
                    Columns = new List<EntityColumnMetadata>
                    {
                        new EntityColumnMetadata{ Name="name", DisplayName="用户名", DataType = DataType.String},
                        new EntityColumnMetadata{ Name="age", DisplayName="年龄", DataType = DataType.Number},
                    }
                } };
        }
        private EntityData GetExportData()
        {
            return new EntityData
            {
                Name = "user",
                Datas = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        ["name"]="zhangsan",
                        ["age"]=13
                    }
                }
            };
        }

        [Fact]
        public async Task ShouldBeginExport()
        {
            var service = this.GetService<IExportService>();
            var token = await service.BeginExport(GetExportMetaData());
            token.Should().NotBeNull();
            token.ExpiredIn.Should().BeGreaterThan(1);

        }

        [Fact]
        public async Task ShouldCancelExportSuccess()
        {
            var service = this.GetService<IExportService>();
            var token = await service.BeginExport(GetExportMetaData());
            var res2 = async () => { await service.CancelExport(token.Token); };
            await res2.Should().NotThrowAsync();
        }
        [Fact]
        public async Task ShouldCancelExportFailureWhenCancelTwice()
        {
            var service = this.GetService<IExportService>();
            var token = await service.BeginExport(GetExportMetaData());
            _ = await service.CancelExport(token.Token);
            var res2 = async () => { await service.CancelExport(token.Token); };
            await res2.Should().ThrowAsync<Exception>().WithMessage("Token不存在");
        }

        [Fact]
        public async Task ShouldExportData()
        {
            var service = this.GetService<IExportService>();
            var token = await service.BeginExport(GetExportMetaData());
            await service.Export(token.Token, GetExportData());
            var stream = await service.EndExport(token.Token);
            var users = MiniExcel.Query<UserInfo>(stream).ToList();
            users.Count.Should().Be(1);
        }
        [Fact]
        public async Task ShouldExportDataWhenMultiExport()
        {
            var service = this.GetService<IExportService>();
            var token = await service.BeginExport(GetExportMetaData());
            await service.Export(token.Token, GetExportData());
            await service.Export(token.Token, GetExportData());
            await service.Export(token.Token, GetExportData());
            await service.Export(token.Token, GetExportData());
            await service.Export(token.Token, GetExportData());
            using var stream = await service.EndExport(token.Token);
            var users = MiniExcel.Query<UserInfo>(stream).ToList();
            users.Count.Should().Be(5);

        }

        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddDistributedMemoryCache();
        }
        public record UserInfo
        {
            [ExcelColumnName("用户名")]
            public string Name { get; set; }
            [ExcelColumnName("年龄")]
            public int Age { get; set; }
        }
    }
}
