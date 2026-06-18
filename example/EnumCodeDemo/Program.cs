
using YS.Knife.EnumCode;

namespace EnumCodeDemo
{
    [YS.Knife.ExposeApi(typeof(IEnumCodeService))]
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }
    }
}
