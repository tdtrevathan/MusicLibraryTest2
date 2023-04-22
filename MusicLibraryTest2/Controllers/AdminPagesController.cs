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
using NAudio.CoreAudioApi;

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
            string sortBy;
            bool isAscending;
            DateTime? fromDate;
            DateTime? toDate;
            string role;
            bool noArchived;

            if (!string.IsNullOrEmpty(Request.QueryString["fromDate"]))
            {
                fromDate = DateTime.Parse(Request.QueryString["fromDate"]);
            }
            else fromDate = null;

            if (!string.IsNullOrEmpty(Request.QueryString["toDate"]))
            {
                toDate = DateTime.Parse(Request.QueryString["toDate"]);
            }
            else toDate = null;

            if (fromDate != null && toDate != null)
            {
                users = db.GetUsersData(fromDate.Value, toDate.Value);
            }
            else
            {
                users = db.GetUsersData();
            }

            if (!string.IsNullOrEmpty(Request.QueryString["role"]))
            {
                role = Request.QueryString["role"];
                users = users.Where(u => u.Role == role).ToList();
            }
            else role = "";
            
            if (!string.IsNullOrEmpty(Request.QueryString["noArchived"]))
            {
                noArchived = bool.Parse(Request.QueryString["noArchived"]);
                if (noArchived)
                users = users.Where(u => u.IsArchived != noArchived).ToList();
            }
            else noArchived = false;

            if (!string.IsNullOrEmpty(Request.QueryString["sortBy"]))
            {
                sortBy = Request.QueryString["sortBy"];
            }
            else sortBy = "id";

            if (!string.IsNullOrEmpty(Request.QueryString["isAscending"]))
            {
                isAscending = bool.Parse(Request.QueryString["isAscending"]);
            }
            else isAscending = true;

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
                case "login":
                    if (isAscending)
                        users = users.OrderBy(u => u.LastLoginAt).ToList();
                    else
                        users = users.OrderByDescending(u => u.LastLoginAt).ToList();
                    break;
                case "archived":
                    if (isAscending)
                        users = users.OrderBy(u => u.IsArchived).ToList();
                    else
                        users = users.OrderByDescending(u => u.IsArchived).ToList();
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
            ViewBag.role = role;
            ViewBag.noArchived = noArchived;

            return PartialView(users);
        }

        public ActionResult SongList()
        {
            List<Song> songs = new List<Song>();
            string sortBy;
            bool isAscending;
            DateTime? fromDate;
            DateTime? toDate;
            string genre;
            bool noArchived;

            if (!string.IsNullOrEmpty(Request.QueryString["fromDate"]))
            {
                fromDate = DateTime.Parse(Request.QueryString["fromDate"]);
            }
            else fromDate = null;

            if (!string.IsNullOrEmpty(Request.QueryString["toDate"]))
            {
                toDate = DateTime.Parse(Request.QueryString["toDate"]);
            }
            else toDate = null;

            if (fromDate != null && toDate != null)
            {
                songs = db.GetSongData(fromDate.Value, toDate.Value);
            }
            else
            {
                songs = db.GetSongData();
            }

            if (!string.IsNullOrEmpty(Request.QueryString["genre"]))
            {
                genre = Request.QueryString["genre"];
                songs = songs.Where(s => s.Genre.ToLower() == genre).ToList();
            }
            else genre = "";

            if (!string.IsNullOrEmpty(Request.QueryString["noArchived"]))
            {
                noArchived = bool.Parse(Request.QueryString["noArchived"]);
                if (noArchived)
                    songs = songs.Where(s => s.IsArchived != noArchived).ToList();
            }
            else noArchived = false;

            if (!string.IsNullOrEmpty(Request.QueryString["sortBy"]))
            {
                sortBy = Request.QueryString["sortBy"];
            }
            else sortBy = "created";

            if (!string.IsNullOrEmpty(Request.QueryString["isAscending"]))
            {
                isAscending = bool.Parse(Request.QueryString["isAscending"]);
            }
            else isAscending = true;

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
                case "archived":
                    if (isAscending)
                        songs = songs.OrderBy(s => s.IsArchived).ToList();
                    else
                        songs = songs.OrderByDescending(s => s.IsArchived).ToList();
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
            ViewBag.genre = genre;
            ViewBag.noArchived = noArchived;

            return PartialView(songs);
        }

        public ActionResult BrowseAllSongs()
        {
            List<SongModel> songList = new List<SongModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT song.Id, song.title, song.genre, song.likes, song.views" +
                    $" FROM song WHERE song.isArchived = 0 ORDER BY views DESC LIMIT 50", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var songModel = new SongModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Title = reader["title"].ToString(),
                        Genre = reader["genre"].ToString(),
                        Likes = Convert.ToInt32(reader["likes"]),
                        Views = Convert.ToInt32(reader["views"]),
                        LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"])),
                        UserFollowingArtist = Convert.ToBoolean(CheckIfUserFollowingArtist(Convert.ToInt32(reader["id"]))),
                        UserIsAdmin = true
                    };

                    GetArtistInfo(songModel);

                    songList.Add(songModel);
                }
            }

            SongModels songModels = new SongModels()
            {
                SongList = songList
            };

            return PartialView("_SongsList", songModels);
        }

        public bool CheckIfLikedByUser(int songId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand(
                    $"SELECT * FROM user_likes WHERE" +
                    $" userId = {profile.Id} AND songId = {songId}", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CheckIfUserFollowingArtist(int songId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            SongModel songModel = new SongModel()
            {
                Id = songId
            };

            GetArtistInfo(songModel);

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand(
                    $" SELECT followingId FROM user_follows,user_songs" +
                    $" WHERE user_follows.followerId = {profile.Id}" +
                    " AND user_follows.followingId = user_songs.userId" +
                    $" AND user_songs.songId = {songModel.Id}; ", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void GetArtistInfo(SongModel songModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand(
                    $"SELECT album.title, album.artist_name FROM album,album_songs,song WHERE" +
                    $" album.Id = album_songs.albumId " +
                    $" AND album_songs.songId = {songModel.Id}", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        songModel.Artist = reader["artist_name"].ToString();
                        songModel.AlbumName = reader["title"].ToString();
                    }
                }
                else
                {
                    con.Close();
                    con.Open();
                    string command = $"SELECT username FROM user,user_songs,song" +
                    $" WHERE song.Id = user_songs.songId " +
                    $" AND user.Id = user_songs.userId " +
                    $" AND song.Id = {songModel.Id}";

                    cmd = new MySqlCommand(command, con);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        songModel.Artist = reader["username"].ToString();
                    }
                }
            }
        }
    }
}