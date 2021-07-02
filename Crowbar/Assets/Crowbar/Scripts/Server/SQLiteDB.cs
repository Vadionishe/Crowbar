using Mono.Data.Sqlite;
using System.Collections.Generic;

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

        public static List<string> GetRecords(string query)
        {
            List<string> records = new List<string>();

            using (SqliteConnection sqliteConnection = new SqliteConnection("Data Source=" + DBPath))
            {
                using (SqliteCommand sqliteCommand = new SqliteCommand(sqliteConnection))
                {
                    sqliteConnection.Open();
                    sqliteCommand.CommandText = query;

                    using (SqliteDataReader reader = sqliteCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string name = reader.GetValue(0).ToString();
                                string deep = reader.GetValue(1).ToString();

                                records.Add($"{name} - {deep} m");
                            }
                        }
                    }

                    return records;
                }
            }
        }
    }
}
