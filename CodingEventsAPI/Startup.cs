using System.Collections.Generic;
using CodingEventsAPI.Controllers;
using CodingEventsAPI.Data;
using CodingEventsAPI.Data.Repositories;
using CodingEventsAPI.Middleware;
using CodingEventsAPI.Services;
using CodingEventsAPI.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodingEventsAPI {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      // set the ServerOrigin for Resource links
      ResourceLink.ServerOrigin = Configuration["Server:Origin"];

      services.AddControllers();

      // register repositories
      services.AddScoped<ITagRepository, TagRepository>();
      services.AddScoped<ICodingEventRepository, CodingEventRepository>();

      // register services
      services.AddScoped<IMemberService, MemberService>();
      services.AddScoped<IOwnerService, OwnerService>();
      services.AddScoped<IAuthedUserService, AuthedUserService>();
      services.AddScoped<IPublicAccessService, PublicAccessService>();
      services.AddScoped<ICodingEventTagService, CodingEventTagService>();

      // authenticate using JWT Bearer
      // TODO: insert values for the empty properties of the JWTOptions object in appsettings.json 
      services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => Configuration.Bind("JWTOptions", options));
      // binds the appsettings.json JWTOptions JSON object to the JWTBearerOptions object
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions?view=aspnetcore-3.0
      // this is a shorthand approach to externalize the configuration in appsettings
  
      // notice each JWTOptions[optionProperty] corresponds to the JWTBearerOptions object properties from the documentation
        // ex: JWTOptions.Audience in appsettings is assigned to options.Audience
      // even complex nested objects are bound
        // ex: JWTOptions.TokenValidationParameters has its object entries automatically bound to options.TokenValidationParameters
          // as options.TokenValidationParameters = new TokenValidationParameters { property = value, ... }

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

          // meta annotations for endpoints in UI
          options.EnableAnnotations();

          // req/res body examples
          options.ExampleFilters();
        }
      );

      // register swagger example response objects
      services.AddSwaggerExamplesFromAssemblyOf<TagExample>();
      services.AddSwaggerExamplesFromAssemblyOf<TagsExample>();
      services.AddSwaggerExamplesFromAssemblyOf<NewTagExample>();
      services.AddSwaggerExamplesFromAssemblyOf<MemberExample>();
      services.AddSwaggerExamplesFromAssemblyOf<MembersExample>();
      services.AddSwaggerExamplesFromAssemblyOf<CodingEventExample>();
      services.AddSwaggerExamplesFromAssemblyOf<CodingEventsExample>();
      services.AddSwaggerExamplesFromAssemblyOf<NewCodingEventExample>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        IdentityModelEventSource.ShowPII = true; // show protected PII for debugging in development
      }

      app.UseRouting();
      // must come after UseRouting
      app.UseAuthentication();
      app.UseAuthorization(); // Authorization is implicit for any Authenticated request

      app.UseMiddleware<AddUserIdClaimMiddleware>();

      // and before UseEndpoints
      app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

      // configure swagger and UI page
      app.UseSwagger();
      app.UseSwaggerUI(
        options => {
          options.RoutePrefix = ""; // root path of the server, "/", will display swagger docs
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coding Events API Documentation");
        }
      );

      // run migrations on startup
      var dbContext = app.ApplicationServices.CreateScope()
        .ServiceProvider.GetService<CodingEventsDbContext>();
      dbContext.Database.Migrate();
    }
  }
}

