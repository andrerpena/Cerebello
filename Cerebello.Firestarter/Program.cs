using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Models;
using System.IO;
using System.Text.RegularExpressions;
using Cerebello.Firestarter.Helpers;
using System.Linq;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCerebelloPath = ConfigurationManager.AppSettings["CerebelloPath"];

            bool isToChooseDb = true;

            string connName = null;

            bool wasAttached = false;

            // New options:
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("DateTime (UTC):   {0}", DateTime.UtcNow);
                Console.WriteLine("DateTime (Local): {0}", DateTime.Now);
                Console.WriteLine("DateTime (?):     {0}", DateTime.Now.ToUniversalTime());

                while (isToChooseDb)
                {
                    if (wasAttached)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("Detaching DB... ");

                        using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            Firestarter.DetachLocalDatabase(db);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("DONE");
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;

                    // Getting available connections from ConfigurationManager so that user can choose one.
                    Console.WriteLine("Choose connection to use:");

                    var connStr = new Dictionary<string, string>();
                    for (int idxConnStr = 0; idxConnStr < ConfigurationManager.ConnectionStrings.Count; idxConnStr++)
                    {
                        var connStrSettings = ConfigurationManager.ConnectionStrings[idxConnStr];
                        connStr[idxConnStr.ToString()] = connStrSettings.Name;
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("{0}: ", idxConnStr);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(connStrSettings.Name);
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine(@"cls: Clear screen.");
                    Console.WriteLine(@"q:   Quit!");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine();
                    Console.Write("Type the option and press Enter: ");

                    // User may now choose a connection.
                    int idx;
                    string userOption = Console.ReadLine().ToLowerInvariant().Trim();
                    if (!int.TryParse(userOption, out idx) || !connStr.TryGetValue(idx.ToString(), out connName))
                    {
                        if (userOption == "q" || userOption == "quit")
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Bye!");
                            return;
                        }

                        if (userOption == "cls")
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Clear();
                            continue;
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option.");
                        Console.WriteLine();
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(connName))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Cannot connect because the connection string is empty.");
                        Console.WriteLine();
                        continue;
                    }

                    try
                    {
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
                    catch (Exception ex)
                    {
                        ConsoleWriteException(ex);
                        Console.WriteLine();
                        continue;
                    }

                    break;
                }

                isToChooseDb = false;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Current DB: {0}", connName);
                Console.WriteLine("Project Path: {0}", rootCerebelloPath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"nu  - Create new user.");
                Console.WriteLine(@"clr - Clear all data.");
                Console.WriteLine(@"p1  - Populate database with items (type p1? to know more).");
                Console.WriteLine(@"sys - Initialize DB with system data.");
                Console.WriteLine(@"drp - Drop all tables and FKs.");
                Console.WriteLine(@"crt - Create all tables and FKs using script.");
                Console.WriteLine(@"db  - Change database.");
                Console.WriteLine(@"cls - Clear screen.");

                Console.WriteLine(wasAttached ? @"atc - Leaves DB attached when done." : @"dtc - Detach DB when done.");

                Console.WriteLine(@"q   - Quit.");
                Console.WriteLine(@"    Type ? after any option to get help.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Write("What would you like to do with the DB: ");
                string userOption1 = Console.ReadLine();

                switch (userOption1.Trim().ToLowerInvariant())
                {
                    case "atc?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("When you are done with the current DB, it will be left attached.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "atc":
                        wasAttached = false;
                        break;

                    case "dtc?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("When you are done with the current DB, it is going to be detached.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "dtc":
                        wasAttached = true;
                        break;

                    case "crt?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine(@"Loads the file '\DB\Scripts\script.sql',");
                            Console.WriteLine("Changes the collation of columns in the script to 'Latin1_General_CI_AI',");
                            Console.WriteLine("and then executes the changed script.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "crt":
                        {
                            try
                            {
                                using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                    Program.CreateDatabaseUsingScript(db, rootCerebelloPath);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Done!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            catch (Exception ex)
                            {
                                ConsoleWriteException(ex);
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
                            Console.WriteLine("Gives options to initialize SYS tables:");
                            Console.WriteLine("1: SYS_MedicalEntity: TISS - Conselhos Profissionais");
                            Console.WriteLine("2: SYS_MedicalSpecialty: TISS - CBO-S (especialidades)");
                            Console.WriteLine("3: SYS_MedicalProcedures: CBHPM (procedimentos médicos)");
                            Console.WriteLine("4: Initialize_SYS_Cid10: CID10");
                            Console.WriteLine("5: Initialize_SYS_Contracts: SaaS contracts");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "sys":
                        {
                            var defaultOptions = new List<int> { 1, 2, 3, 4, 5 };
                            var optionsSys = new List<int>(defaultOptions);
                            while (true)
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine();

                                Console.WriteLine("SYS tables will be populated with:");

                                Console.WriteLine(
                                    "    ({0}) 1: SYS_MedicalEntity: TISS - Conselhos Profissionais",
                                    optionsSys.Contains(1) ? 'x' : ' ');

                                Console.WriteLine(
                                    "    ({0}) 2: SYS_MedicalSpecialty: TISS - CBO-S (especialidades)",
                                    optionsSys.Contains(2) ? 'x' : ' ');

                                Console.WriteLine(
                                    "    ({0}) 3: SYS_MedicalProcedures: CBHPM (procedimentos médicos)",
                                    optionsSys.Contains(3) ? 'x' : ' ');
                                Console.WriteLine(@"        CBHPM data come from text file: \DB\cbhpm_2010.txt.");

                                Console.WriteLine(
                                    "    ({0}) 4: Initialize_SYS_Cid10: CID10",
                                    optionsSys.Contains(4) ? 'x' : ' ');

                                Console.WriteLine(
                                    "    ({0}) 5: Initialize_SYS_Contracts: SaaS contracts",
                                    optionsSys.Contains(4) ? 'x' : ' ');

                                Console.WriteLine();

                                // options related to sys tables
                                Console.WriteLine("Type an option to include or exclude it.");
                                Console.WriteLine("Type D for default, A for all, and N for none.");
                                Console.WriteLine("Press enter when ready, or Escape to cancel.");
                                ConsoleKeyInfo key1 = default(ConsoleKeyInfo);

                                key1 = Console.ReadKey();

                                int val;
                                if (int.TryParse(key1.KeyChar.ToString(), out val))
                                {
                                    if (optionsSys.Contains(val)) optionsSys.Remove(val);
                                    else optionsSys.Add(val);
                                }

                                if (key1.KeyChar == 'D' || key1.KeyChar == 'd')
                                {
                                    optionsSys = new List<int>(defaultOptions);
                                }

                                if (key1.KeyChar == 'N' || key1.KeyChar == 'n')
                                {
                                    optionsSys = new List<int> { };
                                }

                                if (key1.KeyChar == 'A' || key1.KeyChar == 'a')
                                {
                                    optionsSys = Enumerable.Range(1, 9).ToList();
                                }

                                if (key1.Key == ConsoleKey.Enter)
                                {
                                    break;
                                }

                                if (key1.Key == ConsoleKey.Escape)
                                {
                                    optionsSys = null;
                                    break;
                                }

                                Console.WriteLine();
                            }

                            if (optionsSys == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Canceled!");
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            }

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Continue? (y/n): ");
                            var key = default(ConsoleKeyInfo);
                            if (key.KeyChar != 'y' && key.KeyChar != 'n')
                                key = Console.ReadKey();
                            Console.WriteLine();

                            if (key.KeyChar == 'y')
                            {
                                try
                                {
                                    using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                    {
                                        if (optionsSys.Contains(1))
                                        {
                                            Console.WriteLine("Initialize_SYS_MedicalEntity");
                                            Firestarter.Initialize_SYS_MedicalEntity(db);
                                        }

                                        if (optionsSys.Contains(2))
                                        {
                                            Console.WriteLine("Initialize_SYS_MedicalSpecialty");
                                            Firestarter.Initialize_SYS_MedicalEntity(db);
                                        }

                                        if (optionsSys.Contains(3))
                                        {
                                            Console.WriteLine("Initialize_SYS_MedicalProcedures");
                                            Firestarter.Initialize_SYS_MedicalProcedures(
                                                    db,
                                                    Path.Combine(rootCerebelloPath, @"DB\cbhpm_2010.txt"),
                                                    progress: new Action<int, int>(ConsoleWriteProgressIntInt));
                                        }

                                        if (optionsSys.Contains(4))
                                        {
                                            Console.WriteLine("Initialize_SYS_Cid10");
                                            Firestarter.Initialize_SYS_Cid10(
                                                db,
                                                progress: new Action<int, int>(ConsoleWriteProgressIntInt));
                                        }

                                        if (optionsSys.Contains(5))
                                        {
                                            Console.WriteLine("Initialize_SYS_Contracts");
                                            Firestarter.Initialize_SYS_Contracts(db);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Program.ConsoleWriteException(ex);

                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Partially done!");
                                    Console.ForegroundColor = ConsoleColor.White;

                                    break;
                                }

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
                                    DateOfBirth = DateTime.UtcNow.AddYears(-userAge),
                                };

                                var utcNow = DateTime.UtcNow;

                                // Verifying user-name.
                                User user;
                                var result = CerebelloWebRole.Code.SecurityManager.CreateUser(out user, userToCreate, db, utcNow, null);

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
                            Console.WriteLine("Clears the output from all previous commands.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    case "clr?":
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

                    case "clr":
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
                                    try
                                    {
                                        Firestarter.ClearAllData(db);
                                    }
                                    catch (Exception ex)
                                    {
                                        Program.ConsoleWriteException(ex);
                                    }

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
                            Console.WriteLine("    Initialize_SYS_MedicalEntity");
                            Console.WriteLine("    Initialize_SYS_MedicalSpecialty");
                            Console.WriteLine("    Initialize_SYS_Contracts");
                            Console.WriteLine("    Initialize_SYS_Cid10");
                            Console.WriteLine("    Initialize_SYS_MedicalProcedures");
                            Console.WriteLine("    Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel");
                            Console.WriteLine("    SetupDoctor (for each doctor)");
                            Console.WriteLine("    CreateFakePatients (for each doctor)");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "p1":
                        try
                        {
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;

                                // Initializing system tables
                                InitSysTables(db, rootCerebelloPath);

                                OptionP1(db);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Done!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.ConsoleWriteException(ex);

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Partially done!");
                            Console.ForegroundColor = ConsoleColor.White;

                            break;
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
                        Console.WriteLine();

                        break;

                    case "r?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Resets the current database, to the default working state.");
                            Console.WriteLine("Work database is dropped, recreated, and populated with p1 option.");
                            Console.WriteLine("Test database is dropped, recreated, and populated with all SYS tables.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                        }

                        break;

                    case "r":
                        {
                            bool isTestDb = connName.ToUpper().Contains("TEST");

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("DB type: {0}", isTestDb ? "TEST" : "WORK");

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Reset the database (y/n): ");
                            var userReset = Console.ReadLine();
                            if (userReset != "y" && userReset != "n")
                                continue;
                            bool resetDb = userReset == "y";

                            Console.ForegroundColor = ConsoleColor.Gray;

                            // Doing what the user has told.
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                            {
                                if (resetDb)
                                {
                                    try
                                    {
                                        Console.WriteLine("DropAllTables");
                                        Firestarter.DropAllTables(db);

                                        Console.WriteLine("CreateDatabaseUsingScript");
                                        Program.CreateDatabaseUsingScript(db, rootCerebelloPath);

                                        InitSysTables(db, rootCerebelloPath);

                                        if (!isTestDb)
                                            OptionP1(db);
                                    }
                                    catch (Exception ex)
                                    {
                                        Program.ConsoleWriteException(ex);

                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("Partially done!");
                                        Console.ForegroundColor = ConsoleColor.White;

                                        break;
                                    }

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

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option.");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
        }

        private static void OptionP1(CerebelloEntities db)
        {
            // Create practice, contract, doctors and other things
            Console.WriteLine("Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel");
            var listDoctors = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(db);

            // Setup doctor schedule and document templates
            Console.WriteLine("SetupDoctor");
            foreach (var doctor in listDoctors)
                Firestarter.SetupDoctor(doctor, db);

            // Create patients
            Console.WriteLine("CreateFakePatients");
            foreach (var doctor in listDoctors)
                Firestarter.CreateFakePatients(doctor, db);
        }

        private static void InitSysTables(CerebelloEntities db, string rootCerebelloPath)
        {
            Console.WriteLine("Initialize_SYS_MedicalEntity");
            Firestarter.Initialize_SYS_MedicalEntity(db);

            Console.WriteLine("Initialize_SYS_MedicalSpecialty");
            Firestarter.Initialize_SYS_MedicalSpecialty(db);

            Console.WriteLine("Initialize_SYS_Contracts");
            Firestarter.Initialize_SYS_Contracts(db);

            Console.WriteLine("Initialize_SYS_Cid10");
            Firestarter.Initialize_SYS_Cid10(
                db,
                progress: new Action<int, int>(ConsoleWriteProgressIntInt));

            Console.WriteLine("Initialize_SYS_MedicalProcedures");
            Firestarter.Initialize_SYS_MedicalProcedures(
                db,
                Path.Combine(rootCerebelloPath, @"DB\cbhpm_2010.txt"),
                progress: new Action<int, int>(ConsoleWriteProgressIntInt));
        }

        private static void CreateDatabaseUsingScript(CerebelloEntities db, string rootCerebelloPath)
        {
            // ToDo: figure out a way to remove this.. we should have a common path or something
            var path = Path.Combine(rootCerebelloPath, @"DB\Scripts");
            string script = File.ReadAllText(Path.Combine(path, "script.sql"));
            var script2 = SqlHelper.SetScriptColumnsCollation(script, "Latin1_General_CI_AI");
            // Creating tables.
            Firestarter.ExecuteScript(db, script2);
        }

        private static void ConsoleWriteException(Exception ex)
        {
            var ex1 = ex;

            while (ex1 != null)
            {
                // exception type
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(string.Format("Exception: {0}", ex1.GetType().Name));
                Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));

                // message
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Red;
                var lines = Regex.Split(ex1.Message, @"\r\n|\r|\n");
                foreach (var eachLine in lines)
                {
                    Console.Write(eachLine);
                    if (Console.CursorLeft > 0)
                        Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                }

                // stack-trace
                Console.BackgroundColor = ConsoleColor.DarkRed;
                int fncLevel = 0;
                if (ex1.StackTrace != null)
                {
                    var linesStackTrace = Regex.Split(ex1.StackTrace, @"\r\n|\r|\n");
                    foreach (var eachLine in linesStackTrace)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(string.Format("{0:0000}:", fncLevel++));
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(eachLine);
                        if (Console.CursorLeft > 0)
                            Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                    }
                }

                // inner exception
                ex1 = ex1.InnerException;
                if (ex1 != null)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("==== inner exception ====");
                    if (Console.CursorLeft > 0)
                        Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ConsoleWriteProgressIntInt(int x, int y)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (x == 0 || x == y || x * 10 / y != (x - 1) * 10 / y)
                Console.WriteLine(string.Format("    {0} of {1} - {2}%", x, y, x * 100 / y));
            Console.ForegroundColor = oldColor;
        }
    }
}