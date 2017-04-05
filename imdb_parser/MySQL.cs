using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace imdb_parser
{
    class MySQL
    {
        IDbConnection dbcon;
        public void connect(string host, string port, string db, string username, string password)
        {
            Console.WriteLine("----------------------");
            Console.WriteLine("Connecting to MySQL DB");
            string connectMysql = "server=" + host +
                                  ";port=" + port +
                                  ";database=" + db +
                                  ";userid=" + username +
                                  ";password=" + password;
            dbcon = new MySqlConnection(connectMysql);
            dbcon.Open();
        }

        public void upload(string movieID, string title, string year, List<string> genresList, string description, Dictionary<string, string> actorsList, Dictionary<string, string> directorsList, List<string> stillsList, Dictionary<string, string> actorsPhotos)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string genresComma = "";
            string actorCodesComma = "";
            string directorsComma = "";

            //combine all genres in one string
            for (int i = 0; i < genresList.Count; i++) { genresComma += genresList.ElementAt(i) + ","; }
            //combine all actors in one string
            for (int i = 0; i < actorsList.Count; i++) { actorCodesComma += actorsList.Values.ElementAt(i) + ","; }
            //combine all directors in one string
            for (int i = 0; i < directorsList.Count; i++) { directorsComma += directorsList.Values.ElementAt(i) + ","; }

            //add a new film to imdb_test.films
            //but first check if our film is already in the db
            int filmCount = mysqlExists(dbcmd, "SELECT gid FROM imdb_test.films WHERE gid = '" + movieID + "';");
            if (filmCount > 0) { Console.WriteLine("Film {0} was found, do nothing.", movieID); }
            else
            {
                Console.WriteLine("Film {0} was not found, adding...", movieID);
                string mysqlInsertFilms = "INSERT INTO imdb_test.films (gid, title, directors, actors, year, description, genres) VALUES ('"
                    + movieID + "', '"
                    + trimChars(title) + "', '"
                    + directorsComma.TrimEnd(',') + "', '"
                    + actorCodesComma.TrimEnd(',') + "', "
                    + year + ", '"
                    + trimChars(description) + "', '"
                    + genresComma.TrimEnd(',') + "');";
                dbcmd.CommandText = mysqlInsertFilms;
                dbcmd.ExecuteNonQuery();

                //TODO: rewrite this bit (can be done 1 query)
                //add new actors and directors to imdb_test.actors and imdb_test.directors
                foreach (KeyValuePair<string, string> actor in actorsList)
                {
                    //check if actor is already present
                    int actorCount = mysqlExists(dbcmd, "SELECT actor_id FROM imdb_test.actors WHERE actor_id = '" + actor.Value + "';");
                    if (actorCount > 0) { Console.WriteLine("Actor {0} was found, do nothing.", actor.Value); }
                    else
                    {
                        Console.WriteLine("Actor {0} was not found, adding...", actor.Value);
                        string mysqlInsertActors = "INSERT INTO imdb_test.actors (actor_name, actor_id) VALUES ('" + trimChars(actor.Key) + "', '" + actor.Value + "');";
                        dbcmd.CommandText = mysqlInsertActors;
                        dbcmd.ExecuteNonQuery();
                    }
                }
                
                //add actors' photos
                foreach (KeyValuePair<string, string> photo in actorsPhotos)
                {
                    //check if photo is already present
                    int photoCount = mysqlExists(dbcmd, "SELECT url FROM imdb_test.actors WHERE actor_id = '" + photo.Key + "';");
                    if (photoCount > 0) { Console.WriteLine("Actor's photo {0} was found, do nothing.", photo.Key); }
                    else
                    {
                        Console.WriteLine("Actor's photo {0} was not found, adding url...", photo.Key);
                        string mysqlInsertActors = "UPDATE imdb_test.actors SET url = '" + photo.Value + "' WHERE actor_id = '" + photo.Key + "';";
                        dbcmd.CommandText = mysqlInsertActors;
                        dbcmd.ExecuteNonQuery();
                    }
                }

                foreach (KeyValuePair<string, string> director in directorsList)
                {
                    //check if director is already present
                    int directorCount = mysqlExists(dbcmd, "SELECT director_id FROM imdb_test.directors WHERE director_id = '" + director.Value + "';");
                    if (directorCount > 0) { Console.WriteLine("Director {0} was found, do nothing.", director.Value); }
                    else
                    {
                        Console.WriteLine("Director {0} was not found, adding...", director.Value);
                        string mysqlInsertDirectors = "INSERT INTO imdb_test.directors (director_name, director_id) VALUES ('" + trimChars(director.Key) + "', '" + director.Value + "');";
                        dbcmd.CommandText = mysqlInsertDirectors;
                        dbcmd.ExecuteNonQuery();
                    }
                }
                //add stills
                foreach (string still in stillsList)
                {
                    string stillFixed = still.Replace(",100,100", "").Replace("UY100", "").Replace("UX100", "");
                    string mysqlInsertStills = "INSERT INTO imdb_test.stills (gid, url) VALUES ('" + movieID + "', '" + stillFixed + "');";
                    dbcmd.CommandText = mysqlInsertStills;
                    dbcmd.ExecuteNonQuery();
                }

                // clean up the mysql connection
                dbcmd.Dispose();
                dbcmd = null;
            }
        }

        public void disconnect()
        {
            dbcon.Close();
            dbcon = null;
        }

        public static string trimChars(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
        }

        public int mysqlExists(IDbCommand dbcmd, string mysqlQuery)
        {
            //check if record exists
            List<string> firstResult = new List<string>();
            dbcmd.CommandText = mysqlQuery;

            using (IDataReader rdr = dbcmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    try
                    {
                        var myString = rdr.GetString(0);
                        firstResult.Add(myString);
                    }
                    catch
                    {
                        firstResult.Clear();
                    }
                }
                rdr.Close();
            }

            return firstResult.Count();
        }
    }
}