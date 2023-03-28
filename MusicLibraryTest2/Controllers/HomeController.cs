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
using System.Drawing;


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

        public ActionResult LoadSong(int songId)
        {
            AddView(songId);

            SongModel songModel = new SongModel()
            {
                Id = songId
            };
            return PartialView("_PlaySong", songModel);
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

        public ActionResult GetMostPopularArtistReport()
        {
            List<UserReportModel> userReports = new List<UserReportModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {

                string command = $"SELECT artist_name,SUM(views) as views,SUM(likes) as likes" +
                    $" FROM album,album_songs,song WHERE album.id = album_songs.albumid" +
                    $" AND album_songs.songId = song.id" +
                    $" AND created_at > now() - interval 1 month" +
                    $" GROUP BY artist_name " +
                    $" ORDER BY views desc, likes desc";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var userReport = new UserReportModel()
                        {
                            ArtistName = reader["artist_name"].ToString(),
                            Likes = Convert.ToInt32(reader["likes"]),
                            Views = Convert.ToInt32(reader["views"])
                        };

                        userReports.Add(userReport);
                    }
                }
            }

            var userReportModels = new UserReportModels() { UserReports = userReports };

            return PartialView("_UserReport", userReportModels);
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

        public int AddView(int songId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"UPDATE song SET views = views + 1 WHERE song.id = {songId}", con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                if (cmd.ExecuteNonQuery() > 0)
                {
                    cmd = new MySqlCommand($"INSERT INTO user_views (userId,songId) values ({profile.Id},{songId})", con);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        cmd = new MySqlCommand($"SELECT views FROM song WHERE song.id = {songId}", con);

                        MySqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            return Convert.ToInt32(reader["veiws"]);
                        }
                    }
                }
            }
            return 0;
        }

        public bool FollowArtist(string artistName)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO user_follows (followerId,followingId) values ({profile.Id},(SELECT user.Id FROM user WHERE user.username = '{artistName}'))", con);
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    return true;
                }

            }
            return false;
        }
        public ActionResult SearchSongs()
        {
            List<SongModel> songList = new List<SongModel>();
            string searchText = Request.QueryString["searchTerm"];
            string searchBy = Request.QueryString["searchBy"];
            string query = "";
            switch (searchBy)
            {
                case "title":
                    query = $"SELECT song.Id, song.title, song.genre, song.likes " +
                            $"FROM song " +
                            $"WHERE song.title LIKE '%{searchText}%' " +
                            $"ORDER BY song.likes DESC";
                    break;
                case "artist":
                    query = $"SELECT song.Id, song.title, song.genre, song.likes " +
                            $"FROM song " +
                            $"LEFT JOIN album_songs ON song.ID = album_songs.songId " +
                            $"LEFT JOIN album ON album.ID = album_songs.albumId " +
                            $"WHERE album.artist_name LIKE '%{searchText}%' " +
                            $"ORDER BY song.likes DESC";
                    break;
                case "genre":
                    query = $"SELECT song.Id, song.title, song.genre, song.likes " +
                            $"FROM song " +
                            $"WHERE song.genre LIKE '%{searchText}%' " +
                            $"ORDER BY song.likes DESC";
                    break;
                default:
                    query = $"SELECT song.Id, song.title, song.genre, song.likes " +
                            $"FROM song " +
                            $"WHERE song.title LIKE '%{searchText}%' " +
                            $"ORDER BY song.likes DESC";
                    break;
            }
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand(query, con);

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
                        LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"])),
                        UserFollowingArtist = Convert.ToBoolean(CheckIfUserFollowingArtist(Convert.ToInt32(reader["id"])))
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
                        LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"])),
                        UserFollowingArtist = Convert.ToBoolean(CheckIfUserFollowingArtist(Convert.ToInt32(reader["id"])))
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

        public ActionResult AddToPlaylistForm(int songId)
        {
            AddToPlaylistModel addToPlaylistModel = new AddToPlaylistModel();

            addToPlaylistModel.UserPlaylistDictionary = GetDictionaryOfUserPlaylists();
            addToPlaylistModel.songId = songId;

            return PartialView("_AddToPlaylistForm", addToPlaylistModel);
        }

        [HttpPost]
        public bool AddToPlaylist(AddToPlaylistModel addToPlaylistModel)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"INSERT INTO playlist_songs (playlistId,songId)" +
                    $" values ('{addToPlaylistModel.playlistId}','{addToPlaylistModel.songId}')";

                MySqlCommand cmd = new MySqlCommand(command, con);

                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                try
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    return false;
                }

            }
            return false;
        }

        Dictionary<string, int> GetDictionaryOfUserPlaylists()
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];
            
            Dictionary<string, int> userPlaylists = new Dictionary<string, int>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT playlist.id, playlist.name" +

                    $" FROM user,user_playlists,playlist" +

                    $" WHERE user.id = user_playlists.userId" +
                    $" AND user_playlists.playlistId = playlist.id" +
                    $" AND user.id = {profile.Id}" +
                    $" AND playlist.isArchived = 0";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    string title = Convert.ToString(reader["name"]);
                    userPlaylists.Add(title, id);
                }
            }
            return userPlaylists;
        }

        [HttpPost]
        public ActionResult CreatePlaylist(CreatePlaylistModel createPlaylistModel)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"INSERT INTO playlist (name) " +
                    $" values ('{createPlaylistModel.Name}')";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }

                command = $"INSERT INTO user_playlists (userId,playlistId) " +
                $"values ('{profile.Id}',(SELECT id From playlist " +
                $"WHERE name = '{createPlaylistModel.Name}' " +
                $"LIMIT 1))";

                cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;

                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return View("HomePage", (ProfileModel)Session["ProfileInfo"]);
        }

        public ActionResult GetPlaylistSongs(int? playlistId)
        {
            List<SongModel> songList = new List<SongModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT song.id, song.title, song.genre" +

                    $" FROM playlist, playlist_songs, song" +

                    $" WHERE playlist.id = playlist_songs.playlistId" +
                    $" AND playlist_songs.songId = song.id" +
                    $" AND playlist.id = {playlistId}";

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
                            LikedByUser = CheckIfLikedByUser(Convert.ToInt32(reader["id"]))
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

            return PartialView("_SongsList", songModels);
        }

        public ActionResult GetNotifications()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];
            List<NotificationModel> list = new List<NotificationModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT notifications.album_title, notifications.artist_name" +

                    $" FROM notifications" +

                    $" WHERE notifications.userId = {profileModel.Id}" +
                    $" AND notifications.isArchived = 0" +
                    $" ORDER BY created_at DESC";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    NotificationModel notificationModel = new NotificationModel()
                    {
                        ArtistName = reader["artist_name"].ToString(),
                        AlubmTitle = reader["album_title"].ToString(),
                    };
                    list.Add(notificationModel);
                }
            }
            NotificationsModel notificationsModel = new NotificationsModel()
            {
                Notifications = list
            };
            return PartialView("_NotificationsList", notificationsModel);
        }

        public ActionResult CreatePlaylistForm(CreatePlaylistModel createPlaylistModel)
        {
            return PartialView("_CreatePlaylistForm", createPlaylistModel);
        }

        public ActionResult GetPlaylists()
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            List<PlaylistModel> playlists = new List<PlaylistModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT playlist.id, playlist.name" +

                    $" FROM playlist, user_playlists, user" +

                    $" WHERE user.id = user_playlists.userId" +
                    $" AND user_playlists.playlistId = playlist.id" +
                    $" AND user.id = {profile.Id}" +
                    $" AND playlist.isArchived = 0";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    var playlistModel = new PlaylistModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                    };

                    playlists.Add(playlistModel);
                }
            }

            PlaylistModels playlistModels = new PlaylistModels()
            {
                Playlists = playlists
            };

            return PartialView("_Playlists", playlistModels);
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
                    $" SELECT followingId FROM user_follows,user_albums,album_songs" +
                    $" WHERE user_follows.followerId = {profile.Id}" +
                    " AND user_follows.followingId = user_albums.userId" +
                    " AND user_albums.albumId = album_songs.albumId" +
                    $" AND album_songs.songId = {songModel.Id}; ", con);

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