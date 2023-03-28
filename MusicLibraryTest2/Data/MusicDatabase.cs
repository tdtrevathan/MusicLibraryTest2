using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using MusicLibraryTest2.Models;
using MySqlConnector;

namespace MusicLibraryTest2.Data
{

    class MusicDatabase
    {
        string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public List<User> GetUsersData(DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<User> users = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT user.id, user.username, user.created_at, user.modified_at, user.last_login_at, user.is_archived, role.type AS role " +
                                                    $"FROM user " +
                                                    $"LEFT JOIN user_roles ON user.id = user_roles.userid " +
                                                    $"LEFT JOIN role ON role.id = user_roles.roleid " +
                                                    $"{(fromDate != null ? $"WHERE created_at >= '{fromDate.Value.ToString("yyyy-MM-dd")}' " : "")}" +
                                                    $"{(toDate != null ? $"{(fromDate != null ? "AND" : "WHERE")} created_at <= '{toDate.Value.ToString("yyyy-MM-dd 23:59:59")}'" : "")}", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User();
                    user.Id = Convert.ToInt32(reader["id"]);
                    user.Username = reader["username"].ToString();
                    user.Role = reader["role"].ToString();
                    user.CreatedAt = (reader["created_at"] as DateTime?).GetValueOrDefault();
                    user.ModifiedAt = (reader["modified_at"] as DateTime?).GetValueOrDefault();
                    user.LastLoginAt = (reader["last_login_at"] as DateTime?).GetValueOrDefault();
                    user.IsArchived = (reader["is_archived"] as bool?).GetValueOrDefault(false);
                    users.Add(user);
                }
                return users;
            }
        }

        public List<Song> GetSongData(DateTime? fromDate = null, DateTime? toDate = null)
        {
            List<Song> songs = new List<Song>();
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT song.id, song.title AS songTitle, song.views, song.likes, song.genre,  user.username AS artist_name, album.title AS albumTitle, song.created_at, song.modified_at, song.isarchived " +
                                                    $"FROM song " +
                                                    $"LEFT JOIN album_songs ON song.ID = album_songs.songId " +
                                                    $"LEFT JOIN album ON album.ID = album_songs.albumId " +
                                                    $"LEFT JOIN user_songs ON user_songs.songid = song.id " +
                                                    $"LEFT JOIN user ON user_songs.userid = user.id " +
                                                    $"{(fromDate != null || toDate != null ? "WHERE " : "")}" +
                                                    $"{(fromDate != null ? $"created_at >= '{fromDate.Value.ToString("yyyy-MM-dd")}'" : "")}" +
                                                    $"{(fromDate != null && toDate != null ? " AND " : "")}" +
                                                    $"{(toDate != null ? $"created_at <= '{toDate.Value.ToString("yyyy-MM-dd 23:59:59")}'" : "")}", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var song = new Song();
                    song.Id = Convert.ToInt32(reader["id"]);
                    song.Views = Convert.ToInt32(reader["views"]);
                    song.Likes = Convert.ToInt32(reader["likes"]);
                    song.Title = reader["songTitle"].ToString();
                    song.Genre = reader["genre"].ToString();
                    song.Artist = reader["artist_name"].ToString();
                    song.AlbumName = reader["albumTitle"].ToString();
                    song.CreatedAt = (reader["created_at"] as DateTime?).GetValueOrDefault();
                    song.ModifiedAt = (reader["modified_at"] as DateTime?).GetValueOrDefault();
                    song.IsArchived = (reader["isarchived"] as bool?).GetValueOrDefault(false);
                    songs.Add(song);
                }
                return songs;
            }
        }
    }
}