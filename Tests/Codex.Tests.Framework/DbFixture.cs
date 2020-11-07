using Codex.Core.Extensions;
using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Codex.Tests.Framework
{
    public class DbFixture : IDisposable
    {
        public DbFixture(
            MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService,
            IServiceProvider services)
        {
            _mongoDbSettings = mongoDbSettings;
            _tenantAccessService = tenantAccessService;
            _services = services;

            var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);
        }

        private readonly MongoDbSettings _mongoDbSettings;
        private readonly ITenantAccessService _tenantAccessService;
        private readonly IServiceProvider _services;

        public IServiceProvider Services
        {
            get => _services;
        }

        public string GetDatabaseName(string tenantId) => $"{_mongoDbSettings.DatabaseName}-{tenantId}";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            DropDatabaseAsync().GetAwaiter().GetResult();
        }

        private async Task DropDatabaseAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();
            if (string.IsNullOrWhiteSpace(tenant?.Id))
            {
                throw new ArgumentNullException("Id", "TenantId must be not null or whitespace");
            }

            var client = new MongoClient(this._mongoDbSettings.ConnectionString);
            client.DropDatabase(GetDatabaseName(tenant.Id));
        }

        public async Task UseDataSetAsync(
            params string[] locations)
        {
            var tenant = await _tenantAccessService.GetTenantAsync();
            if (string.IsNullOrWhiteSpace(tenant?.Id))
            {
                throw new ArgumentNullException("TenantId", nameof(Tenant.Id));
            }

            await DropDatabaseAsync();

            foreach (string location in locations)
            {
                if (File.Exists(location))
                {
                    using FileStream fs = File.OpenRead(location);

                    using JsonDocument document = await JsonDocument.ParseAsync(fs);

                    var client = new MongoClient(this._mongoDbSettings.ConnectionString);
                    var database = client.GetDatabase(GetDatabaseName(tenant.Id));

                    foreach (JsonElement element in document.RootElement.EnumerateArray())
                    {
                        var collectionName = element.GetProperty("collectionName").GetString();

                        if (collectionName != null)
                        {
                            var collection = database.GetCollection<BsonDocument>(collectionName.ToCamelCase());

                            foreach (JsonElement jsonElement in element.GetProperty("data").EnumerateArray())
                            {
                                var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonElement.ToString());

                                if (bsonDocument != null)
                                {
                                    await collection.InsertOneAsync(bsonDocument);
                                }
                            }
                        }                        
                    }
                }
                else
                {
                    throw new FileNotFoundException(location);
                }
            }
        }
    }
}