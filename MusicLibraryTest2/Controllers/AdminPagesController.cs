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
            string sortBy = Request.QueryString["sortBy"];
            bool isAscending = bool.Parse(Request.QueryString["isAscending"]);
            switch (sortBy)
            {
                case "id":
                    if (isAscending)
                        users = users.OrderBy(u => u.Id).ToList();
                    else
                        users = users.OrderByDescending(u => u.Id).ToList();
                    break;
                case "name":
                    if (isAscending)
                        users = users.OrderBy(u => u.Username).ToList();
                    else
                        users = users.OrderByDescending(u => u.Username).ToList();
                    break;
                case "created":
                    if (isAscending)
                        users = users.OrderBy(u => u.CreatedAt).ToList();
                    else
                        users = users.OrderByDescending(u => u.CreatedAt).ToList();
                    break;
                default:
                    users = users.OrderBy(u => u.Id).ToList();
                    break;
            }

            ViewBag.sortBy = sortBy;
            ViewBag.isAscending = isAscending;

            return View(users);
        }
        public ActionResult SongList()
        {
            List<Song> songs = db.GetSongData();
            return View(songs);
        }

    }
}