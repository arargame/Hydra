using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{
    public interface ICustomConfigurationService
    {
        string Get(string key, string? defaultValue = null);

        T Get<T>(string key, T defaultValue = default!);

        bool TryGet<T>(string key, out T value);
    }
    public class CustomConfigurationService : ICustomConfigurationService
    {
        private readonly IConfiguration _configuration; // appsettings + env variables
        private readonly ISecretManager _secretManager; // Secret store örneği

        public CustomConfigurationService(IConfiguration configuration, ISecretManager secretManager)
        {
            _configuration = configuration;
            _secretManager = secretManager;
        }

        public string Get(string key, string? defaultValue = null)
        {
            // Öncelik: secret manager > config
            var secretValue = _secretManager.GetSecret(key);
            if (!string.IsNullOrEmpty(secretValue))
                return secretValue;

            var configValue = _configuration[key];
            return !string.IsNullOrEmpty(configValue) ? configValue : defaultValue ?? string.Empty;
        }

        public T Get<T>(string key, T defaultValue = default!)
        {
            var valStr = Get(key);
            if (string.IsNullOrEmpty(valStr))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(valStr, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            var val = Get<T>(key);
            if (val == null || val.Equals(default(T)))
            {
                value = default!;
                return false;
            }
            value = val;
            return true;
        }
    }

    public interface ISecretManager
    {
        string? GetSecret(string key);
        Task<string?> GetSecretAsync(string key);
    }

    public class LocalSecretManager : ISecretManager
    {
        private readonly IConfiguration _configuration;

        public LocalSecretManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string? GetSecret(string key)
        {
            return _configuration[$"Secrets:{key}"];
        }

        public Task<string?> GetSecretAsync(string key)
        {
            return Task.FromResult(GetSecret(key));
        }
    }

}
