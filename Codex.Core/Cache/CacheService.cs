using Codex.Core.Models;
using Dapr.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Core.Cache
{
    [ExcludeFromCodeCoverage]
    public abstract class CacheService<T>
    {
        private readonly int ExpireTimeInMinutes;

        protected CacheService(int expireTimeInMinutes)
        {
            ExpireTimeInMinutes = expireTimeInMinutes;
        }

        public virtual async Task UpdateCacheAsync(DaprClient daprClient, string cacheKey, T data)
        {
            var state = await daprClient.GetStateEntryAsync<StateData<T>?>(ConfigConstant.CodexStoreName, cacheKey);
            state.Value = new StateData<T>(data, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(), ExpireTimeInMinutes);
            await state.SaveAsync();
        }

        public virtual async Task ClearCacheAsync(DaprClient daprClient, string cacheKey)
        {
            await daprClient.DeleteStateAsync(ConfigConstant.CodexStoreName, cacheKey);
        }

        private static bool IsExpiredCache(StateData<T>? stateData)
        {
            if (stateData == null)
                return true;

            if (stateData.ExpireTimeInMinutes <= 0)
                return false;

            return new DateTimeOffset(DateTime.UtcNow) >= DateTimeOffset.FromUnixTimeSeconds(stateData.CreationTimestamp).AddMinutes(stateData.ExpireTimeInMinutes);
        }

        public virtual async Task<T?> GetCacheAsync(DaprClient daprClient, string cacheKey)
        {
            var state = await daprClient.GetStateEntryAsync<StateData<T>?>(ConfigConstant.CodexStoreName, cacheKey);

            if (state.Value == null || IsExpiredCache(state.Value))
            {
                if (state.Value != null)
                    await daprClient.DeleteStateAsync(ConfigConstant.CodexStoreName, cacheKey);

                return default;
            }
            else
            {
                return state.Value.Data;
            }
        }
    }
}
