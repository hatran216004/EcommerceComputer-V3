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

        static GmailHandler()
        {
            email = ConfigurationManager.AppSettings["GmailAddress"];
            password = ConfigurationManager.AppSettings["GmailPassword"];
            smtp.Connect("smtp.gmail.com", 465, true);
            smtp.Authenticate(email, password);
        }

        public static bool SendMail(MimeMessage message)
        {
            try
            {
                smtp.Send(message);
                smtp.Disconnect(true);
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