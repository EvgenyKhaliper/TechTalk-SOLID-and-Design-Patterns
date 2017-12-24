using System;

namespace SOLID_and_Design_Patterns
{
    public static class DatabaseQueryMaker
    {
        public static string CreateSelectUserSettings(Guid userId)
        {
            return "query with @userId as param";
        }

        public static string CreateUpdateUserSettings(Guid userId, string alias, bool sendNewsletter)
        {
            return "update with @userId, @alias, @sendNewsletter as param";
        }

        public static string CreateInsertUserSettings(Guid userId, string @alias, bool sendNewsletter)
        {
            return "insert with @userId, @alias, @sendNewsletter as param";
        }
    }
}