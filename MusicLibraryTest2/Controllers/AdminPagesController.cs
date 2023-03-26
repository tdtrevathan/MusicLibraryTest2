using MusicLibraryTest2.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Mvc;
using NAudio.Wave;
using System.Web.Profile;
using MusicLibraryTest2.Data;

namespace MusicLibraryTest2.Controllers
{
    public class AdminPagesController : Controller
    {
        string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        MusicDatabase db = new MusicDatabase();

        public ActionResult AdminPage()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];

            return View("AdminPage", profileModel);
        }
        public ActionResult UserList()
        {
            List<User> users = db.GetUserData();
            return View(users);
        }
        public ActionResult SongList()
        {
            List<Song> songs = db.GetSongData();
            return View(songs);
        }
    }
}