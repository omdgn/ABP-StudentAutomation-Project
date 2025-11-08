using System;
using System.Collections.Generic;
using System.Text;
using abp_obs_project.Localization;
using Volo.Abp.Application.Services;

namespace abp_obs_project;

/* Inherit your application services from this class.
 */
public abstract class abp_obs_projectAppService : ApplicationService
{
    protected abp_obs_projectAppService()
    {
        LocalizationResource = typeof(abp_obs_projectResource);
    }
}
