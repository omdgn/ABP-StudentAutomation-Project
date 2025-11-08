using Volo.Abp.Modularity;

namespace abp_obs_project;

[DependsOn(
    typeof(abp_obs_projectDomainModule),
    typeof(abp_obs_projectTestBaseModule)
)]
public class abp_obs_projectDomainTestModule : AbpModule
{

}
