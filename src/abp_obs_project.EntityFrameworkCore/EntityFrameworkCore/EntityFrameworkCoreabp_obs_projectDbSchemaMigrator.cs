using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using abp_obs_project.Data;
using Volo.Abp.DependencyInjection;

namespace abp_obs_project.EntityFrameworkCore;

public class EntityFrameworkCoreabp_obs_projectDbSchemaMigrator
    : Iabp_obs_projectDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreabp_obs_projectDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the abp_obs_projectDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<abp_obs_projectDbContext>()
            .Database
            .MigrateAsync();
    }
}
