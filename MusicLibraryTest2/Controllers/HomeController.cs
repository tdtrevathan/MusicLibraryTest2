using MusicLibraryTest2.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;
using MySqlConnector;

namespace MusicLibraryTest2.Controllers
{
    public class HomeController : Controller
    {
        string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public ActionResult GetArtist(int? artistId)
        {
            
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM artist WHERE artist.Id = {artistId}", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                var artistModel = new ArtistModel();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    artistModel.Id = Convert.ToInt32(reader["Id"]);
                    artistModel.AlbumId = Convert.ToInt32(reader["albumId"]);
                    artistModel.Name = reader["name"].ToString();
                    artistModel.IsArchived = Convert.ToBoolean(reader["isArchived"]);
                }
                return View(artistModel);
            }
        }
        public ActionResult Index()
        {

            //List<SongModel> songs = new List<SongModel>();
            //
            //using (MySqlConnection con = new MySqlConnection(connection))
            //{
            //    MySqlCommand cmd = new MySqlCommand("SELECT * FROM song", con);
            //    cmd.CommandType = System.Data.CommandType.Text;
            //    con.Open();
            //
            //    MySqlDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        var songModel = new SongModel();
            //        songModel.Id = Convert.ToInt32(reader["Id"]);
            //        songModel.Views = Convert.ToInt32(reader["views"]);
            //        songModel.Likes = Convert.ToInt32(reader["likes"]);
            //        songModel.Title = reader["Title"].ToString();
            //        songModel.Duration = reader["duration"].ToString();
            //        songModel.IsArchived = Convert.ToBoolean(reader["isArchived"]);
            //        songs.Add(songModel);
            //    }
            //}

            return View();
        }

        [HttpPost]
        public ActionResult SubmitCredentials(LoginModel loginModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM user WHERE user.username = '{loginModel.UserName}' and user.password = '{loginModel.Password}' LIMIT 1", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    Console.WriteLine("valid");
                }
                else
                {
                    Console.WriteLine("Invalid");
                }

            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your about page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}