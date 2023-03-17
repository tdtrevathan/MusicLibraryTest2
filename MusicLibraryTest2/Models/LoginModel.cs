using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace MusicLibraryTest2.Models 
{
    public class LoginModel
    {
        private string password;

        public string UserName { get; set; }
        public string Password
        {
            get{return password;} set {password = GetHash(value);}
        }
        public bool ValidCredentials { get; set; }

        private string GetHash(string password)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}

