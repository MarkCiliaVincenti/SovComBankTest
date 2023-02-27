﻿using Bank.ApiWebApp.Models;
using Bank.Services.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bank.ApiWebApp;

internal sealed class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    private IConfiguration Configuration { get; }

    private static Action<ApiBehaviorOptions> ConfigureApiBehaviorOptions => options =>
    {
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
            new ApiResult<IEnumerable<ValidationError>>(ValidationError.GetValidationErrors(context),
                "Bad Request"));
    };

    private static Action<ApiExplorerOptions> ConfigureApiExplorerOptions => options =>
    {
        options.DefaultApiVersion = VersionConfig.DefaultApiVersion;
        options.SubstituteApiVersionInUrl = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    };

    private static Action<ApiVersioningOptions> ConfigureApiVersionOptions => options =>
    {
        options.DefaultApiVersion = VersionConfig.DefaultApiVersion;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader(VersionConfig.ApiVersionParameterName),
            new HeaderApiVersionReader(VersionConfig.ApiVersionParameterName)
        );
        options.AssumeDefaultVersionWhenUnspecified = true;
    };

    private static Action<SwaggerGenOptions> ConfigureSwaggerGenOptions => swaggerGenOptions =>
    {
        swaggerGenOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"), true);

        AddSwaggerApiVersionDocs();
        swaggerGenOptions.UseAllOfToExtendReferenceSchemas();


        void AddSwaggerApiVersionDocs()
        {
            foreach (var existingVersion in VersionConfig.ExistingVersions)
            {
                swaggerGenOptions.SwaggerDoc(existingVersion,
                    new OpenApiInfo
                    {
                        Title = "SMS Send Web-Service.",
                        Version = existingVersion,
                        Contact = new OpenApiContact
                        {
                            Url = new Uri("https://example.com/"),
                            Name = "bank",
                            Email = "help@example.com"
                        },
                        Description = "SMS Invite Message Publish Web-Service.",
                        License = new OpenApiLicense
                        {
                            Url = new Uri(
                                "https://ru.wikipedia.org/wiki/%D0%9B%D0%B8%D1%86%D0%B5%D0%BD%D0%B7%D0%B8%D1%8F_MIT"),
                            Name = "GSM Format"
                        }
                    });
            }
        }
    };

    private static Action<MvcOptions> ConfigureMvcOptions => options =>
    {
        options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ApiResult<ErrorFeatures>),
            StatusCodes.Status500InternalServerError));
    };

    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddSmsService(options => Configuration.GetSection("SmsService").Bind(options))
            .AddVersionedApiExplorer(ConfigureApiExplorerOptions)
            .AddApiVersioning(ConfigureApiVersionOptions)
            .AddSwaggerGen(ConfigureSwaggerGenOptions)
            .AddControllers(ConfigureMvcOptions).ConfigureApiBehaviorOptions(ConfigureApiBehaviorOptions);

    public void Configure(IApplicationBuilder app) =>
        app
            .UseHttpsRedirection()
            .UseStaticFiles()
            .UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer>
                    {
                        new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}", Description = "Main Server" }
                    };
                });
            })
            .UseSwaggerUI(setup =>
            {
                foreach (var existingVersion in VersionConfig.ExistingVersions)
                {
                    setup.SwaggerEndpoint($"/swagger/{existingVersion}/swagger.json",
                        existingVersion);
                }

                setup.RoutePrefix = "help";
                setup.EnableValidator();
                setup.DocumentTitle = "SMS Invite Message Publish Web-Service.";
            })
            .UseExceptionHandler("/error")
            .UseStatusCodePagesWithReExecute("/error/{0}")
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
}
