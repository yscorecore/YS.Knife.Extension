
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest
{
    public class MapperTest : YS.Knife.Hosting.KnifeHost
    {
        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddFlyTigerMapper(typeof(MapperTest).Assembly);
        }

        [Fact]
        public void ShouldGetIQueryableMapperService()
        {
            this.GetService<IQuerableMapper>().Should().NotBeNull();
        }

        [Fact]
        public void ShouldMapperQuerableWhenDefineMapperAttribute()
        {
            var service = this.GetService<IQuerableMapper>();
            var source = new User1[] { new User1 { Name = "Tom", Age = 18 } }.AsQueryable();
            var target = service.MapQuery<User1, User2>(source).ToArray();
            target.Should().BeEquivalentTo(new User2[] { new User2 { Name = "Tom", Age = 18 } });
        }
        [Fact]
        public void ShouldThrowErrorWhenMapperQuerable_NotDefineMapperAttribute()
        {
            var service = this.GetService<IQuerableMapper>();
            var source = new User1[] { new User1 { Name = "Tom", Age = 18 } }.AsQueryable();
            var action = () => service.MapQuery<User1, User3>(source).ToArray();
            action.Should().ThrowExactly<Exception>().WithMessage("Can not find query mapper from YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User1 to YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User3");
        }
        [Fact]
        public void ShouldGetIConvertMapperMapperService()
        {
            this.GetService<IConvertMapper>().Should().NotBeNull();
        }

        [Fact]
        public void ShouldMapperConvertWhenDefineMapperAttribute()
        {
            var service = this.GetService<IConvertMapper>();
            var source = new User1 { Name = "Tom", Age = 18 };
            var target = service.Convert<User1, User2>(source);
            target.Should().BeEquivalentTo(new User2 { Name = "Tom", Age = 18 });
        }
        [Fact]
        public void ShouldThrowErrorWhenMapperConvert_NotDefineMapperAttribute()
        {
            var service = this.GetService<IConvertMapper>();
            var source = new User1 { Name = "Tom", Age = 18 };
            var action = () => service.Convert<User1, User3>(source);
            action.Should().ThrowExactly<Exception>().WithMessage("Can not find convert mapper from YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User1 to YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User3");
        }

        [Fact]
        public void ShouldGetICopyMapperMapperService()
        {
            this.GetService<ICopyMapper>().Should().NotBeNull();
        }

        [Fact]
        public void ShouldMapperCopyWhenDefineMapperAttribute()
        {
            var service = this.GetService<ICopyMapper>();
            var source = new User1 { Name = "Tom", Age = 18 };
            var target = new User2();
            service.Copy(source, target);
            target.Should().BeEquivalentTo(new User2 { Name = "Tom", Age = 18 });
        }
        [Fact]
        public void ShouldThrowErrorWhenMapperCopy_NotDefineMapperAttribute()
        {
            var service = this.GetService<ICopyMapper>();
            var source = new User1 { Name = "Tom", Age = 18 };
            var target = new User3();
            var action = () => service.Copy(source, target);
            action.Should().ThrowExactly<Exception>().WithMessage("Can not find copy mapper from YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User1 to YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest.MapperTest+User3");
        }

        public record User1
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        public record User2
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        public record User3
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
