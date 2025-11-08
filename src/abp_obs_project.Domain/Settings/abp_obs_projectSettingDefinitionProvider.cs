using Volo.Abp.Settings;

namespace abp_obs_project.Settings;

public class abp_obs_projectSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(abp_obs_projectSettings.MySetting1));
    }
}
