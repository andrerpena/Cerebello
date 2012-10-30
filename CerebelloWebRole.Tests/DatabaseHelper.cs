using System.Configuration;
using System.Data.SqlClient;

namespace CerebelloWebRole.Tests
{
    /// <summary>
    /// Helps attaching and detaching databases.
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        /// Attaches the test database and clears all data.
        /// </summary>
        public static void AttachCerebelloTestDatabase()
        {
            // attaches the database
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDBServer"].ConnectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(string.Format(@"
    Declare @found_dn_name varchar(200)
    SELECT DISTINCT @found_dn_name=name
        FROM sys.sysdatabases
        WHERE name = '{0}'
    if (@found_dn_name is null)
    begin
        USE master
		CREATE DATABASE {0}
			ON (FILENAME = N'{1}')
			FOR ATTACH
    end
", "CerebelloTEST", @"C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\cerebelloTEST.mdf"), conn);

                using (cmd)
                    cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Detaches the test database.
        /// </summary>
        public static void DetachCerebelloTestDatabase()
        {
            SqlConnection.ClearAllPools();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDBServer"].ConnectionString))
            {
                conn.Open();

                var cmd = new SqlCommand(string.Format(@"
    Declare @found_dn_name varchar(200)
    SELECT DISTINCT @found_dn_name=name
        FROM sys.sysdatabases
        WHERE name = '{0}'
    if (not @found_dn_name is null)
    begin
        USE master
        ALTER DATABASE {0}
           SET SINGLE_USER WITH ROLLBACK IMMEDIATE
        EXEC sp_detach_db @dbname = '{0}', @skipchecks = 'true'
    end
", "CerebelloTEST"), conn);

                using (cmd)
                    cmd.ExecuteNonQuery();
            }
        }
    }
}
