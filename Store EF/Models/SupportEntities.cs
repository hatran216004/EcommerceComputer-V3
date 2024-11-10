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

        public virtual int CountReviews(int productId = 0)
        {
            SqlCommand cmd = new SqlCommand($"DECLARE @Out Int\r\nEXEC @Out = CountReviews {productId}\r\nSELECT @Out", conn);
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

        public virtual int GetProductDiscountPercent(int productId = 0)
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

        public virtual double StarAVG(int productId = 0)
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
    }
}