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
    public class SignUpModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool ValidCredentials { get; set; }

        public List<string> ErrorMessages = new List<string>();
    }
}

