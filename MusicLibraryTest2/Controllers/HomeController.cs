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
using System.Drawing;
using System.Reflection.Metadata;
using System.IO;
using System.Reflection;
using Cassandra;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

//Test 123

namespace MusicLibraryTest2.Controllers
{
    //i made a change
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
                    return View("HomePage");
                }
                else
                {
                    loginModel.ValidCredentials = false;
                    return View("Index",loginModel);
                }
            }
        }

        public ActionResult CreateSongForm(CreateSongModel createSongModel)
        {
            return View(createSongModel);
        }

        [HttpPost]
        public ActionResult CreateSong(CreateSongModel createSongModel)
        {

            MemoryStream target = new MemoryStream();
            createSongModel.songFile.InputStream.CopyTo(target);
            byte[] songData = target.ToArray();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO song (title,duration,genre,songFile) values ('{createSongModel.Title}',{createSongModel.Duration},'{createSongModel.Genre}',@data)", con);
                cmd.Parameters.Add("@data", MySqlDbType.Blob).Value = songData;
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0) {
                    var test2 = 10;
                }
            }
                return View("HomePage");
        }

        [HttpPost]
        public ActionResult SignUp(SignUpModel signUpModel)
        {
            bool userNameIsValid = UsernameIsValid(signUpModel);
            bool passwordIsValid = PasswordIsValid(signUpModel);

            if (userNameIsValid && passwordIsValid)
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO user (username,password) values ('{signUpModel.UserName}',SHA1('{signUpModel.Password}'))", con);
                    cmd.CommandType = System.Data.CommandType.Text;
                    con.Open();

                    try
                    {
                        if (cmd.ExecuteNonQuery() < 0)
                        {
                            var errorMessage = "An error occured";
                            signUpModel.ErrorMessages.Add(errorMessage);
                            signUpModel.ValidCredentials = false;
                            return View("SignUpForm", signUpModel);
                        }
                        else
                        {
                            return View("HomePage");
                        }
                    }
                    catch (MySqlConnector.MySqlException)
                    {
                        var errorMessage = "An error occured";
                        signUpModel.ErrorMessages.Add(errorMessage);

                        signUpModel.ValidCredentials = false;
                        return View("SignUpForm", signUpModel);
                    }
                }
            }
            else
            {
                signUpModel.ValidCredentials = false;
                return View("SignUpForm", signUpModel);
            }
        }

        [HttpGet]
        public ActionResult GetSong()
        {
            SongModel songModel = new SongModel();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT title,genre,songFile FROM song WHERE title like '%my%' LIMIT 1", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var test = reader.GetString(0);

                        songModel = new SongModel
                        {
                            Title = reader.GetString(0),
                            Genre = reader.GetString(1),
                        };

                        var bytesObject = reader.GetValue(2);

                        var songBytesArray = (byte[])bytesObject;

                        songModel.songFile = songBytesArray;
                    }
                }
            }

            return View("PlaySong", songModel);
        }

        public ActionResult PlayAudio(byte[] song)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(song, 0, song.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return File(memStream, "audio/mp3");
        }

        private bool UsernameIsValid(SignUpModel signUpModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM user WHERE user.username = '{signUpModel.UserName}' ", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    var errorMessage = "Username is already taken";
                    signUpModel.ErrorMessages.Add(errorMessage);
                }
            }

            if (signUpModel.ErrorMessages.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool PasswordIsValid(SignUpModel signUpModel)
        {
            if (signUpModel.Password.All(char.IsLetter))
            {
                var errorMessage = "Password must contain either a number or special character";
                signUpModel.ErrorMessages.Add(errorMessage);
            }

            if (signUpModel.Password.All(char.IsLower))
            {
                var errorMessage = "Password must contain an uppercase character";
                signUpModel.ErrorMessages.Add(errorMessage);
            }

            if (signUpModel.Password.Any(char.IsWhiteSpace))
            {
                var errorMessage = "Password cannot contain whitespace";
                signUpModel.ErrorMessages.Add(errorMessage);
            }

            if (signUpModel.Password.Length < 5 || signUpModel.Password.Length > 10)
            {
                var errorMessage = "Password must be between 5 and 10 characters long";
                signUpModel.ErrorMessages.Add(errorMessage);
            }

            if (signUpModel.ErrorMessages.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult SignUpForm()
        {
            ViewBag.Message = "Create a profile";

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