using System;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Interface segregation principle
    // “many client-specific interfaces are better than one general-purpose interface.”
    
    public class UserSettingsDomainServiceISP : IUserSettingsDomainService
    {
        private IQuery<UserSettings> _query;
        private ICommand<UserSettings> _command;

        public UserSettingsDomainServiceISP()
        {
            _query = new FlexibleCache();
            _command = new UserSettingsFlexibleStore();
        }

        public void SaveUserSettings(UserSettings userSettings)
        {
            var userId = userSettings.UserId;
            var selectedUserSettings = _query.Get(userId);

            if (selectedUserSettings != null)
            {
                _command.Update(new UserSettings
                {
                    UserId = userId,
                    Alias = userSettings.Alias,
                    SendNewsletter = selectedUserSettings.SendNewsletter
                });
            }
            else
            {
                _command.Add(new UserSettings
                {
                    UserId = userId,
                    Alias = userSettings.Alias,
                    SendNewsletter = true
                });
            }
        }

        public UserSettings GetUserSettings(Guid userId)
        {
            return _query.Get(userId);
        }
    }

    public interface IQuery<T>
    {
        T Get(Guid id);
    }

    public interface ICommand<T>
    {
        void Add(T item);
        void Update(T item);
    }
    
    public class FlexibleCache : IQuery<UserSettings>
    {
        private IQuery<UserSettings> _query;
        private ICache<UserSettings> _command;
        
        public FlexibleCache()
        {
            _query = new UserSettingsFlexibleStore();
            _command = new UserSettingsFileCacheProxy();
        }
        
        public UserSettings Get(Guid id)
        {
            var userSettings = _command.Get(id);

            if (userSettings != null) return userSettings;

            userSettings = _query.Get(id);

            if (userSettings == null)
            {
                throw new Exception("user not found");
            }

            _command.Set(userSettings);

            return userSettings;
        }
    }

    public class UserSettingsFlexibleStore : 
        IQuery<UserSettings>, 
        ICommand<UserSettings>
    {
        private IStore<UserSettings> _store;
        
        public UserSettingsFlexibleStore()
        {            
            _store = new DecoratedAliasCorrectingUserSettingsStore();
        }

        public void Add(UserSettings item)
        {
            _store.Add(item);
        }

        public void Update(UserSettings item)
        {
            _store.Update(item);
        }

        public UserSettings Get(Guid id)
        {
            return _store.Get(id);
        }
    }
}