﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace MusicLibraryTest2.Models 
{
    public class NotificationModel
    {
        public string ArtistName { get; set; }
        public string AlubmTitle { get; set; }
        public int AlbumId { get; set; }
    }
}

