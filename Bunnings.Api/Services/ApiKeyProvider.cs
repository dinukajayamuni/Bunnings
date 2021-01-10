using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bunnings.Api.Services
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        private readonly ILogger<IApiKeyProvider> _logger;
        private readonly IConfiguration _configuration;

        public ApiKeyProvider(ILogger<IApiKeyProvider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task<IApiKey> ProvideAsync(string key)
        {
            try
            {
                return Task.FromResult<IApiKey>(new ApiKey(_configuration["ApiKey"], "Consumer"));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                throw;
            }
        }
    }

    public class ApiKey : IApiKey
    {
        public ApiKey(string key, string owner, List<Claim> claims = null)
        {
            Key = key;
            OwnerName = owner;
            Claims = claims ?? new List<Claim>();
        }

        public string Key { get; }
        public string OwnerName { get; }
        public IReadOnlyCollection<Claim> Claims { get; }
    }
}
