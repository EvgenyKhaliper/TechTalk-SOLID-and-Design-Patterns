using System;
using System.IO;
using Newtonsoft.Json;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Liskov substitution principle
    // “objects in a program should be replaceable with instances of their subtypes without altering the correctness of that program.”    

    // Bird
    //    .Fly()
    // Duck : Bird
    // Ostrich : Bird
    
    public class UserSettingsDomainServiceLSP : IUserSettingsDomainService
    {
        private ICache<UserSettings> _cache;
        private IStore<UserSettings> _store;

        public UserSettingsDomainServiceLSP()
        {
            _cache = new UserSettingsFileCache();
            _store = new DecoratedAliasCorrectingUserSettingsStore();
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

    public class UserSettingsFileCache : ICache<UserSettings>
    {
        private string _temp;

        public void Init()
        {
            _temp = Path.Combine(
                Path.GetTempPath(), 
                Guid.NewGuid().ToString());
        }

        public UserSettings Get(Guid id)
        {
            var path = Path.Combine(_temp, $"{id}:settings");
            return !File.Exists(path)
                ? new UserSettings()
                : JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(path));
        }

        public void Set(UserSettings item)
        {
            File.WriteAllText(Path.Combine(_temp, $"{item.UserId}:settings"), JsonConvert.SerializeObject(item));
        }
    }

    public class UserSettingsFileCacheProxy : ICache<UserSettings>
    {
        private IPathProvider _provider;
        public UserSettings Get(Guid id)
        {
            CreateProvider();

            return JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(_provider.Get($"{id}:settings")));
        }

        public void Set(UserSettings item)
        {
            CreateProvider();
            
            File.WriteAllText(_provider.Get($"{item.UserId}:settings"), JsonConvert.SerializeObject(item));
        }
        
        private void CreateProvider()
        {
            if (_provider == null)
            {
                _provider = new FilePathProvider();
            }
        }
    }

    public interface IPathProvider
    {
        string Get(string folder);
    }

    public class FilePathProvider : IPathProvider
    {
        public string Get(string folder)
        {
            var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            return Path.Combine(temp, folder);
        }
    }

}