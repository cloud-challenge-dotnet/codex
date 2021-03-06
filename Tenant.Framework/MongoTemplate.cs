﻿using Codex.Core.Extensions;
using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Resources;
using Microsoft.Extensions.Localization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework
{
    [ExcludeFromCodeCoverage]
    public class MongoTemplate<TDocument, TId>
    {
        MongoClient? _mongoClient;
        IMongoDatabase? _database;
        private readonly MongoDbSettings _mongoDbSettings;
        private readonly ITenantAccessService _tenantAccessService;
        private readonly string _collectionName;
        private readonly IStringLocalizer<TenantFrameworkResource> _sl;

        public MongoTemplate(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService,
            IStringLocalizer<TenantFrameworkResource> sl)
        {
            _tenantAccessService = tenantAccessService;
            _mongoDbSettings = mongoDbSettings;
            _sl = sl;

            _collectionName = (typeof(TDocument)).Name.ToCamelCase();
            if (_collectionName.EndsWith("Row"))
            {
                _collectionName = _collectionName[0..^3];
            }
        }

        public MongoClient MongoClient
        {
            get => _mongoClient ??= new MongoClient(_mongoDbSettings.ConnectionString);
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

        public async Task<IMongoCollection<TDocument>> GetRepositoryAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException(_sl[TenantFrameworkResource.TENANT_NOT_FOUND]!);

            return GetDatabase(tenant.Id).GetCollection<TDocument>(_collectionName);
        }

        public async Task<bool> ExistsByIdAsync(TId id)
        {
            var repository = await GetRepositoryAsync();

            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return (await repository.CountDocumentsAsync(filter)) > 0;
        }
        public async Task<TDocument?> FindOneAsync(TId id)
        {
            var repository = await GetRepositoryAsync();

            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return await repository.Find(filter).Limit(1).SingleOrDefaultAsync();
        }

        public virtual async Task<TDocument> InsertAsync(TDocument document)
        {
            var repository = await GetRepositoryAsync();

            await repository.InsertOneAsync(document);

            return document;
        }

        public async Task DeleteAsync(TId id)
        {
            var repository = await GetRepositoryAsync();

            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            await repository.DeleteOneAsync(filter);
        }

        public async Task DropDatabaseAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException(_sl[TenantFrameworkResource.TENANT_NOT_FOUND]!);

            await MongoClient.DropDatabaseAsync(GetDatabaseName(tenant.Id));
        }

        public async Task DropCollectionAsync()
        {
            var tenant = await _tenantAccessService.GetTenantAsync();

            if (string.IsNullOrWhiteSpace(tenant?.Id))
                throw new ArgumentException(_sl[TenantFrameworkResource.TENANT_NOT_FOUND]!);

            await GetDatabase(tenant.Id).DropCollectionAsync(_collectionName);
        }
    }
}
