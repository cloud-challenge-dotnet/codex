using Codex.Core.Tools.AutoMapper;
using Codex.Tests.Framework;

namespace Codex.Tenants.Api.Tests
{
    public class TenantPropertiesServiceIT : IClassFixture<Fixture>
    {
        private readonly IMapper _mapper;

        public TenantPropertiesServiceIT()
        {
            //auto mapper configuration
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile(new CoreMappingProfile());
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Fact]
        public async Task UpdateProperties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1", null))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenant = await tenantPropertiesService.UpdatePropertiesAsync("Id1", new());

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task UpdateProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1", null))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenant = await tenantPropertiesService.UpdatePropertyAsync("Id1", "data", new());

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task DeleteProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1", null))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenant = await tenantPropertiesService.DeletePropertyAsync("Id1", "data");

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task FindProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id", "name",
                    new Dictionary<string, List<string>>()
                    {
                        { "data", new() { "" } }
                    }
                ))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.NotNull(tenantProperties);
        }

        [Fact]
        public async Task FindProperty_Null_Property()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id", "name",
                    null
                ))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.Null(tenantProperties);
        }


        [Fact]
        public async Task FindProperty_Null_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)null)
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.Null(tenantProperties);
        }

        [Fact]
        public async Task FindProperties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id", "name", new()))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.NotNull(tenantProperties);
        }

        [Fact]
        public async Task FindProperties_Null_Properties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id1", "name", null))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.Null(tenantProperties);
        }

        [Fact]
        public async Task FindProperties_Null_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantRow?)null)
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object, _mapper);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.Null(tenantProperties);
        }
    }
}
