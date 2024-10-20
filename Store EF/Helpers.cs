using Store_EF.Models;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Linq;

namespace Store_EF
{
    public static class Helpers
    {
        public static string FormattedPrice(int price)
        {
            NumberFormatInfo nfi = new CultureInfo("vi-VN", false).NumberFormat;
            return price.ToString("C0", nfi).Replace(",", ".");
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (phone.Length == 10 || phone.Length == 11)
            {
                if (phone[0] == '0')
                    return true;
                else
                    return false;
            }
            else return false;
        }

        public static bool IsValidImage(HttpPostedFileBase postedFile)
        {
            if (postedFile == null)
                return false;
            Stream stream = postedFile.InputStream;
            try
            {
                Image.FromStream(stream, false, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUserAdmin(int userId, StoreEntities store)
        {
            User_ user = store.Users.FirstOrDefault(x => x.UserId == userId);
            if (user == null)
                return false;
            if (user.RoleName != "Admin")
                return false;
            return true;
        }

        public static bool IsValidImage(Stream stream)
        {
            try
            {
                Image.FromStream(stream, false, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, byte[] buffer)
        {
            byte[] data = hashAlgorithm.ComputeHash(buffer);
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, Stream stream)
        {
            byte[] data = hashAlgorithm.ComputeHash(GetBytes(stream));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }

        public static byte[] GetBytes(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream))
            {
                var memoryStream = new MemoryStream();
                bufferedStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}