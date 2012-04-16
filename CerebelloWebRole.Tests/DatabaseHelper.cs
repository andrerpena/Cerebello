using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using Cerebello.Model;

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

                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText =
                        @"CREATE DATABASE CerebelloTEST ON 
                    ( FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.SQLEXPRESS\MSSQL\DATA\cerebelloTEST.mdf' )
                     FOR ATTACH ;";

                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // probably the database exists already because a previous test failed.. let's move on
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Clears all test database data
        /// </summary>
        public static void ClearAllData()
        {
            // clears the data
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString))
            {
                conn.Open();

                var command = conn.CreateCommand();

                command.CommandText = "sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
                command.ExecuteNonQuery();

                command.CommandText = "sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasForeignRef'') = 1 DELETE FROM ? else TRUNCATE TABLE ?'";
                command.ExecuteNonQuery();

                command.CommandText = "sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'";
                command.ExecuteNonQuery();

                command.CommandText = "sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1DBCC CHECKIDENT (''?'', RESEED, 0)'";
                command.ExecuteNonQuery();

                conn.Close();
            }
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
                SqlCommand cmd = new SqlCommand("", conn);
                cmd.CommandText = @"sp_detach_db CerebelloTEST";
                cmd.ExecuteNonQuery();
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
