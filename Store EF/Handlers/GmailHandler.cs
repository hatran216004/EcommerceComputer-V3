using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Configuration;
using System.Diagnostics;

namespace Store_EF.Handlers
{
    public static class GmailHandler
    {
        static SmtpClient smtp = new SmtpClient();
        static string email;
        static string password;

        public static string Email => email;

        static void Auth()
        {
            smtp.Connect("smtp.gmail.com", 465, true);
            smtp.Authenticate(email, password);
        }

        static GmailHandler()
        {
            email = ConfigurationManager.AppSettings["GmailAddress"];
            password = ConfigurationManager.AppSettings["GmailPassword"];
            try
            {
                Auth();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void SendMail(MimeMessage message)
        {
            try
            {
                if (!smtp.IsConnected)
                    Auth();
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}