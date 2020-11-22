using Codex.Core.Extensions;
using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework
{
    public class MongoTemplate<TDocument>
    {
        MongoClient? _mongoClient;
        IMongoDatabase? _database;
        private readonly MongoDbSettings _mongoDbSettings;
        private readonly ITenantAccessService _tenantAccessService;

        public MongoTemplate(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService)
        {
            _tenantAccessService = tenantAccessService;
            _mongoDbSettings = mongoDbSettings;
        }

        public static ClusterBuilder ConfigureCluster(ClusterBuilder builder)
        {
#if TRACE
            var traceSource = new TraceSource(nameof(MongoTemplate<TDocument>), SourceLevels.Verbose);
            builder.TraceWith(traceSource);
            builder.TraceCommandsWith(traceSource);
#endif
            return builder;
        }

        public MongoClient MongoClient
        {
            get => _mongoClient ??= new MongoClient(new MongoClientSettings()
            {
                
                Server = new MongoServerAddress("localhost"),
                ClusterConfigurator = cb =>
                {
                    cb.Subscribe<CommandStartedEvent>(e =>
                    {
                        Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                    });
                }
            }); //_mongoDbSettings.ConnectionString);
        }

        public string GetDatabaseName(string tenantId) => $"{_mongoDbSettings.DatabaseName}-{tenantId}";

        public string GetMongoPropertyName(string classPropertyName) => classPropertyName.ToCamelCase();

        public IMongoDatabase GetDatabase(string tenantId)
        {
            if (_database == null)
            {
                var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
                ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);

                var database = MongoClient.GetDatabase(GetDatabaseName(tenantId));
                _database = database;
            }

            return _database;
        }

        public async Task<IMongoCollection<TDocument>> GetRepositoryAsync(){
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException("TenantId must be not null or whitespace");

            return GetDatabase(tenant.Id).GetCollection<TDocument>((typeof(TDocument).Name).ToCamelCase());
        }

        public async Task<bool> ExistsByIdAsync(string id)
        {
            var repository = await GetRepositoryAsync();

            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return (await repository.CountDocumentsAsync(filter)) > 0;
        }
        public async Task<TDocument?> FindOneAsync(string id)
        {
            var repository = await GetRepositoryAsync();

            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return await repository.Find(filter).Limit(1).SingleOrDefaultAsync();
        }

        public async Task<TDocument> InsertAsync(TDocument tenant)
        {
            var repository = await GetRepositoryAsync();

            await repository.InsertOneAsync(tenant);

            return tenant;
        }

        public async Task DropDatabaseAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException("TenantId must be not null or whitespace");

            await MongoClient.DropDatabaseAsync(GetDatabaseName(tenant.Id));
        }

        public async Task DropCollectionAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException("TenantId must be not null or whitespace");

            await GetDatabase(tenant.Id).DropCollectionAsync(typeof(TDocument).Name);
        }
    }
}
