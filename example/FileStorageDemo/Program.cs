using Microsoft.AspNetCore.StaticFiles;

using YS.Knife.Hosting;

namespace FileStorageDemo
{
    [YS.Knife.ExposeApi(typeof(YS.Knife.FileStorage.IFileUploadWebService))]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);
            builder.Services.AddControllers();


            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions());

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
