using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
            string connectMysql = "server="     + host +
                                  ";port="      + port +
                                  ";database="  + db +
                                  ";userid="    + username +
                                  ";password="  + password;
            dbcon = new MySqlConnection(connectMysql);
            dbcon.Open();
        }

        public void upload(string movieID, string title, string year, List<string> genresList, string description, Dictionary<string, string> starsList, Dictionary<string, string> directorsList, List<string> stillsList)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string genresComma = "";
            string actorsComma = "";
            string directorsComma = "";

            //combine all genres in one string
            for (int i = 0; i < genresList.Count; i++) { genresComma += genresList.ElementAt(i) + ",";  }
            //combine all actors in one string
            for (int i = 0; i < starsList.Count; i++) { actorsComma += starsList.Values.ElementAt(i) + ","; }
            //combine all directors in one string
            for (int i = 0; i < directorsList.Count; i++) { directorsComma += directorsList.Values.ElementAt(i) + ","; }

            //add a new film to imdb_test.films
            string mysqlQuery = "INSERT INTO imdb_test.films (gid, title, directors, actors, year, description, genres) VALUES ('"
                + movieID + "', '"
                + title + "', '"
                + directorsComma.TrimEnd(',') + "', '"
                + actorsComma.TrimEnd(',') + "', "
                + year + ", '"
                + description + "', '"
                + genresComma.TrimEnd(',') + "');";

            dbcmd.CommandText = mysqlQuery;
            dbcmd.ExecuteNonQuery();

/*            string mysqlQuery = "SELECT id, gid, title, directors, actors, year, description FROM imdb_test.films WHERE id > 0";
            dbcmd.CommandText = mysqlQuery;

            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("---------------------------------------------");

                string id = (string)reader["id"].ToString();
                string gid = (string)reader["gid"].ToString();
                string year1 = (string)reader["year"].ToString();
                string title1 = (string)reader["title"].ToString();
                string actors = (string)reader["actors"].ToString();
                string directors = (string)reader["directors"].ToString();
                string description1 = (string)reader["description"].ToString();

                Console.WriteLine("[debug] {0} {1} {2} {3} {4} {5} {6}", id, gid, year1, title1, actors, directors, description1);
            }
            reader.Close();
            reader = null; */

            // clean up the mysql connection
            dbcmd.Dispose();
            dbcmd = null;
        }

        public void disconnect()
        {
            dbcon.Close();
            dbcon = null;
        }
    }
}
