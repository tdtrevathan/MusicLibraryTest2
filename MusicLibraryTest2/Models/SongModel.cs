using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace MusicLibraryTest2.Models 
{
    public class SongModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public string FilePath { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public bool IsArchived { get; set; }
    }
}

