using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicLibraryTest2.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool isAdmin { get; set; }
        public bool isArtist { get; set; }
    }
}