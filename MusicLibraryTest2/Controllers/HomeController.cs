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
            return View();
        }

        [HttpPost]
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
                            LoginModel loginModel = new LoginModel()
                            {
                                UserName = signUpModel.UserName,
                                Password = signUpModel.Password,
                            };

                            return SubmitCredentials(loginModel);
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
        public ActionResult AddMusic()
        {
            return PartialView("_AddMusic");
        }

        public ActionResult CreateSongForm(CreateSongModel createSongModel)
        {
            return PartialView("_CreateSongForm",createSongModel);
        }

        public ActionResult CreateAlbumForm(CreateAlbumModel createAlbumModel)
        {
            return PartialView("_CreateAlbumForm", createAlbumModel);
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
                    
                }
            }
            return View("HomePage", (ProfileModel)Session["ProfileInfo"]);
        }

        [HttpPost]
        public ActionResult CreateAlbum(CreateAlbumModel createAlbumModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"INSERT INTO album (title,description,genre,artist_name,releaseDate) " +
                    $"values ('{createAlbumModel.Title}','{createAlbumModel.Description}','{createAlbumModel.Genre}','{createAlbumModel.ArtistName}','{createAlbumModel.ReleaseDate}')";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return View("HomePage", (ProfileModel)Session["ProfileInfo"]);
        }



        public ActionResult GetUserAlbums(int? userId)
        {
            List<AlbumModel> albums = new List<AlbumModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT album.id, album.title, album.genre, album.description, " +
                                          $"album.releaseDate, album.artist_name" +

                    $" FROM user, user_albums, album" +

                    $" WHERE user.id = user_albums.userId" +
                    $" AND user_albums.albumId = album.id" +
                    $" AND user.id = {userId}" +
                    $" AND album.isArchived = 0";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                //while (reader.HasRows)
                //{
                    while (reader.Read())
                    {
                        var albumModel = new AlbumModel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Title = reader["title"].ToString(),
                            ArtistName = reader["artist_name"].ToString(),
                            Genre = reader["genre"].ToString(),
                            Description = reader["description"].ToString(),
                            ReleaseDate = Convert.ToDateTime(reader["releaseDate"])
                        };

                        albums.Add(albumModel);
                    }
                    //reader.NextResult();
                //}
            }

            AlbumModels albumModels = new AlbumModels()
            {
                Albums = albums
            };

            return PartialView("_Albums", albumModels);
        }

        public ActionResult GetAlbumSongs(int? albumId)
        {
            List<SongModel> songList = new List<SongModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT song.id, song.title, song.genre, song.songFile " +

                    $"FROM album, album_songs, song " +

                    $"WHERE album.id = album_songs.albumId" +
                    $" AND album_songs.songId = song.id" +
                    $" AND album.id = {albumId}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var songModel = new SongModel
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Title = reader["title"].ToString(),
                            Genre = reader["genre"].ToString(),
                        };

                        songList.Add(songModel);
                    }
                    reader.NextResult();
                }
            }

            SongModels songModels = new SongModels()
            {
                SongList = songList
            };

            return PartialView("_PlaySong", songModels);
        }

        public ActionResult PlayAudio(int songId)
        {

            var songData = GetByteArray(songId);

            if (songData == null)
            {
                return HttpNotFound();
            }
            else
            {
                return File(songData, "audio/mpeg");
            }
        }

        byte[] GetByteArray(int songId)
        {
 
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT song.songFile From song where song.id = {songId}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        return (byte[])reader["songFile"];
                    }
                }
            }

            return null;
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