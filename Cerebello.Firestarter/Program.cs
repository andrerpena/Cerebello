using System;
using System.Collections.Generic;
using System.Configuration;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            // Getting available connections from ConfigurationManager so that user can choose one.
            Console.Clear();
            Console.WriteLine("Choose connection to use:");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Dictionary<string, string> connStr = new Dictionary<string, string>();
            for (int idxConnStr = 0; idxConnStr < ConfigurationManager.ConnectionStrings.Count; idxConnStr++)
            {
                var connStrSettings = ConfigurationManager.ConnectionStrings[idxConnStr];
                connStr[idxConnStr.ToString()] = connStrSettings.Name;
                Console.WriteLine(string.Format("{0}: {1}", idxConnStr, connStrSettings.Name));
            }
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
            Console.Write("Type the option number and press Enter: ");

            // User may now choose a connection.
            int idx;
            string connName;
            var userOption = Console.ReadLine();
            if (!int.TryParse(userOption, out idx) || !connStr.TryGetValue(idx.ToString(), out connName))
                return;

            // New options:
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(string.Format("Current DB: {0}", connName));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("nu  - Create new user.");
                Console.WriteLine("cls - Clear all data.");
                Console.WriteLine("p1  - Populate setup 1.");
                Console.WriteLine("sys - Initialize DB with system data.");
                Console.WriteLine("dp  - Drop all tables and FKs.");
                Console.WriteLine("c   - Create all tables and FKs using script.");
                Console.WriteLine("q   - Quit.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Write("What would you like to do with the DB: ");
                userOption = Console.ReadLine();

                switch (userOption.Trim().ToLowerInvariant())
                {
                    case "c":
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("m = MASB");
                            Console.WriteLine("a = André");
                            Console.ForegroundColor = ConsoleColor.White;
                            userOption = Console.ReadLine();

                            var dicPath = new Dictionary<string, string>
                            {
                                { "m", @"P:\Projects.2012\Cerebello\DB\Scripts" },
                                { "a", @"?????" },
                            };

                            string path;
                            dicPath.TryGetValue(userOption, out path);

                            try
                            {
                                if (path != null)
                                {
                                    string script = File.ReadAllText(Path.Combine(path, "script.sql"));
                                    using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                    {
                                        Firestarter.ExecuteScript(db, script);
                                    }
                                }

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Done!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(string.Format("Exception: {0}", ex.GetType().Name));
                                Console.WriteLine(ex.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        break;

                    case "dp":
                        {
                            Console.Write("This will drop EVERY table in your DB... are you sure? (y/n): ");
                            var userDropAll = Console.ReadLine();
                            if (userDropAll != "y" && userDropAll != "n")
                                continue;
                            bool clearAllData = userDropAll == "y";

                            // Doing what the user has told.
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            {
                                if (clearAllData)
                                {
                                    Firestarter.DropAllTables(db);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Done!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Canceled!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }

                        break;

                    case "sys":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("DB sys tables will be populated with:");
                            Console.WriteLine("    SYS_MedicalEntity: TISS - Conselhos Profissionais");
                            Console.WriteLine("    SYS_MedicalSpecialty: TISS - CBO-S (especialidades)");
                            Console.WriteLine();

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Continue? (y/n): ");
                            ConsoleKeyInfo key = default(ConsoleKeyInfo);
                            if (key.KeyChar != 'y' && key.KeyChar != 'n')
                                key = Console.ReadKey();
                            Console.WriteLine();

                            if (key.KeyChar == 'y')
                            {
                                using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                    Firestarter.InitializeDatabaseWithSystemData(db);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Done!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Canceled!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        break;

                    case "nu":
                        {
                            // Asking for user data.
                            Console.WriteLine("UserName: ");
                            var userName = Console.ReadLine();
                            Console.WriteLine("Password: ");
                            var userPassword = Console.ReadLine();
                            Console.WriteLine("Email: ");
                            var userEmail = Console.ReadLine();
                            Console.WriteLine("Full name (default: =UserName): ");
                            var userFullName = Console.ReadLine();
                            if (string.IsNullOrEmpty(userFullName))
                                userFullName = userName;
                            Console.WriteLine("Age (default: 30): ");
                            int userAge;
                            if (!int.TryParse(Console.ReadLine(), out userAge))
                                userAge = 30;

                            // Creating user.
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            {
                                var user = CerebelloWebRole.Code.SecurityManager.CreateUser(new CreateAccountViewModel
                                    {
                                        UserName = userName,
                                        Password = userPassword,
                                        ConfirmPassword = userPassword,
                                        EMail = userEmail,
                                        FullName = userFullName,
                                        DateOfBirth = DateTime.Now.AddYears(-userAge),
                                    }, db);

                                // This does not work because a practice is needed...
                                // Implement this when needed... for now, I just need the pwd-salt and pwd-hash.
                                //db.SaveChanges();

                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine(string.Format("PasswordSalt = \"{0}\";", user.PasswordSalt));
                                Console.WriteLine(string.Format("Password = \"{0}\";", user.Password));
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        break;

                    case "cls":
                        {
                            Console.Write("Clear all data (y/n): ");
                            var userClearAll = Console.ReadLine();
                            if (userClearAll != "y" && userClearAll != "n")
                                continue;
                            bool clearAllData = userClearAll == "y";

                            // Doing what the user has told.
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            {
                                if (clearAllData)
                                {
                                    Firestarter.ClearAllData(db);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Done!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Canceled!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }

                        break;

                    case "p1":
                        using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("InitializeDatabaseWithSystemData");
                            Firestarter.InitializeDatabaseWithSystemData(db);

                            Console.WriteLine("CreateFakeUserAndPractice_2");
                            var listDoctors = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(db);

                            db.SaveChanges();

                            Console.WriteLine("SetupDoctor");
                            foreach (var doctor in listDoctors)
                                Firestarter.SetupDoctor(doctor, db);

                            db.SaveChanges();

                            Console.WriteLine("SetupUserData");
                            foreach (var doctor in listDoctors)
                                Firestarter.SetupUserData(doctor, db);

                            db.SaveChanges();

                            Console.WriteLine("CreateFakePatients");
                            foreach (var doctor in listDoctors)
                                Firestarter.CreateFakePatients(doctor, db);

                            db.SaveChanges();

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Done!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        break;

                    case "q":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Bye!");
                        return;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option.");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
        }
    }
}