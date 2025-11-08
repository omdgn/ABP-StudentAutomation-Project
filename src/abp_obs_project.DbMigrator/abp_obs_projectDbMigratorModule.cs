using abp_obs_project.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace abp_obs_project.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(abp_obs_projectEntityFrameworkCoreModule),
    typeof(abp_obs_projectApplicationContractsModule)
    )]
public class abp_obs_projectDbMigratorModule : AbpModule
{
}
