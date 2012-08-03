using System;
using System.Collections.Generic;
using System.Configuration;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Models;
using System.IO;
using System.Text.RegularExpressions;
using Cerebello.Firestarter.Helpers;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            //var cbhpm = Cbhpm.LoadData();

            bool isToChooseDb = true;

            string connName = null;

            bool wasAttached = false;

            // New options:
            while (true)
            {
                if (isToChooseDb)
                {
                    if (wasAttached)
                    {
                        using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            Firestarter.DetachLocalDatabase(db);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("DB detached.");
                    }

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
                    string userOption = Console.ReadLine();
                    if (!int.TryParse(userOption, out idx) || !connStr.TryGetValue(idx.ToString(), out connName))
                        return;

                    if (string.IsNullOrWhiteSpace(connName))
                        return;

                    using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                    {
                        wasAttached = Firestarter.AttachLocalDatabase(db);
                        if (wasAttached)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("DB attached!");
                        }
                    }
                }

                isToChooseDb = false;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(string.Format("Current DB: {0}", connName));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("nu  - Create new user.");
                Console.WriteLine("cls - Clear all data.");
                Console.WriteLine("p1  - Populate database with items (type p1? to know more).");
                Console.WriteLine("sys - Initialize DB with system data.");
                Console.WriteLine("drp - Drop all tables and FKs.");
                Console.WriteLine("crt - Create all tables and FKs using script.");
                Console.WriteLine("db  - Change database.");
                Console.WriteLine("q   - Quit.");
                Console.WriteLine("    Type ? after any option to get help.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Write("What would you like to do with the DB: ");
                string userOption1 = Console.ReadLine();

                switch (userOption1.Trim().ToLowerInvariant())
                {
                    case "crt?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Loads the file 'script.sql' and executes it.");
                            Console.WriteLine("You must tell who you are, so that the right file is loaded.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "crt":
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(@"a = André");
                            Console.WriteLine(@"c = C:\Cerebello");
                            Console.WriteLine(@"m = MASB");
                            Console.ForegroundColor = ConsoleColor.White;
                            userOption1 = Console.ReadLine();

                            // ToDo: figure out a way to remove this.. we should have a common path or something
                            var dicPath = new Dictionary<string, string>
                            {
                                { "a", @"D:\Projetos\Azure\Cerebello\DB\Scripts" },
                                { "c", @"C:\Cerebello\DB\Scripts" },
                                { "m", @"P:\Projects.2012\Cerebello\DB\Scripts" },
                            };

                            string path;
                            dicPath.TryGetValue(userOption1, out path);

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

                    case "drp?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Methods that will be called:");
                            Console.WriteLine("    DropAllTables");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "drp":
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

                    case "sys?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Methods that will be called:");
                            Console.WriteLine("    InitializeDatabaseWithSystemData");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
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

                    case "nu?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("No help yet.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
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
                                var userToCreate = new CreateAccountViewModel
                                {
                                    UserName = userName,
                                    Password = userPassword,
                                    ConfirmPassword = userPassword,
                                    EMail = userEmail,
                                    FullName = userFullName,
                                    DateOfBirth = DateTime.Now.AddYears(-userAge),
                                };

                                // Verifying user-name.
                                User user;
                                var result = CerebelloWebRole.Code.SecurityManager.CreateUser(out user, userToCreate, db);

                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine(string.Format("PasswordSalt = \"{0}\";", user.PasswordSalt));
                                Console.WriteLine(string.Format("Password = \"{0}\";", user.Password));

                                if (result == CerebelloWebRole.Code.Security.CreateUserResult.Ok)
                                {
                                    // This does not work because a practice is needed...
                                    // Implement this when needed... for now, I just need the pwd-salt and pwd-hash.
                                    //db.SaveChanges();
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(string.Format("Failed: {0}", result));
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                        break;

                    case "cls?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Methods that will be called:");
                            Console.WriteLine("    ClearAllData");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
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

                    case "p1?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Methods that will be called:");
                            Console.WriteLine("    InitializeDatabaseWithSystemData");
                            Console.WriteLine("    CreateFakeUserAndPractice_2");
                            Console.WriteLine("    SetupDoctor");
                            Console.WriteLine("    SetupUserData");
                            Console.WriteLine("    CreateFakePatients");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
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

                    case "q?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Quits the program.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "q":
                        // Dettaching previous DB if it was attached in this session.
                        if (wasAttached)
                        {
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                Firestarter.DetachLocalDatabase(db);

                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("DB detached.");
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Bye!");
                        return;

                    case "db?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Changes the current database.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "db":
                        isToChooseDb = true;

                        break;

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