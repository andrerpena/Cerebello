using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Cerebello.Model;
using Cerebello.Firestarter;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Helps attaching and detaching databases.
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        /// Attaches the test database and clears all data
        /// </summary>
        public static void AttachCerebelloTestDatabase()
        {
            // attaches the database
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDBServer"].ConnectionString))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    var databaseName = "CerebelloTEST";

                    // verify the DB existence prior to creating a new one
                    command.CommandText = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);

                    object databaseId = command.ExecuteScalar();
                    if (databaseId == null)
                    {
                        // according to stackoverflow it's ok to reuse commands as long as the params are cleared
                        command.CommandText = string.Format(
                            @"CREATE DATABASE {0} ON 
                                    ( FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\cerebelloTEST.mdf' )
                                     FOR ATTACH ;", databaseName);

                        command.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Clears all test database data
        /// </summary>
        [Obsolete("This is now in the 'FireStarter' class.")]
        public static void ClearAllData()
        {
        }

        /// <summary>
        /// Detaches the test database
        /// </summary>
        public static void DetachCerebelloTestDatabase()
        {
            SqlConnection.ClearAllPools();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDBServer"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmdExists = new SqlCommand("", conn);
                cmdExists.CommandText = string.Format(
                    "SELECT db_id(N'{0}')",
                    "CerebelloTEST");

                var dbExists = cmdExists.ExecuteScalar();

                if (!(dbExists == null || dbExists is DBNull))
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandText = "sp_detach_db CerebelloTEST";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Returns a test object context
        /// </summary>
        /// <returns></returns>
        public static CerebelloEntities GetTestObjectContext()
        {
            return new CerebelloEntities("name=TestDBEF");
        }
    }
}
