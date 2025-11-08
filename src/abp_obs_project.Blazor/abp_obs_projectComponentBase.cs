using abp_obs_project.Localization;
using Volo.Abp.AspNetCore.Components;

namespace abp_obs_project.Blazor;

public abstract class abp_obs_projectComponentBase : AbpComponentBase
{
    protected abp_obs_projectComponentBase()
    {
        LocalizationResource = typeof(abp_obs_projectResource);
    }
}
