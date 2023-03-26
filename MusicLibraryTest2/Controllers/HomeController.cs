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
using System.Web.Security;


namespace MusicLibraryTest2.Controllers
{
    public class HomeController : Controller
    {
        string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HomePage()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];
            return View("HomePage", profileModel);
        }

        public ActionResult BecomeArtist()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO user_roles (userId,roleId)" +
                    $" values ('{profileModel.Id}',(SELECT id FROM role WHERE type = 'artist'))", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();


                if (cmd.ExecuteNonQuery() > 0)
                {
                    profileModel.Roles.Add("artist");
                    Session["ProfileInfo"] = profileModel;
                }

            }

            return View("HomePage", profileModel);
        }

        public ActionResult ShowNavbar()
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];
            return PartialView("_NavBar", profile);
        }

        public ActionResult SubmitCredentials(LoginModel loginModel)
        {
            ProfileModel profileModel = new ProfileModel();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * FROM user WHERE user.username = '{loginModel.UserName}' and user.password = '{loginModel.Password}' LIMIT 1", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["Id"]);
                        List<string> roles = GetUserRoles(userId);

                        profileModel = new ProfileModel()
                        {
                            LogedIn = true,
                            Name = reader["username"].ToString(),
                            Id = userId,
                            Roles = roles
                        };
                    }
                    Session["ProfileInfo"] = profileModel;

                    return View("HomePage", profileModel);

                }
                else
                {
                    loginModel.ValidCredentials = false;
                    return View("Index", loginModel);
                }
            }
        }

        public ActionResult SignUp(SignUpModel signUpModel)
        {
            bool userNameIsValid = UsernameIsValid(signUpModel);
            bool passwordIsValid = PasswordIsValid(signUpModel);

            if (userNameIsValid && passwordIsValid)
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    MySqlCommand cmd = new MySqlCommand($"INSERT INTO user (username,password)" +
                        $" values ('{signUpModel.UserName}',SHA1('{signUpModel.Password}'))", con);

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

                            if (AssignUserRole(signUpModel))
                            {
                                LoginModel loginModel = new LoginModel()
                                {
                                    UserName = signUpModel.UserName,
                                    Password = signUpModel.Password,
                                };

                                return SubmitCredentials(loginModel);
                            }
                            else
                            {
                                var errorMessage = "An error occured";
                                signUpModel.ErrorMessages.Add(errorMessage);

                                signUpModel.ValidCredentials = false;
                                return View("SignUpForm", signUpModel);
                            }
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


        private bool AssignUserRole(SignUpModel signUpModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"INSERT INTO user_roles (userId,roleId)" +
                    $" values ((SELECT user.Id FROM user where username = '{signUpModel.UserName}')," +
                    $"(SELECT Id FROM role WHERE role.type = 'user'))";


                MySqlCommand cmd = new MySqlCommand(command, con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                try
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return true;
                    }
                    else
                    {

                        return false;
                    }
                }
                catch (MySqlConnector.MySqlException)
                {
                    return false;
                }
            }
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

        List<string> GetUserRoles(int id)
        {
            List<string> roles = new List<string>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT role.type FROM user,user_roles,role WHERE user.id = user_roles.userId AND user_roles.roleId = role.id AND user.id = {id}", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        roles.Add(reader["type"].ToString());
                    }
                }
            }

            return roles;
        }

        public int AddLike(int songId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"UPDATE song SET likes = likes + 1 WHERE song.id = {songId}", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                if (cmd.ExecuteNonQuery() > 0)
                {
                    cmd = new MySqlCommand($"INSERT INTO user_likes (userId,songId) values ({profile.Id},{songId})", con);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        cmd = new MySqlCommand($"SELECT likes FROM song WHERE song.id = {songId}", con);

                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            return Convert.ToInt32(reader["likes"]);
                        }
                    }
                }
            }
            return 0;
        }

        public ActionResult BrowseAllSongs()
        {
            List<SongModel> songList = new List<SongModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT song.Id, song.title, song.genre, song.likes " +
                    $" FROM song ORDER BY likes DESC", con);

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
                        LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"]))
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

        public ActionResult BrowseLikedSongs()
        {
            List<SongModel> songList = new List<SongModel>();
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT song.Id, song.title, song.genre, song.likes " +
                    $" FROM song, user_likes " +
                    $" WHERE song.Id = user_likes.songId " +
                    $" AND user_likes.userId = {profile.Id}", con);

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
                        LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"]))
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

        public ActionResult SignUpForm()
        {
            ViewBag.Message = "Create a profile";

            return View();
        }

        public bool CheckIfLikedByUser(int songId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand(
                    $"SELECT * FROM user_likes WHERE" +
                    $" userId = {profile.Id} AND songId = {songId}",con);

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
                while (reader.Read())
                {
                    songModel.Artist = reader["artist_name"].ToString();
                    songModel.AlbumName = reader["title"].ToString();
                }
            }
        }
    }
}