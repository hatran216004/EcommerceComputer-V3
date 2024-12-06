using Store_DbRecovery.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Store_DbRecovery
{
    public static class Helpers
    {
        public static List<List<object>> ConvertToTableData(this IEnumerable<Backup> backups)
        {
            List<List<object>> result = new List<List<object>>();
            for (int i = 0; i < backups.Count(); i++)
            {
                Backup b = backups.ElementAt(i);
                List<object> curr = new List<object>() {
                    $"{i + 1}",
                    $"{b.Name}",
                    $"{b.Type}",
                    $"{b.CreatedAt}",
                    $"{b.Desc}"
                };
                result.Add(curr);
            }
            return result;
        }

        public static bool IsValidBackup(this IEnumerable<Backup> backups, SqlConnection sqlConn)
        {
            bool isValid = true;
            SqlCommand cmd = sqlConn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            foreach (Backup backup in backups)
            {
                cmd.CommandText = $"RESTORE VERIFYONLY FROM DISK = N'{backup.Path}'";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }

        static string GetDatabaseName(this IEnumerable<Backup> backups, SqlConnection sqlConn)
        {
            Backup b = backups.First(x => x.Type.Equals("Full", System.StringComparison.InvariantCultureIgnoreCase));
            SqlCommand cmd = sqlConn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = $"RESTORE HEADERONLY FROM DISK = N'{b.Path}'";
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string result = reader["DatabaseName"].ToString();
                    reader.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return string.Empty;
        }

        public static bool Restore(this IEnumerable<Backup> backups, SqlConnection sqlConn)
        {
            string name = GetDatabaseName(backups, sqlConn);
            StringBuilder builder = new StringBuilder();
            foreach (Backup b in backups)
            {
                switch (b.Type.ToLower())
                {
                    case "full":
                        {
                            builder.AppendLine($"RESTORE DATABASE {name} FROM DISK = N'{b.Path}' WITH NORECOVERY;");
                            break;
                        }
                    case "differential":
                        {
                            builder.AppendLine($"RESTORE DATABASE {name} FROM DISK = N'{b.Path}' WITH NORECOVERY;");
                            break;
                        }
                    case "log":
                        {
                            builder.AppendLine($"RESTORE LOG {name} FROM DISK = '{b.Path}' WITH NORECOVERY;");
                            break;
                        }
                }
            }
            builder.AppendLine($"RESTORE DATABASE {name} WITH RECOVERY;");
            SqlCommand cmd = sqlConn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = builder.ToString();
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }
    }
}
