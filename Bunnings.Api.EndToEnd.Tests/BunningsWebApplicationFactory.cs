using System.Collections.Generic;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bunnings.Api.EndToEnd.Tests
{
    public class BunningsWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly BlobContainerClient _blobContainerClient;
        public BlobClient BlobClient { get; }

        public BunningsWebApplicationFactory()
        {
            _blobContainerClient = Substitute.For<BlobContainerClient>();
            BlobClient = Substitute.For<BlobClient>();
            _blobContainerClient.GetBlobClient(Arg.Any<string>()).Returns(BlobClient);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        ["ApiKey"] = "DummyKey"
                    });
            });
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(provider => _blobContainerClient);
            });
        }
    }
}
