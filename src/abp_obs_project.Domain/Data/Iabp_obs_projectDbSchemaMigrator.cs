using System.Threading.Tasks;

namespace abp_obs_project.Data;

public interface Iabp_obs_projectDbSchemaMigrator
{
    Task MigrateAsync();
}
