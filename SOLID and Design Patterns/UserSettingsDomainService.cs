using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace SOLID_and_Design_Patterns
{    
    public interface IUserSettingsDomainService
    {
        void SaveUserSettings(UserSettings userSettings);
        UserSettings GetUserSettings(Guid userId);
    }
    
    public class UserSettingsDomainService : IUserSettingsDomainService
    {
        private IMemoryCache _cache;
        private string _connectionString;
        
        public UserSettingsDomainService()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(10)
            });

            _connectionString = "localhost:666;admin;admin;sa_role=true;";
        }   

        public void SaveUserSettings(UserSettings userSettings)
        {
            // Last minute feature/hack.
            // Sounds familiar ?
            var alias = userSettings.Alias;
            if (alias.Length < 5 || !Regex.IsMatch(alias, "^[a-zA-Z][a-zA-Z0-9]*$"))
            {
                alias = AliasGenerator.New();
            }

            var userId = userSettings.UserId;
            var selectQuery = DatabaseQueryMaker.CreateSelectUserSettings(userId);
            var selectedUserSettings = DatabaseQueryExecutor.Query<UserSettings>(_connectionString, selectQuery);

            if (selectedUserSettings != null)
            {
                var updateCommand = DatabaseQueryMaker.CreateUpdateUserSettings(userId,
                    alias,
                    selectedUserSettings.SendNewsletter);
                DatabaseQueryExecutor.Update<UserSettings>(_connectionString, updateCommand);
            }
            else
            {
                var insertCommand = DatabaseQueryMaker.CreateInsertUserSettings(userId, 
                    alias,
                    userSettings.SendNewsletter);
                DatabaseQueryExecutor.Insert<UserSettings>(_connectionString, insertCommand);
            }
        }

        public UserSettings GetUserSettings(Guid userId)
        {
            var found = _cache.TryGetValue($"{userId}:settings", out UserSettings userSettings);
            if (!found)
            {
                var query = DatabaseQueryMaker.CreateSelectUserSettings(userId);
                userSettings = DatabaseQueryExecutor.Query<UserSettings>(_connectionString, query);
            
                if (userSettings == null)
                {
                    throw new Exception("user not found");
                }
       
                _cache.Set($"{userId}:settings", 
                    userSettings, 
                    new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1)));

                return userSettings;
            }
            else
            {
                return userSettings;
            }
        }
    }
}