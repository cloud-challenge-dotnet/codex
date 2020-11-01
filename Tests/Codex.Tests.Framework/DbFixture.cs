using Codex.Core.Models;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;
using Codex.Tests.Framework.Models;
using System.Runtime.InteropServices;
using Codex.Core.Extensions;

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

        MongoDbSettings _mongoDbSettings;
        ITenantAccessService _tenantAccessService;
        IServiceProvider _services;

        public IServiceProvider Services
        {
            get => _services;
        }

        public string GetDatabaseName(string tenantId) => $"{_mongoDbSettings.DatabaseName}-{tenantId}";

        public void Dispose()
        {
            DropDatabaseAsync().GetAwaiter().GetResult();
        }

        private async Task DropDatabaseAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();
            if (string.IsNullOrWhiteSpace(tenant?.Id))
            {
                throw new ArgumentNullException("TenantId");
            }

            var client = new MongoClient(this._mongoDbSettings.ConnectionString);
            client.DropDatabase(GetDatabaseName(tenant.Id));
        }

        public enum LoadStrategyEnum { CleanInsert }

        private async Task DropCollectionAsync(string collectionName)
        {
            var tenant = await _tenantAccessService.GetTenantAsync();
            if (string.IsNullOrWhiteSpace(tenant?.Id))
            {
                throw new ArgumentNullException("TenantId");
            }

            var client = new MongoClient(this._mongoDbSettings.ConnectionString);
            var database = client.GetDatabase(GetDatabaseName(tenant.Id));
            await database.DropCollectionAsync(collectionName);
        }

        public async Task UseDataSetAsync(
            LoadStrategyEnum loadStrategyEnum = LoadStrategyEnum.CleanInsert,
            params string[] locations)
        {
            var tenant = await _tenantAccessService.GetTenantAsync();
            if (string.IsNullOrWhiteSpace(tenant?.Id))
            {
                throw new ArgumentNullException("TenantId");
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