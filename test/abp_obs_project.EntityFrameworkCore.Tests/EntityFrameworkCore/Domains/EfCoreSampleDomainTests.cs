using abp_obs_project.Samples;
using Xunit;

namespace abp_obs_project.EntityFrameworkCore.Domains;

[Collection(abp_obs_projectTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<abp_obs_projectEntityFrameworkCoreTestModule>
{

}
