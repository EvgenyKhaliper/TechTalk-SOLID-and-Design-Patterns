using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Single responsibility principle
    // a class should have only a single responsibility 
    // (i.e. changes to only one part of the software's specification should be able to affect the specification of the class).
    
    public class UserSettingsDomainServiceSRP : IUserSettingsDomainService
    {
        private ICache<UserSettings> _cache;
        private IStore<UserSettings> _store;
        
        public UserSettingsDomainServiceSRP()
        {
            _cache = new UserSettingsCache();
            _store = new UserSettingsStore();
        }   

        public void SaveUserSettings(UserSettings userSettings)
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
    
    public interface IAliasAutoCorrected
    {
        string Correct(string alias);
    }

    public class AliasAutoCorrected : IAliasAutoCorrected
    {
        public string Correct(string alias)
        {
            if (alias.Length < 5 || !Regex.IsMatch(alias, "^[a-zA-Z][a-zA-Z0-9]*$"))
            {
                alias = AliasGenerator.New();
            }

            return alias;
        }
    }
    
    
    public interface IStore<T>
    {
        T Get(Guid id);
        void Add(T item);
        void Update(T item);
    }

    public class UserSettingsStore : IStore<UserSettings>
    {
        private string _connectionString;
        private IAliasAutoCorrected _correcter;
        public UserSettingsStore()
        {
            _connectionString = "localhost:666;admin;admin;sa_role=true;";
            _correcter = new AliasAutoCorrected();
        }

        public UserSettings Get(Guid id)
        {
            var query = DatabaseQueryMaker.CreateSelectUserSettings(id);
            return DatabaseQueryExecutor.Query<UserSettings>(_connectionString, query);
        }

        public void Add(UserSettings item)
        {
            var insertCommand = DatabaseQueryMaker.CreateInsertUserSettings(item.UserId, 
                _correcter.Correct(item.Alias),
                item.SendNewsletter);
            DatabaseQueryExecutor.Insert<UserSettings>(_connectionString, insertCommand);
        }

        public void Update(UserSettings item)
        {
            var updateCommand = DatabaseQueryMaker.CreateUpdateUserSettings(item.UserId,
                _correcter.Correct(item.Alias),
                item.SendNewsletter);
            DatabaseQueryExecutor.Update<UserSettings>(_connectionString, updateCommand);
        }
    }

    public interface ICache<T>
    {
        T Get(Guid id);
        void Set(T item);
    }

    public class UserSettingsCache : ICache<UserSettings>
    {        
        private IMemoryCache _cache;
        public UserSettingsCache()
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
                    .SetSlidingExpiration(TimeSpan.FromHours(1)));
        }
    }
}