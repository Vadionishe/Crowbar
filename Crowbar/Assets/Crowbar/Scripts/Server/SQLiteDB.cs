using Mono.Data.Sqlite;

namespace Crowbar.Server
{
    public static class SQLiteDB
    {
        private static readonly string DBPath = @"C:\Users\Validay\Desktop\Crowbar.db";

        public static void ExecuteRequestWithoutAnswer(string query)
        {
            using (SqliteConnection sqliteConnection = new SqliteConnection("Data Source=" + DBPath))
            {
                using (SqliteCommand sqliteCommand = new SqliteCommand(sqliteConnection))
                {
                    sqliteConnection.Open();
                    sqliteCommand.CommandText = query;
                    sqliteCommand.ExecuteNonQuery();
                }
            }
        }

        public static string ExecuteRequestWithAnswer(string query)
        {
            using (SqliteConnection sqliteConnection = new SqliteConnection("Data Source=" + DBPath))
            {
                using (SqliteCommand sqliteCommand = new SqliteCommand(sqliteConnection))
                {
                    sqliteConnection.Open();
                    sqliteCommand.CommandText = query;
                    var answer = sqliteCommand.ExecuteScalar();

                    if (answer != null)
                        return answer.ToString();
                    else
                        return null;
                }
            }
        }
    }
}
