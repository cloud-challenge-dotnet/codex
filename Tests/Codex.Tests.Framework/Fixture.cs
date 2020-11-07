using MongoDB.Bson.Serialization.Conventions;
using System;

namespace Codex.Tests.Framework
{
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