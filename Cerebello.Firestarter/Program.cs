using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Reflection;
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

            bool showHidden = false;

            Console.BufferWidth = 200;

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
                Console.WriteLine(@"r      - Magic option: reset database (differentiates WORK and TEST database).");
                Console.WriteLine(@"db     - Change database.");

                if (showHidden)
                {
                    Console.WriteLine(@"clr    - Clear all data.");
                    Console.WriteLine(@"p1     - Populate database with items (type p1? to know more).");
                    Console.WriteLine(@"drp    - Drop all tables and FKs.");
                    Console.WriteLine(@"crt    - Create all tables and FKs using script.");
                    Console.WriteLine(@"cls    - Clear screen.");
                    Console.WriteLine(wasAttached
                        ? @"atc    - Leaves DB attached when done."
                        : @"dtc    - Detach DB when done.");
                }

                Console.WriteLine(string.Format(@"RETURN - {0} options.", showHidden ? "Hides visible" : "Shows hidden"));

                Console.WriteLine(@"q      - Quit.");
                Console.WriteLine(@"       Type ? after any option to get help.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Write("What would you like to do with the DB: ");
                string userOption1 = Console.ReadLine();

                switch (userOption1.Trim().ToLowerInvariant())
                {
                    case "":

                        showHidden = !showHidden;

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(showHidden ? "Complex options: VISIBLE" : "Complex options: HIDDEN");

                        break;

                    case "atc?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("When you are done with the current DB, it will be left attached.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                            Console.WriteLine();
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
                            Console.WriteLine();
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
                            Console.WriteLine();
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
                            Console.WriteLine();
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

                    case "cls?":
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine();
                            Console.WriteLine("Clears the output from all previous commands.");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                            Console.WriteLine();
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
                            Console.WriteLine();
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
                            Console.WriteLine("    CreateSecretary_Milena");
                            Console.WriteLine("    SetupDoctor (for each doctor)");
                            Console.WriteLine("    CreateFakePatients (for each doctor)");
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Press any key to continue.");
                            Console.ReadKey();
                            Console.WriteLine();
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
                            Console.WriteLine();
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
                            Console.WriteLine();
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
                            Console.WriteLine();
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

                    case "exec":

                        try
                        {
                            Console.Write("Command: ");
                            using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                                new Exec(db).ExecCommand(0, "", null, new Queue<string>());

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Done!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        catch (Exception ex)
                        {
                            ConsoleWriteException(ex);
                        }

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadKey();
                        Console.WriteLine();

                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option.");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
        }


        private class Exec
        {
            private readonly CerebelloEntities db;

            public Exec(CerebelloEntities db)
            {
                this.db = db;
            }

            private static string ReadCommand(Queue<string> commands)
            {
                bool empty = commands.Count == 0;
                if (empty)
                    foreach (var command in Console.ReadLine().Split(' '))
                        commands.Enqueue(command);

                string input = commands.Dequeue();
                var matchStr = Regex.Match(input, @"^\s*""");
                if (matchStr.Success)
                    while (!(matchStr = Regex.Match(input, @"^\s*""((\\""|.)*)""\s*$")).Success)
                        input += " " + commands.Dequeue();

                if (!empty)
                    Console.WriteLine(input);

                if (matchStr.Success)
                    input = matchStr.Groups[1].Value.Replace("\\\\", "\\").Replace("\\n", "\n")
                        .Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\0", "\0").Replace("\\v", "\v");

                return input;
            }

            /// <summary>
            /// Executes a command on this class using reflection.
            /// </summary>
            /// <param name="level">Level of indentation of the command.</param>
            /// <param name="prefix">Command prefix, so that if prefix is "New" and user input is "User", the command is "NewUser".</param>
            /// <param name="outType">Type that should be returned. Only used for numeric litterals.</param>
            /// <param name="commands">List of commands in the queue.</param>
            /// <returns></returns>
            public object ExecCommand(int level, string prefix, Type outType, Queue<string> commands)
            {
                Console.ForegroundColor = ConsoleColor.White;
                var input = ReadCommand(commands);

                if (input == "null")
                    return null;

                if (outType == typeof(string))
                    return input;

                if (outType != null && outType != typeof(string) && outType.IsPrimitive)
                    try { return Convert.ChangeType(input, outType); }
                    catch { }

                var command = prefix + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input);
                var method = typeof(Exec).GetMethod(command, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    var listParam = new List<object>();
                    foreach (var eachParam in method.GetParameters())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("{0}{1} ", new string(' ', (level + 1) * 4), eachParam.ParameterType.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("{0}: ", eachParam.Name);
                        listParam.Add(ExecCommand(level + 1, eachParam.Name, eachParam.ParameterType, commands));
                    }

                    return method.Invoke(this, listParam.ToArray());
                }

                if (input == "")
                    throw new Exception("Invalid input.");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("{0}{1}: ", new string(' ', (level + 1) * 4), command);
                return this.ExecCommand(level + 1, command, outType, commands);
            }

            #region Commands called via reflection
            // ReSharper disable MemberCanBePrivate.Local
            // ReSharper disable UnusedMember.Local
            // ReSharper disable UnusedParameter.Local

            public void DelUser(User user)
            {
                if (user != null && user.Secretary != null)
                    db.DeleteObject(user.Secretary);
                if (user != null && user.Administrator != null)
                    db.DeleteObject(user.Administrator);
                if (user != null && user.Doctor != null)
                    db.DeleteObject(user.Doctor);
                if (user != null) this.db.DeleteObject(user);
                db.SaveChanges();
            }
            public void DelPractice(Practice practice)
            {
                if (practice != null) this.db.DeleteObject(practice);
                db.SaveChanges();
            }
            public void SetOwner(User user)
            {
                if (user != null)
                {
                    if (user.Practice != null)
                    {
                        if (user.Practice.Owner != null) user.Practice.Owner.IsOwner = false;
                        user.Practice.Owner = user;
                    }
                    user.IsOwner = true;
                }
                db.SaveChanges();
            }
            public void DelSecretary(Secretary secretary) { DelUser(secretary.Users.Single()); }
            public void DelAdministrator(Administrator administrator) { DelUser(administrator.Users.Single()); }
            public void DelDoctor(Doctor doctor) { DelUser(doctor.Users.Single()); }
            public Secretary NewSecretary(Practice practice, string username, string password, string name)
            {
                return Firestarter.CreateSecretary(db, practice, username, password, name);
            }
            public Secretary NewSecretaryTtMilena(Practice practice) { return Firestarter.CreateSecretary_Milena(db, practice); }
            public Secretary SecretaryMilena() { return db.Secretaries.First(x => x.Users.FirstOrDefault().UserName == "milena"); }
            public Practice PracticeNew(string name, User user, string urlId) { return Firestarter.CreatePractice(db, name, user, urlId); }
            public Practice NewPractice(string name, User user, string urlId) { return Firestarter.CreatePractice(db, name, user, urlId); }
            public Practice PracticeFirst() { return db.Practices.FirstOrDefault(); }
            public Practice PracticeDrHouse() { return db.Practices.Single(x => x.UrlIdentifier == "consultoriodrhouse"); }
            public Practice PracticeByUrlId(string urlIdentifier) { return db.Practices.Single(x => x.UrlIdentifier == urlIdentifier); }
            public User UserAndre() { return db.Users.Single(x => x.UserName == "andrerpena"); }
            public User UserMiguel() { return db.Users.Single(x => x.UserName == "masbicudo"); }
            public User UserOwner(Practice practice) { return practice.Owner; }
            public Doctor DoctorById(int id) { return db.Doctors.Single(d => d.Id == id); }
            public Secretary SecretaryById(int id) { return db.Secretaries.Single(d => d.Id == id); }
            public User UserById(int id) { return db.Users.Single(d => d.Id == id); }
            public Administrator AdministratorById(int id) { return db.Administrators.Single(d => d.Id == id); }

            // ReSharper restore UnusedParameter.Local
            // ReSharper restore UnusedMember.Local
            // ReSharper restore MemberCanBePrivate.Local
            #endregion
        }

        private static void OptionP1(CerebelloEntities db)
        {
            // Create practice, contract, doctors and other things
            Console.WriteLine("Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel");
            var listDoctors = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(db);

            // Create practice, contract, doctors and other things
            Console.WriteLine("CreateSecretary_Milena");
            var milena = Firestarter.CreateSecretary_Milena(db, listDoctors[0].Users.First().Practice);

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