using System;

namespace SOLID_and_Design_Patterns
{
    // Wikipedia
    // Open/closed principle
    // “software entities … should be open for extension, but closed for modification.”
    
    public class UserSettingsDomainServiceOCP : IUserSettingsDomainService
    {
        private ICache<UserSettings> _cache;
        private IStore<UserSettings> _store;
        
        public UserSettingsDomainServiceOCP()
        {
            _cache = new UserSettingsCache();
            _store = new AliasCorrectingUserSettingsStore();
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
    
    public class NonAliasCorrectingUserSettingsStore : IStore<UserSettings>
    {
        private string _connectionString;
        public NonAliasCorrectingUserSettingsStore()
        {
            _connectionString = "localhost:666;admin;admin;sa_role=true;";
        }

        public UserSettings Get(Guid id)
        {
            var query = DatabaseQueryMaker.CreateSelectUserSettings(id);
            return DatabaseQueryExecutor.Query<UserSettings>(_connectionString, query);
        }

        public virtual void Add(UserSettings item)
        {
            var insertCommand = DatabaseQueryMaker.CreateInsertUserSettings(item.UserId, 
                item.Alias,
                item.SendNewsletter);
            DatabaseQueryExecutor.Insert<UserSettings>(_connectionString, insertCommand);
        }

        public virtual void Update(UserSettings item)
        {
            var updateCommand = DatabaseQueryMaker.CreateUpdateUserSettings(item.UserId,
                item.Alias,
                item.SendNewsletter);
            DatabaseQueryExecutor.Update<UserSettings>(_connectionString, updateCommand);
        }
    }
    
    public class AliasCorrectingUserSettingsStore : NonAliasCorrectingUserSettingsStore
    {
        private IAliasAutoCorrected _correcter;
        public AliasCorrectingUserSettingsStore()
        {
            _correcter = new AliasAutoCorrected();
        }

        public override void Add(UserSettings item)
        {
            item.Alias = _correcter.Correct(item.Alias);
            base.Add(item);
        }

        public override void Update(UserSettings item)
        {            
            item.Alias = _correcter.Correct(item.Alias);
            base.Update(item);
        }
    }
    
    public class DecoratedAliasCorrectingUserSettingsStore : IStore<UserSettings>
    {
        private IStore<UserSettings> _store;
        private IAliasAutoCorrected _correcter;
        
        public DecoratedAliasCorrectingUserSettingsStore()
        {            
            _correcter = new AliasAutoCorrected();
            _store = new NonAliasCorrectingUserSettingsStore();
        }

        public UserSettings Get(Guid id)
        {
            return _store.Get(id);
        }

        public void Add(UserSettings item)
        {
            item.Alias = _correcter.Correct(item.Alias);
            _store.Add(item);
        }

        public void Update(UserSettings item)
        {
            item.Alias = _correcter.Correct(item.Alias);
            _store.Update(item);
        }
    }
}