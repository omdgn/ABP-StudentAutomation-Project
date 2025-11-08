using Volo.Abp.Modularity;

namespace abp_obs_project;

[DependsOn(
    typeof(abp_obs_projectApplicationModule),
    typeof(abp_obs_projectDomainTestModule)
)]
public class abp_obs_projectApplicationTestModule : AbpModule
{

}
