using Microsoft.EntityFrameworkCore;
using Switcharoo.Database;
using Switcharoo.Entities;
using Switcharoo.Interfaces;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo;

public sealed class EfRepository(SwitcharooContext context) : IRepository
{
    public async Task<(bool isActive, bool wasFound)> GetFeatureStateAsync(string featureName, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Name == featureName && x.Environments.Any(y => y.Environment.Id == environmentKey));

        if (feature is null)
        {
            return (false, false);
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey);

        return featureEnvironment is null ? (false, false) : (featureEnvironment.IsEnabled, true);
    }

    public async Task<(bool isActive, bool wasChanged, string reason)> ToggleFeatureAsync(Guid featureKey, Guid environmentKey, Guid authKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Id == featureKey && x.Environments.Any(y => y.Environment.Id == environmentKey));

        if (feature is null)
        {
            return (false, false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey);

        if (featureEnvironment is null)
        {
            return (false, false, "Environment not found");
        }

        featureEnvironment.IsEnabled = !featureEnvironment.IsEnabled;
        await context.SaveChangesAsync();

        return (featureEnvironment.IsEnabled, true, string.Empty);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddFeatureAsync(string featureName, string description, Guid authKey)
    {
        var user = await context.Users.Include(x => x.Features).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var feature = user.Features.SingleOrDefault(x => x.Name == featureName);

        if (feature is not null)
        {
            return (false, Guid.Empty, "Feature already exists");
        }

        feature = new Entities.Feature()
        {
            Name = featureName,
            Description = description,
        };

        user.Features.Add(feature);
        await context.SaveChangesAsync();

        return (true, feature.Id, string.Empty);
    }

    public async Task<(bool wasAdded, string reason)> AddEnvironmentToFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Id == featureKey);

        if (feature is null)
        {
            return (false, "Feature not found");
        }

        var environment = await context.Environments.SingleOrDefaultAsync(x => x.Id == environmentKey);

        if (environment is null)
        {
            return (false, "Environment not found");
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey);

        if (featureEnvironment is not null)
        {
            return (false, "Environment already exists");
        }

        featureEnvironment = new Entities.FeatureEnvironment()
        {
            Environment = environment,
            IsEnabled = false,
        };

        feature.Environments.Add(featureEnvironment);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool deleted, string reason)> DeleteFeatureAsync(Guid featureKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Id == featureKey);

        if (feature is null)
        {
            return (false, "Feature not found");
        }

        context.Features.Remove(feature);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool deleted, string reason)> DeleteEnvironmentFromFeatureAsync(Guid featureKey, Guid environmentKey)
    {
        var feature = await context.Features.Include(x => x.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Id == featureKey);

        if (feature is null)
        {
            return (false, "Feature not found");
        }

        var featureEnvironment = feature.Environments.SingleOrDefault(x => x.Environment.Id == environmentKey);

        if (featureEnvironment is null)
        {
            return (false, "Environment not found");
        }

        feature.Environments.Remove(featureEnvironment);
        await context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool wasAdded, Guid key, string reason)> AddEnvironmentAsync(string environmentName, Guid authKey)
    {
        var user = await context.Users.Include(x => x.Environments).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return (false, Guid.Empty, "User not found");
        }

        var environment = user.Environments.SingleOrDefault(x => x.Name == environmentName);

        if (environment is not null)
        {
            return (false, Guid.Empty, "Environment already exists");
        }

        environment = new Environment { Id = Guid.NewGuid(), Name = environmentName };

        user.Environments.Add(environment);
        await context.SaveChangesAsync();

        return (true, environment.Id, string.Empty);
    }

    public async Task<(bool wasFound, List<Environment> environments, string reason)> GetEnvironmentsAsync(Guid authKey)
    {
        var user = await context.Users.Include(x => x.Environments).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return (false, new List<Environment>(), "User not found");
        }

        return (true, user.Environments, string.Empty);
    }

    public async Task<(bool wasFound, List<Feature> features, string reason)> GetFeaturesAsync(Guid authKey)
    {
        var user = await context.Users.Include(x => x.Features).ThenInclude(feature => feature.Environments).ThenInclude(featureEnvironment => featureEnvironment.Environment).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return (false, new List<Feature>(), "User not found");
        }

        return (true, user.Features, string.Empty);
    }

    public async Task<bool> IsAdminAsync(Guid authKey)
    {
        return await context.Users.AnyAsync(x => x.Id == authKey.ToString());
    }

    public async Task<bool> IsFeatureAdminAsync(Guid featureKey, Guid authKey)
    {
        var user = await context.Users.Include(x => x.Features).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return false;
        }

        return user.Features.Any(x => x.Id == featureKey);
    }

    public async Task<bool> IsEnvironmentAdminAsync(Guid environmentKey, Guid authKey)
    {
        var user = await context.Users.Include(x => x.Environments).SingleOrDefaultAsync(x => x.Id == authKey.ToString());

        if (user is null)
        {
            return false;
        }

        return user.Environments.Any(x => x.Id == environmentKey);
    }
}
