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

        public void upload(string movieID, string title, string year, string description, Dictionary<string, string> starsList, Dictionary<string, string> directorsList, List<string> stillsList)
        {
            IDbCommand dbcmd = dbcon.CreateCommand();
            string mysqlQuery = "SELECT id, gid, title, directors, actors, year, description FROM imdb_test.films WHERE id > 0";
            dbcmd.CommandText = mysqlQuery;



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
