using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace abp_obs_project.Data;

/* This is used if database provider does't define
 * Iabp_obs_projectDbSchemaMigrator implementation.
 */
public class Nullabp_obs_projectDbSchemaMigrator : Iabp_obs_projectDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
