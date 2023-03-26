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

        public List<User> GetUserData()
        {
            List<User> users = new List<User>();
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT *, role.type AS role " +
                                                    $"FROM user " +
                                                    $"LEFT JOIN user_roles ON user.id = user_roles.userid " +
                                                    $"LEFT JOIN role ON role.id = user_roles.roleid", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User();
                    user.Id = Convert.ToInt32(reader["ID"]);
                    user.Username = reader["username"].ToString();
                    user.Role = reader["role"].ToString();
                    users.Add(user);
                }
                return users;
            }
        }

        public List<Song> GetSongData()
        {
            List<Song> songs = new List<Song>();
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                MySqlCommand cmd = new MySqlCommand($"SELECT * ,song.title AS songTitle, album.title AS albumTitle " +
                                                    $"FROM song " +
                                                    $"LEFT JOIN album_songs ON song.ID = album_songs.songId " +
                                                    $"LEFT JOIN album ON album.ID = album_songs.albumId;", con);
                cmd.CommandType = System.Data.CommandType.Text;
                con.Open();

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var song = new Song();
                    song.Title = reader["songTitle"].ToString();
                    song.Genre = reader["genre"].ToString();
                    song.Artist = reader["artist_name"].ToString();
                    song.AlbumName = reader["albumTitle"].ToString();
                    songs.Add(song);
                }
                return songs;
            }
        }
    }
}