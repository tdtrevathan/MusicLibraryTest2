using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace MusicLibraryTest2.Models
{
    public class ArtistReportModel
    {
        public string SongName { get; set; }
        public string AlbumName { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
    }
}