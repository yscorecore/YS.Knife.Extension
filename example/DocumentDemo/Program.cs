
using YS.Knife.Document;

namespace DocumentDemo
{
    [YS.Knife.ExposeApi(typeof(IDocTypeConvertService))]
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
