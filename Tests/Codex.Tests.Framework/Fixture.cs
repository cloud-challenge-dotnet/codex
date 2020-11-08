using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tests.Framework
{
    [ExcludeFromCodeCoverage]
    public class Fixture
    {
        public Fixture(IServiceProvider services)
        {
            _services = services;

            var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);
        }

        private readonly IServiceProvider _services;

        public IServiceProvider Services
        {
            get => _services;
        }
    }
}