using Volo.Abp.Modularity;

namespace abp_obs_project;

/* Inherit from this class for your domain layer tests. */
public abstract class abp_obs_projectDomainTestBase<TStartupModule> : abp_obs_projectTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
