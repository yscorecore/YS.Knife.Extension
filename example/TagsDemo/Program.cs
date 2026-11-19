using Microsoft.EntityFrameworkCore;

using YS.Knife.Hosting;

namespace TagsDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);

            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<TagDbContext>(op =>
            {
                op.UseSqlite("Data Source=abc.db.tmp").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            // 初始化数据库
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TagDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
