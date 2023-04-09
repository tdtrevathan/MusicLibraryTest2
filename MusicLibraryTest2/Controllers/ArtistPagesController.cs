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


namespace MusicLibraryTest2.Controllers
{
    public class ArtistPagesController : Controller
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

        public ActionResult ArtistPage()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];

            return View("ArtistPage", profileModel);
        }

        public ActionResult AddMusic()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];

            return PartialView("_AddMusic", profileModel);
        }

        public ActionResult CreateSongForm()
        {
            CreateSongModel createSongModel = new CreateSongModel();

            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];

            createSongModel.ProfileId = profileModel.Id;

            createSongModel.UserAlbumDictionary = GetDictionaryOfUserAlbums(profileModel.Id);

            return PartialView("_CreateSongForm", createSongModel);
        }

        public ActionResult CreateAlbumForm(CreateAlbumModel createAlbumModel)
        {
            return PartialView("_CreateAlbumForm", createAlbumModel);
        }

        public ActionResult EditAlbumForm(AlbumModel albumModel)
        {
            EditAlbumModel editAlbumModel = new EditAlbumModel()
            {
                Id = albumModel.Id,
                Title = albumModel.Title,
                Genre = albumModel.Genre,
                Description = albumModel.Description,
            };

            return PartialView("_EditAlbumForm", editAlbumModel);
        }

        public ActionResult EditSongForm(SongModel songModel)
        {
            EditSongModel editSongModel = new EditSongModel()
            {
                Id = songModel.Id,
                Title = songModel.Title,
                Genre = songModel.Genre,
            };

            return PartialView("_EditSongForm", editSongModel);
        }

        [HttpPost]
        public ActionResult EditAlbum(EditAlbumModel editAlbumModel)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"UPDATE album SET title = '{editAlbumModel.Title}', description = '{editAlbumModel.Description}', genre = '{editAlbumModel.Genre}'"+
                    $"WHERE Id = {editAlbumModel.Id}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return View("ArtistPage", (ProfileModel)Session["ProfileInfo"]);
        }

        [HttpPost]
        public ActionResult EditSong(EditSongModel editSongModel)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"UPDATE song SET title = '{editSongModel.Title}', genre = '{editSongModel.Genre}'" +
                    $"WHERE Id = {editSongModel.Id}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return View("ArtistPage", (ProfileModel)Session["ProfileInfo"]);
        }

        [HttpPost]
        public ActionResult CreateSong(CreateSongModel createSongModel)
        {

            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            MemoryStream target = new MemoryStream();
            createSongModel.songFile.InputStream.CopyTo(target);
            byte[] songData = target.ToArray();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"INSERT INTO song (title,duration,genre,songFile) values ('{createSongModel.Title}','{createSongModel.Genre}',@data)", con);
                cmd.Parameters.Add("@data", MySqlDbType.Blob).Value = songData;
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            if (createSongModel.albumId != 0)
            {
                //create user_album relationship
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    string command = $"INSERT INTO album_songs (albumId,songId) values" +
                        $" ({createSongModel.albumId}, (SELECT song.id from song WHERE song.title = '{createSongModel.Title}' AND song.genre = '{createSongModel.Genre}'))";

                    MySqlCommand cmd = new MySqlCommand(command, con);
                    cmd.Parameters.Add("@data", MySqlDbType.Blob).Value = songData;
                    cmd.CommandType = System.Data.CommandType.Text;
                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {

                    }
                }
            }
            else
            {
                //create user_song relationship
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    string command = $"INSERT INTO user_songs (userId,songId) values" +
                        $" ({profile.Id}, (SELECT * FROM song WHERE title = '{createSongModel.Title}' AND genre = {createSongModel.Genre}))";

                    MySqlCommand cmd = new MySqlCommand(command, con);
                    cmd.Parameters.Add("@data", MySqlDbType.Blob).Value = songData;
                    cmd.CommandType = System.Data.CommandType.Text;
                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {

                    }
                }

            }

            return View("ArtistPage", (ProfileModel)Session["ProfileInfo"]);
        }

        public ActionResult GetArtistReport()
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            List<ArtistReportModel> artistReports = new List<ArtistReportModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {

                string command = $"SELECT song.title as song_title, album.title as album_title , SUM(likes) as TotalLikes, COUNT(*) as MonthlyViews" +
                " FROM album,album_songs,song,user_views,user_albums" +
                " WHERE album.id = album_songs.albumid" +
                " AND album_songs.songId = song.id" +
                " AND user_views.time_viewed > now() - interval 1 month" +
                " AND user_views.songId = song.id" +
                " AND album.id = user_albums.albumId" +
                " AND song.isArchived = 0" +
                $" AND user_albums.userid = {profile.Id}" +
                " GROUP BY song.title, album.title" +
                " ORDER BY TotalLikes desc";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var artistReport = new ArtistReportModel()
                        {
                            SongName = reader["song_title"].ToString(),
                            AlbumName = reader["album_title"].ToString(),
                            Likes = Convert.ToInt32(reader["TotalLikes"]),
                            Views = Convert.ToInt32(reader["MonthlyViews"])
                        };

                        artistReports.Add(artistReport);
                    }
                }
            }

            var artistReportModels = new ArtistReportModels() { ArtistReports = artistReports };

            return PartialView("_ArtistReport", artistReportModels);

        }


        [HttpPost]
        public ActionResult CreateAlbum(CreateAlbumModel createAlbumModel)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"INSERT INTO album (title,description,genre,artist_name,releaseDate) " +
                    $"values ('{createAlbumModel.Title}','{createAlbumModel.Description}','{createAlbumModel.Genre}','{profile.Name}','{createAlbumModel.ReleaseDate}')";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }

                command = $"INSERT INTO user_albums (userId,albumId) " +
                $"values ('{profile.Id}',(SELECT id From album " +
                $"WHERE title = '{createAlbumModel.Title}' " +
                $"AND artist_name = '{profile.Name}' " +
                $"AND releaseDate = '{createAlbumModel.ReleaseDate}' " +
                $"LIMIT 1))";
                
                cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                
                if (cmd.ExecuteNonQuery() > 0)
                {
                
                }
            }
            return View("ArtistPage", (ProfileModel)Session["ProfileInfo"]);
        }

        Dictionary<string, int> GetDictionaryOfUserAlbums(int artistId)
        {
            Dictionary<string, int> userAlbums = new Dictionary<string, int>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT album.id, album.title" +

                    $" FROM user,user_albums,album" +

                    $" WHERE user.id = user_albums.userId" +
                    $" AND user_albums.albumId = album.id" +
                    $" AND user.id = {artistId}" +
                    $" AND album.isArchived = 0";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    string title = Convert.ToString(reader["title"]);
                    userAlbums.Add(title, id);
                }
            }
            return userAlbums;
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
                string command = $"SELECT song.id, song.title, song.genre, artist_name, likes, views" +

                    $" FROM album, album_songs, song" +

                    $" WHERE album.id = album_songs.albumId" +
                    $" AND album_songs.songId = song.id" +
                    $" AND album.id = {albumId}" +
                    $" AND album.isArchived = 0" +
                    $" AND song.isArchived = 0";

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
                            Artist = reader["artist_name"].ToString(),
                            Likes = Convert.ToInt32(reader["likes"]),
                            Views = Convert.ToInt32(reader["views"]),
                            UserIsArtist = true
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
                cmd.CommandTimeout = 200;
                con.Open();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        byte[] buffer = new byte[4096]; // create a buffer to read the BLOB data in chunks
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            long bytesRead;
                            long startIndex = 0;
                            while ((bytesRead = reader.GetBytes(0, startIndex, buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, (int)bytesRead); // write the data to a memory stream
                                startIndex += bytesRead;
                            }
                            return memoryStream.ToArray(); // return the byte array
                        }
                    }
                }
            }

            return null;
        }

        public ActionResult GetMilestones()
        {
            ProfileModel profileModel = (ProfileModel)Session["ProfileInfo"];
            List<MilestoneModel> list = new List<MilestoneModel>();

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"SELECT title,songId" +

                    $" FROM milestone" +

                    $" WHERE milestone.userId = {profileModel.Id}" +
                    $" AND milestone.isArchived = 0" +
                    $" ORDER BY created_at DESC";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MilestoneModel notificationModel = new MilestoneModel()
                    {
                        SongName = reader["title"].ToString(),
                        SongId = Convert.ToInt32(reader["songId"]),
                    };
                    list.Add(notificationModel);
                }
            }
            MilestonesModel milestonesModel = new MilestonesModel()
            {
                Milestones = list
            };
            return PartialView("_MilestonesList", milestonesModel);
        }

        public ActionResult ArchiveMilestone(int milestoneId)
        {
            ProfileModel profile = (ProfileModel)Session["ProfileInfo"];

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"UPDATE milestone SET isArchived = 1 WHERE userid = {profile.Id} AND songId = {milestoneId}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return null;
        }

        public ActionResult ArchiveSong(int songId)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"UPDATE song SET song.isArchived = 1 WHERE song.id = {songId}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return null;
        }

        public ActionResult ArchiveAlbum(int albumId)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                string command = $"UPDATE album SET album.isArchived = 1 WHERE album.id = {albumId}";

                MySqlCommand cmd = new MySqlCommand(command, con);
                cmd = new MySqlCommand(command, con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {

                }
            }
            return null;
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

    }
}