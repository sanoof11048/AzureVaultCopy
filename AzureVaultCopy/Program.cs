
using AzureVaultCopy.Data;
using AzureVaultCopy.Services;

namespace AzureVaultCopy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();

            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            // Add services to the container.
            builder.Services.AddSingleton<DapperContext>();
            builder.Services.AddHostedService<ApiKeyRotationService>();
            builder.Services.AddScoped<IApiKeyService, ApiKeyService>();



            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

           
                app.UseSwagger();
                app.UseSwaggerUI();


            //app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
