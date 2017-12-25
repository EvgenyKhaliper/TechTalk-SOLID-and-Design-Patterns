namespace SOLID_and_Design_Patterns
{
    public class App
    {
        public App()
        {
            IUserSettingsDomainService basic = new UserSettingsDomainService();
            
            IUserSettingsDomainService srp = new UserSettingsDomainServiceSRP();
            
            IUserSettingsDomainService ocp = new UserSettingsDomainServiceOCP();
            
            IUserSettingsDomainService lsp = new UserSettingsDomainServiceLSP();
            
            IUserSettingsDomainService isp = new UserSettingsDomainServiceISP();


            var userSettingsFlexibleDIStore = new UserSettingsFlexibleDIStore(
                new DecoratedAliasCorrectingUserSettingsStore());
            
            IUserSettingsDomainService dip = new UserSettingsDomainServiceDIP(
                new FlexibleDICache(
                    userSettingsFlexibleDIStore, 
                    new UserSettingsFileCacheProxy()), 
                userSettingsFlexibleDIStore);
        }
    }
}