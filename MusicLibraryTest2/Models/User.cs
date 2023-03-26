using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicLibraryTest2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsArchived { get; set; }
    }
}