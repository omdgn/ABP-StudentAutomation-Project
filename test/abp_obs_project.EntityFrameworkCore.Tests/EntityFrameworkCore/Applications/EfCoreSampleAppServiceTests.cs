using abp_obs_project.Samples;
using Xunit;

namespace abp_obs_project.EntityFrameworkCore.Applications;

[Collection(abp_obs_projectTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<abp_obs_projectEntityFrameworkCoreTestModule>
{

}
