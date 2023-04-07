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
using Microsoft.Ajax.Utilities;

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
            List<User> users = new List<User>();
            string sortBy = "id";
            bool isAscending = true;
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(Request.QueryString["fromDate"]))
            {
                fromDate = DateTime.Parse(Request.QueryString["fromDate"]);
            }

            if (!string.IsNullOrEmpty(Request.QueryString["toDate"]))
            {
                toDate = DateTime.Parse(Request.QueryString["toDate"]);
            }

            if (fromDate != null && toDate != null)
            {
                users = db.GetUsersData(fromDate.Value, toDate.Value);
            }
            else
            {
                users = db.GetUsersData();
            }
            if (!string.IsNullOrEmpty(Request.QueryString["sortBy"]))
            {
                sortBy = Request.QueryString["sortBy"];
            }
            if (!string.IsNullOrEmpty(Request.QueryString["isAscending"]))
            {
                isAscending = bool.Parse(Request.QueryString["isAscending"]);
            }
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
            ViewBag.userCount = users.DistinctBy(u => u.Id).Count();
            ViewBag.sortBy = sortBy;
            ViewBag.isAscending = isAscending;
            ViewBag.fromDate = fromDate;
            ViewBag.toDate = toDate;

            return PartialView(users);
        }

        public ActionResult SongList()
        {
            List<Song> songs = new List<Song>();
            string sortBy = "created";
            bool isAscending = true;
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(Request.QueryString["fromDate"]))
            {
                fromDate = DateTime.Parse(Request.QueryString["fromDate"]);
            }

            if (!string.IsNullOrEmpty(Request.QueryString["toDate"]))
            {
                toDate = DateTime.Parse(Request.QueryString["toDate"]);
            }

            if (fromDate != null && toDate != null)
            {
                songs = db.GetSongData(fromDate.Value, toDate.Value);
            }
            else
            {
                songs = db.GetSongData();
            }
            if (!string.IsNullOrEmpty(Request.QueryString["sortBy"]))
            {
                sortBy = Request.QueryString["sortBy"];
            }
            if (!string.IsNullOrEmpty(Request.QueryString["isAscending"]))
            {
                isAscending = bool.Parse(Request.QueryString["isAscending"]);
            }
            switch (sortBy)
            {
                case "title":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.Title).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.Title).ToList();
                    break;
                case "genre":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.Genre).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.Genre).ToList();
                    break;
                case "view":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.Views).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.Views).ToList();
                    break;
                case "like":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.Likes).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.Likes).ToList();
                    break;
                case "artist":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.Artist).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.Artist).ToList();
                    break;
                case "album":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.AlbumName).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.AlbumName).ToList();
                    break;
                case "created":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.CreatedAt).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.CreatedAt).ToList();
                    break;
                default:
                    songs = songs.OrderBy(s => s.CreatedAt).ToList();
                    break;
            }
            ViewBag.songCount = songs.DistinctBy(s => s.Id).Count();
            ViewBag.sortBy = sortBy;
            ViewBag.isAscending = isAscending;
            ViewBag.fromDate = fromDate;
            ViewBag.toDate = toDate;

            return PartialView(songs);
        }

    }
}