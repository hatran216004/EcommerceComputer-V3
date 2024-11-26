using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace Store_EF.Models
{
    public class SupportEntities : DbContext
    {
        SqlConnection conn;

        public SupportEntities() : base("name=StoreEntities")
        {
            conn = new SqlConnection(Database.Connection.ConnectionString);
        }

        public Nullable<Backup> BackupDB(string name, string desc, string folderPath, bool differential = false)
        {
            string type = "FULL";
            DateTime now = DateTime.Now;
            string fP = Path.Combine(folderPath, $"{now:yyyyMMddTHHmmss}.bak");
            string query = $"BACKUP DATABASE {conn.Database} TO DISK = N'{fP}' WITH NAME = N'{name}', DESCRIPTION = N'{desc}'";
            if (differential)
            {
                query += ", DIFFERENTIAL";
                type = "DIFFERENTIAL";
            }
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                int test = cmd.ExecuteNonQuery();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return new Backup()
                {
                    CreatedAt = now,
                    Name = name,
                    Desc = desc,
                    Path = fP,
                    Type = type,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public Nullable<Backup> BackupLog(string name, string desc, string folderPath)
        {
            DateTime now = DateTime.Now;
            string fP = Path.Combine(folderPath, $"{now:yyyyMMddTHHmmss}.trn");
            string query = $"BACKUP LOG {conn.Database} TO DISK = N'{fP}' WITH NAME = N'{name}', DESCRIPTION = N'{desc}'";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                int test = cmd.ExecuteNonQuery();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return new Backup()
                {
                    CreatedAt = now,
                    Name = name,
                    Desc = desc,
                    Path = fP,
                    Type = "LOG",
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public int CountReviews(int productId = 0)
        {
            SqlCommand cmd = new SqlCommand($"SELECT dbo.CountReviews({productId})", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                int result = (int)cmd.ExecuteScalar();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return 0;
        }

        public int GetProductDiscountPercent(int productId = 0)
        {
            SqlCommand cmd = new SqlCommand($"SELECT dbo.GetProductDiscountPercent({productId})", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                object result = cmd.ExecuteScalar();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return (int)result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return 0;
        }

        public double StarAVG(int productId = 0)
        {
            SqlCommand cmd = new SqlCommand($"SELECT dbo.StarAVG({productId})", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                object result = cmd.ExecuteScalar();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                return double.Parse(result.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return 0;
        }

        public bool ChangeActiveState(int userId)
        {
            SqlCommand cmd = new SqlCommand($"DECLARE @Out BIT\nEXEC @Out = ChangeActiveState {userId}\nSELECT @Out", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
                if (rows > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public IEnumerable<ViewReport> GetViewReport()
        {
            List<ViewReport> result = new List<ViewReport>();
            SqlCommand cmd = new SqlCommand("SELECT * FROM ViewReport", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ViewReport v = new ViewReport()
                    {
                        Month = reader.GetInt32(0),
                        Year = reader.GetInt32(1),
                        TotalIncome = reader.GetInt32(2),
                        TotalSold = reader.GetInt32(3),
                    };
                    result.Add(v);
                }
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return result;
        }
    }
}