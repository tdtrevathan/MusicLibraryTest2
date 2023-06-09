﻿using Azure.Storage.Blobs.Models;
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
    public class CreateSongModel
    {
        public int ProfileId { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public System.Web.HttpPostedFileWrapper songFile { get; set; }
        public Dictionary<string,int> UserAlbumDictionary { get; set; }
        public int albumId { get; set; }
    }
}

