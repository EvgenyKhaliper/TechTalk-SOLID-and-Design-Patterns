namespace SOLID_and_Design_Patterns
{
    public static class DatabaseQueryExecutor
    {
        public static T Query<T>(string connectionString, string query) where T : new()
        {
            // run query on database, serialize and return;
            return new T();
        }

        public static void Update<T>(string connectionString, object updateCommand)
        {
            // runs command in database, checks for updated == 1
        }

        public static void Insert<T>(string connectionString, string insertCommand)
        {
            // runs command in database, checks for updated == 1
        }
    }
}