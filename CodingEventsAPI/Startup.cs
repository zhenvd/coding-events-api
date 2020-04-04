using CodingEventsAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
<<<<<<< HEAD
using Microsoft.OpenApi.Models;
=======
using Microsoft.IdentityModel.Tokens;
>>>>>>> adb2c: JWT bearer config and appsettings TODO

namespace CodingEventsAPI {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.AddControllers();

      // authenticate using JWT Bearer
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        // TODO: insert values for the empty properties of the JWT.ADB2C object in appsettings.json 
        // ValidAudience: on azure ADB2C > applications > (next to application name)
        // ex: "06eb34fd-455b-4084-92c3-07d5389e6c15"
        // MetadataAddress: on azure ADB2C > User flows > (select flow) > Run user flow (the link at the top right)
        // ex: "https://mycodeevents.b2clogin.com/mycodeevents.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_code_events_signup_signin"
        .AddJwtBearer(options => Configuration.Bind("JWT:ADB2C", options));
      // binds the appsettings.json JWT.ADB2C JSON object to the JwtBearer options object
      // we group ADB2C under a top level JWT key so that other providers can be used if needed
      // notice the JWT.ADB2C[optionProperty] corresponds to the options object properties
      // ex: JWT.ADB2C.RequireHttpsMetadata value is assignd to options.RequireHttpsMetadata
      // even complex nested objects are binded
      // ex: JWT.ADB2C.TokenValidationParameters has its object entries automatically bound to
      // options.TokenValidationParameters = new TokenValidationParameters { property = value, ... }

      var connectionString = Configuration.GetConnectionString("Default");
      services.AddDbContext<CodingEventsDbContext>(o => o.UseMySql(connectionString));

      services.AddSwaggerGen(
        options => {
          options.SwaggerDoc(
            "v1",
            new OpenApiInfo {
              Version = "v1",
              Title = "Coding Events API",
              Description = "REST API for managing Coding Events"
            }
          );
        }
      );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();
      // must come after UseRouting
      app.UseAuthentication();
      app.UseAuthorization(); // Authorization is implicit for any Authenticated request
      // and before UseEndpoints
      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

      app.UseSwagger();
      app.UseSwaggerUI(
        options => {
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Events API Documentation");
        }
      );

      // run migrations on startup
      var dbContext = app.ApplicationServices.CreateScope()
        .ServiceProvider.GetService<CodingEventsDbContext>();
      dbContext.Database.Migrate();
    }
  }
}
