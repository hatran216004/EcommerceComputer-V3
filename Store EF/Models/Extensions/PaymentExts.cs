using System;
using System.Configuration;
using System.IO;
using System.Net.Http;

namespace Store_EF.Models.Extensions
{
    public static class PaymentExts
    {
        public static string QR(this Payment payment)
        {
            if (payment == null)
                return null;
            else
            {
                string bank = ConfigurationManager.AppSettings["Bank"];
                string account = ConfigurationManager.AppSettings["Account"];
                string fullName = ConfigurationManager.AppSettings["FullName"];
                string requestUrl = $"https://qr.sepay.vn/img?bank={bank}&acc={account}&template=compact&amount={payment.Order.TotalPrice()}&des={payment.Code}";
                using (HttpClient client = new HttpClient())
                {
                    var data = client.GetByteArrayAsync(requestUrl).Result;
                    if (Helpers.IsValidImage(new MemoryStream(data)))
                        return Convert.ToBase64String(data);
                    else
                        return string.Empty;
                }
            }
        }
    }
}