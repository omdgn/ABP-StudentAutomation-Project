using abp_obs_project.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace abp_obs_project.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class abp_obs_projectController : AbpControllerBase
{
    protected abp_obs_projectController()
    {
        LocalizationResource = typeof(abp_obs_projectResource);
    }
}
