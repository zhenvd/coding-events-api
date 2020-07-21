using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace CodingEventsAPI {
  public class Program {
    public static void Main(string[] args) {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) {
      return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(
          (context, config) => {
            // only use keyvault in production, user-secrets for local dev
            if (!context.HostingEnvironment.IsProduction()) return;
            
            var builtConfig = config.Build();

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
              new KeyVaultClient.AuthenticationCallback(
                azureServiceTokenProvider.KeyVaultTokenCallback
              )
            );

            // TODO: set KeyVaultName in appsettings.json
            config.AddAzureKeyVault(
              $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/",
              keyVaultClient,
              new DefaultKeyVaultSecretManager()
            );
          }
        )
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
  }
}
