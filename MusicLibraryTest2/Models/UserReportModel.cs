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
    public class UserReportModel
    {
        public string ArtistName { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }

        public List<SongInfo> Songs { get; set; }
    }
}