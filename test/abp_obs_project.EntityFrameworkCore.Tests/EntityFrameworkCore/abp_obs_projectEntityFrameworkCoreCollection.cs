using Xunit;

namespace abp_obs_project.EntityFrameworkCore;

[CollectionDefinition(abp_obs_projectTestConsts.CollectionDefinitionName)]
public class abp_obs_projectEntityFrameworkCoreCollection : ICollectionFixture<abp_obs_projectEntityFrameworkCoreFixture>
{

}
