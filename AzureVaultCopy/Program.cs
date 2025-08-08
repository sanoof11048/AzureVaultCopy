
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

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException("Missing DB connection string");

            builder.Services.AddSingleton(new DapperContext(connectionString));

            builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
            builder.Services.AddSingleton<IHostedService, ApiKeyRotationService>();
            builder.Services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());



            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

           
                app.UseSwagger();
                app.UseSwaggerUI();


            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }


            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
