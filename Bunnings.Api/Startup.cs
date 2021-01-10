using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using AspNetCore.Authentication.ApiKey;
using Azure.Storage.Blobs;
using Bunnings.Api.Filters;
using Bunnings.Api.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace Bunnings.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.SuppressAsyncSuffixInActionNames = false;
                    options.Filters.Add<ExceptionFilter>();
                })
                .AddFluentValidation(opt =>
                {
                    opt.RegisterValidatorsFromAssemblyContaining(typeof(Startup));
                })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
                .AddApiKeyInHeaderOrQueryParams<ApiKeyProvider>(options =>
                {
                    options.Realm = "Bunnings API";
                    options.KeyName = "X-API-KEY";
                });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddSingleton(provider =>
            {
                var blobContainerClient =
                    new BlobContainerClient(Configuration["ConnectionStrings:PeopleStore"], "people");
                blobContainerClient.CreateIfNotExists();

                return blobContainerClient;
            });

            services.AddSingleton(provider =>
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:PeopleStore"]);
                    var tableClient = storageAccount.CreateCloudTableClient();

                    Console.WriteLine("Create a Table for the demo");

                    // Create a table client for interacting with the table service 
                    var table = tableClient.GetTableReference("people");
                    table.CreateIfNotExists();
                    return tableClient;
                }
                catch (FormatException)
                {
                    Console.WriteLine(
                        "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                    throw;
                }
                catch (ArgumentException)
                {
                    Console.WriteLine(
                        "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                    Console.ReadLine();
                    throw;
                }
            });

            services.AddSingleton<IPeopleService, PeopleService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Bunnings API",
                    Description = "Bunnings API",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Dinuka Jayamuni",
                        Email = string.Empty,
                        Url = new Uri("https://www.linkedin.com/in/dinuka-jayamuni-82b8a13/"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });
                c.ExampleFilters();
                c.AddFluentValidationRules();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Enter your Api Key below:",
                    Name = "X-API-KEY",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                        },
                        new List<string>()
                    }
                });
            });
            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bunnings API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
