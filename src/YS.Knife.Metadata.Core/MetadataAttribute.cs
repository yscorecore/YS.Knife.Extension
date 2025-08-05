using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class MetadataAttribute : KnifeAttribute
    {
        public MetadataAttribute(string name) : base(null)
        {
            this.Name = name;
        }
        public string Name { get; }

        public override void RegisterService(IServiceCollection services, IRegisterContext context, Type declareType)
        {
            services.Configure<MetadataOptions>(t =>
            {
                var name = this.Name ?? declareType.FullName;
                t.AddMeta(name, declareType);
            });
        }
    }
}
