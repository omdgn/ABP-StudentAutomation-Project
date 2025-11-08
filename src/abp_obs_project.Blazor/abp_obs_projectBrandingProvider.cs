using Microsoft.Extensions.Localization;
using abp_obs_project.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace abp_obs_project.Blazor;

[Dependency(ReplaceServices = true)]
public class abp_obs_projectBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<abp_obs_projectResource> _localizer;

    public abp_obs_projectBrandingProvider(IStringLocalizer<abp_obs_projectResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
