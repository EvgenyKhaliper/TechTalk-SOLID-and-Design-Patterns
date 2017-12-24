using System;
using Microsoft.Extensions.Caching.Memory;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Liskov substitution principle
    // “objects in a program should be replaceable with instances of their subtypes without altering the correctness of that program.”    
    
    public interface IUserSettingsDomainServiceLSP
    {
        UserSettings SaveUserSettings(UserSettings userSettings);
        UserSettings GetUserSettings(Guid userId);
    }
    
    public class UserSettingsDomainServiceLSP : IUserSettingsDomainServiceLSP
    {
        private ICache<UserSettings> _cache;
        private IStore<UserSettings> _store;
        
        public UserSettingsDomainServiceLSP()
        {
            _cache = new UserSettingsCache();
            _store = new AliasCorrectingUserSettingsStore();
        }   

        public UserSettings SaveUserSettings(UserSettings userSettings)
        {
            var userId = userSettings.UserId;
            var selectedUserSettings = _store.Get(userId);

            if (selectedUserSettings != null)
            {
                _store.Update(new UserSettings
                {
                    UserId = userId,
                    Alias = userSettings.Alias,
                    SendNewsletter = selectedUserSettings.SendNewsletter
                });
            }
            else
            {
                _store.Add(new UserSettings
                {
                    UserId = userId,
                    Alias = userSettings.Alias,
                    SendNewsletter = true
                }); 
            }

            var freshUserSettings = _store.Get(userId);
            _cache.Set(freshUserSettings);
            return freshUserSettings;
        }

        public UserSettings GetUserSettings(Guid userId)
        {
            var userSettings = _cache.Get(userId);
            
            if (userSettings != null) return userSettings;
            
            userSettings = _store.Get(userId);
            
            if (userSettings == null)
            {
                throw new Exception("user not found");
            }
       
            _cache.Set(userSettings);
                
            return userSettings;
        }
    }
    
    public class Cas : ICache<UserSettings> {

        private IMemoryCache _cache;
        public Cas()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(10)
            });           
        }
        
        public UserSettings Get(Guid id)
        {
            _cache.TryGetValue($"{id}:settings", out UserSettings userSettings);
            return userSettings;
        }

        public void Set(UserSettings item)
        {
            _cache.Set($"{item.UserId}:settings", item,
                new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1)));
        }
        
        public bool IsStale()
        {
            
        }
    }
}