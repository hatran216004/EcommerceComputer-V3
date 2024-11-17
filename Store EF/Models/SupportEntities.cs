using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Store_EF.Models
{
    public class SupportEntities : DbContext
    {
        SqlConnection conn;

        public SupportEntities() : base("name=StoreEntities")
        {
            conn = new SqlConnection(Database.Connection.ConnectionString);
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
            } catch (Exception ex)
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
                return (double)result;
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
    }
}