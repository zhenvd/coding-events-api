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
      ResourceLink.ServerOrigin = Configuration["Server:Origin"];

      services.AddControllers();

      // register repositories
      services.AddScoped<ICodingEventRepository, CodingEventRepository>();
      services.AddScoped<ITagRepository, TagRepository>();

      // register services
      services.AddScoped<IMemberService, MemberService>();
      services.AddScoped<IOwnerService, OwnerService>();
      services.AddScoped<IAuthedUserService, AuthedUserService>();
      services.AddScoped<IPublicAccessService, PublicAccessService>();
      services.AddScoped<ICodingEventTagService, CodingEventTagService>();

      // authenticate using JWT Bearer
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        // TODO: insert values for the empty properties of the JWT.ADB2C object in appsettings.json 
        // ValidAudience: on azure ADB2C > applications > (next to application name)
        // ex: "06eb34fd-455b-4084-92c3-07d5389e6c15"
        // MetadataAddress: on azure ADB2C > User flows > (select flow) > Run user flow (the link at the top right)
        // ex: https://{instance}/{domain}/v2.0/.well-known/openid-configuration?p={flow policy}
        // ex: "https://mycodingevents.b2clogin.com/mycodingevents.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_code_events_signup_signin"
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

          // meta annotations for endpoints in UI
          options.EnableAnnotations();

          // req/res body examples
          options.ExampleFilters();

          // source of truth for reference used in SecurityRequirement and SecurityDefinition
          const string securityId = "adb2c";

          // instructs swagger to add token as Authorization header (Bearer <token>)
          options.AddSecurityRequirement(
            new OpenApiSecurityRequirement {
              {
                new OpenApiSecurityScheme {
                  Reference = new OpenApiReference {
                    Id = securityId, // reference
                    Type = ReferenceType.SecurityScheme,
                  },
                  UnresolvedReference = true,
                },
                new List<string>()
              }
            }
          );

          // TODO: insert values for the empty properties of the SwaggerAuth object in appsettings.json 
          // define the oauth flow for swagger to use
          options.AddSecurityDefinition(
            securityId, // matching reference
            new OpenApiSecurityScheme {
              Type = SecuritySchemeType.OAuth2,
              Flows = new OpenApiOAuthFlows {
                Implicit = new OpenApiOAuthFlow {
                  AuthorizationUrl = new System.Uri(
                    Configuration["SwaggerAuth:AuthorizationUrl"], // where to begin the token flow
                    // ex: https://{instance}/{domain}/oauth2/v2.0/authorize?p={flow policy}
                    // ex: https://mycodingevents.b2clogin.com/mycodingevents.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1_code_events_signup_signin
                    System.UriKind.Absolute // external to the API must be absolute not relative
                  ),
                  Scopes = new Dictionary<string, string> {
                    {
                      Configuration["SwaggerAuth:Scopes:UserImpersonation"], // openid token scope
                      // ex: https://{domain}/{app name}/{published scope name}
                      // ex: https://mycodingevents.onmicrosoft.com/code-events/user_impersonation
                      "Access the Coding Events API on behalf of signed in User"
                    }
                  }
                }
              }
            }
          );
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
          options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coding Events API Documentation");
          options.OAuthClientId(Configuration["SwaggerAuth:ClientId"]); // to auto-populate in UI
        }
      );

      // run migrations on startup
      var dbContext = app.ApplicationServices.CreateScope()
        .ServiceProvider.GetService<CodingEventsDbContext>();
      dbContext.Database.Migrate();
    }
  }
}
