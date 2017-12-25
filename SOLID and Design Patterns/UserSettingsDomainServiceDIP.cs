using System;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Dependency inversion principle
    // one should “depend upon abstractions, [not] concretions.”
    
    public class UserSettingsDomainServiceDIP : IUserSettingsDomainService
    {
        private readonly IQuery<UserSettings> _query;
        private readonly ICommand<UserSettings> _command;

        public UserSettingsDomainServiceDIP(IQuery<UserSettings> query, ICommand<UserSettings> command)
        {
            _query = query;
            _command = command;
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
    
    
    public class FlexibleDICache : IQuery<UserSettings>
    {
        private IQuery<UserSettings> _query;
        private ICache<UserSettings> _command;
        
        public FlexibleDICache(IQuery<UserSettings> query, ICache<UserSettings> command)
        {
            _query = query;
            _command = command;
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

    public class UserSettingsFlexibleDIStore : 
        IQuery<UserSettings>, 
        ICommand<UserSettings>
    {
        private IStore<UserSettings> _store;
        
        public UserSettingsFlexibleDIStore(IStore<UserSettings> store)
        {
            _store = store;
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