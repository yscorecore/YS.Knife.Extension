using YS.Knife.Hosting;

namespace YS.Knife.BarCode.Impl.Barcodelib.IntegrationTest
{
    [AutoConstructor]
    public partial class BarCodeServiceTest : KnifeHost
    {
        private Xunit.Abstractions.ITestOutputHelper _output;
        [Fact]
        public void ShouldGetExportService()
        {
            var service = this.GetService<IBarCodeService>();
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldGenerateFile()
        {
            var service = this.GetService<IBarCodeService>();
            string tempPath = Path.GetTempPath();
            string tempFileName = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".jpg");
            await service.GenerateBarCodeToFile("240625000097131", 200, 60, tempFileName);
            _output.WriteLine($"code file generated. {tempFileName}");
            File.Exists(tempFileName).Should().BeTrue();
        }
    }
}
