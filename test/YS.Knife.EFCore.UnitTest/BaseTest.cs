using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit.Abstractions;

namespace YS.Knife.EFCore.UnitTest
{
    [AutoConstructor]
    public partial class BaseTest
    {
        private readonly ITestOutputHelper outputHelper;

        protected IMutableModel GetModel<T>()
        where T : BaseContext
        {
            var options = new DbContextOptionsBuilder<T>();
            options.UseSqlite("Data Source=:memory:").EnableSensitiveDataLogging(true)
                .LogTo((p) => outputHelper.WriteLine(p));
            var context = Activator.CreateInstance(typeof(T), options.Options) as T;
            context.Database.EnsureCreated();
            return context.ModelMetadata;
        }
    }
}
