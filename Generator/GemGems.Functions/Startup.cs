using GemGems.App.Generator;
using GemGems.Azure.Blob;
using GemGems.Functions.Triggers.RPCTrigger;
using GemGems.Image;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(GemGems.Functions.Startup))]
namespace GemGems.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<RPCEventConfigProvider>();

            builder.Services.AddScoped<IStorageService, AzureStorageService>();
            builder.Services.AddScoped<IImageFactory, BitmapImageFactory>();

            builder.Services.AddOptions<NFTGeneratorConfig>()
              .Configure<IConfiguration>((settings, configuration) =>
              {
                  configuration.GetSection("Config").Bind(settings);
              });
        }
    }
}
