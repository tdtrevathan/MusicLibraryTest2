using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace MusicLibraryTest2.Models 
{
    public class ArtistModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int AlbumId { get; set; }
        public bool IsArchived { get; set; }
    }
}

