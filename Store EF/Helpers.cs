using Store_EF.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;

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

        public static string NumberToText(double inputNumber, bool suffix = true)
        {
            string[] unitNumbers = new string[] { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] placeValues = new string[] { "", "nghìn", "triệu", "tỷ" };
            bool isNegative = false;

            // -12345678.3445435 => "-12345678"
            string sNumber = inputNumber.ToString("#");
            double number = Convert.ToDouble(sNumber);
            if (number < 0)
            {
                number = -number;
                sNumber = number.ToString();
                isNegative = true;
            }


            int ones, tens, hundreds;

            int positionDigit = sNumber.Length;   // last -> first

            string result = " ";


            if (positionDigit == 0)
                result = unitNumbers[0] + result;
            else
            {
                // 0:       ###
                // 1: nghìn ###,###
                // 2: triệu ###,###,###
                // 3: tỷ    ###,###,###,###
                int placeValue = 0;

                while (positionDigit > 0)
                {
                    // Check last 3 digits remain ### (hundreds tens ones)
                    tens = hundreds = -1;
                    ones = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                    positionDigit--;
                    if (positionDigit > 0)
                    {
                        tens = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                        positionDigit--;
                        if (positionDigit > 0)
                        {
                            hundreds = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                            positionDigit--;
                        }
                    }

                    if ((ones > 0) || (tens > 0) || (hundreds > 0) || (placeValue == 3))
                        result = placeValues[placeValue] + result;

                    placeValue++;
                    if (placeValue > 3) placeValue = 1;

                    if ((ones == 1) && (tens > 1))
                        result = "một " + result;
                    else
                    {
                        if ((ones == 5) && (tens > 0))
                            result = "lăm " + result;
                        else if (ones > 0)
                            result = unitNumbers[ones] + " " + result;
                    }
                    if (tens < 0)
                        break;
                    else
                    {
                        if ((tens == 0) && (ones > 0)) result = "lẻ " + result;
                        if (tens == 1) result = "mười " + result;
                        if (tens > 1) result = unitNumbers[tens] + " mươi " + result;
                    }
                    if (hundreds < 0) break;
                    else
                    {
                        if ((hundreds > 0) || (tens > 0) || (ones > 0))
                            result = unitNumbers[hundreds] + " trăm " + result;
                    }
                    result = " " + result;
                }
            }
            result = result.Trim();
            if (isNegative) result = "Âm " + result;
            return result + (suffix ? " đồng chẵn" : "");
        }
    }
}