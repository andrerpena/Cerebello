using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cerebello.Firestarter.Helpers;
using Cerebello.Model;
using CerebelloWebRole.Code;

namespace Cerebello.Firestarter
{
    // ReSharper disable LocalizableElement
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().Run();
        }

        private bool isFuncBackupEnabled;

        private string connName;

        private readonly string rootCerebelloPath = ConfigurationManager.AppSettings["CerebelloPath"];

        const int defaultSeed = 101; // default random seed
        private int? rndOption = defaultSeed;

        bool detachDbWhenDone;

        private void Run()
        {
            if (string.IsNullOrEmpty(this.rootCerebelloPath))
                throw new Exception("Cannot start FireStarter. Cannot find Cerebello root path configuration");

            bool isToChooseDb = true;

            bool showHidden = false;

            Console.BufferWidth = 200;

            var lastVer = File.Exists("last.ver") ? File.ReadAllText("last.ver") : null;
            var currVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            File.WriteAllText("last.ver", currVer);

            this.isFuncBackupEnabled = File.Exists("isFuncBackupEnabled");

            // New options:
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Firestarter v{0}", currVer);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (lastVer != currVer)
                    Console.Write(" last used v{0}", lastVer ?? "?.?.?.?");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("DateTime (UTC):   {0}", DateTime.UtcNow);
                Console.WriteLine("DateTime (Local): {0}", DateTime.Now);
                Console.WriteLine("DateTime (?):     {0}", DateTime.Now.ToUniversalTime());
                Console.WriteLine("CurrentDir:       {0}", Environment.CurrentDirectory);

                while (isToChooseDb)
                {
                    if (this.detachDbWhenDone)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("Detaching DB... ");

                        bool ok = false;
                        using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                            try
                            {
                                Firestarter.DetachLocalDatabase(db);
                                ok = true;
                            }
                            catch
                            {
                            }


                        if (ok)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("DONE");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;

                    // Getting available connections from ConfigurationManager so that user can choose one.
                    Console.WriteLine("Choose connection to use:");

                    var connStr = new Dictionary<string, string>();
                    for (int idxConnStr = 0; idxConnStr < ConfigurationManager.ConnectionStrings.Count; idxConnStr++)
                    {
                        var connStrSettings = ConfigurationManager.ConnectionStrings[idxConnStr];
                        connStr[idxConnStr.ToString(CultureInfo.InvariantCulture)] = connStrSettings.Name;
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
                    if (!int.TryParse(userOption, out idx) || !connStr.TryGetValue(
                        idx.ToString(CultureInfo.InvariantCulture),
                        out this.connName))
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

                    if (string.IsNullOrWhiteSpace(this.connName))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Cannot connect because the connection string is empty.");
                        Console.WriteLine();
                        continue;
                    }

                    try
                    {
                        using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                        {
                            var attachResult = Firestarter.AttachLocalDatabase(db);
                            if (attachResult == Firestarter.AttachLocalDatabaseResult.NotFound)
                            {
                                // Create the DB if it does not exist.
                                Console.Write("Would you like to create the database (y/n): ");
                                if (Console.ReadKey().KeyChar == 'y')
                                {
                                    Console.WriteLine();
                                    var result = Firestarter.CreateDatabaseIfNeeded(db);
                                    if (!result)
                                    {
                                        Console.WriteLine("Could not create database.");
                                        continue;
                                    }

                                    bool isTestDb = this.connName.ToUpper().Contains("TEST");
                                    this.detachDbWhenDone = isTestDb;
                                }
                                else
                                    continue;
                            }
                            else
                            {
                                this.detachDbWhenDone = attachResult == Firestarter.AttachLocalDatabaseResult.Ok;
                            }

                            if (this.detachDbWhenDone)
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
                Console.WriteLine("Current DB: {0}", this.connName);
                Console.WriteLine("Project Path: {0}", this.rootCerebelloPath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"r      - Magic option: reset database (differentiates WORK and TEST database).");
                Console.WriteLine(@"db     - Change database.");

                if (showHidden)
                {
                    Console.WriteLine();
                    Console.WriteLine(@"clr    - Clear all data.");
                    Console.WriteLine(@"p1     - Populate database with items (type p1? to know more).");
                    Console.WriteLine(@"drp    - Drop all tables and FKs.");
                    Console.WriteLine(@"crt    - Create all tables and FKs using script.");
                    Console.WriteLine();
                    Console.WriteLine(@"anvll  - Downloads all leaflets from Anvisa site.");
                    Console.WriteLine(@"rnd    - Set the seed to the random generator.");
                    Console.WriteLine();
                    Console.WriteLine(@"bkc    - Create database backup.");
                    Console.WriteLine(@"bkr    - Restore database backup.");
                    Console.Write(@"abk    - Enables or disables functional backups. (");
                    Console.ForegroundColor = isFuncBackupEnabled ? ConsoleColor.DarkGreen : ConsoleColor.Gray;
                    Console.Write(isFuncBackupEnabled ? "enabled" : "disabled");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(@")");

                    Console.WriteLine();
                    using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                    {
                        if (Firestarter.BackupExists(db, "__zero__"))
                            Console.WriteLine(@"zero   - Restores DB to last zeroed state.");
                        if (Firestarter.BackupExists(db, "__undo__"))
                            Console.WriteLine(@"undo   - Undoes the last operation (if possible).");
                        if (Firestarter.BackupExists(db, "__redo__"))
                            Console.WriteLine(@"redo   - Redoes something that was undone.");
                        if (Firestarter.BackupExists(db, "__reset__"))
                            Console.WriteLine(@"reset  - Reset DB to initial set (differentiates WORK and TEST).");
                    }
                    Console.WriteLine();
                    Console.WriteLine(
                        this.detachDbWhenDone
                            ? @"atc    - Leaves DB attached when done."
                            : @"dtc    - Detach DB when done.");
                    Console.WriteLine();
                    Console.WriteLine(@"cls    - Clear screen.");
                }

                Console.WriteLine(@"RETURN - {0} options.", showHidden ? "Hides visible" : "Shows hidden");

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

                    case "abk?": InfoAbk(); break;
                    case "abk": this.OptAbk(); break;

                    case "undo?": InfoUndo(); break;
                    case "undo": this.OptUndo(); break;

                    case "zero?": InfoZero(); break;
                    case "zero": this.OptZero(); break;

                    case "redo?": InfoRedo(); break;
                    case "redo": this.OptRedo(); break;

                    case "reset?": InfoReset(); break;
                    case "reset": this.OptReset(); break;

                    case "atc?": InfoAtc(); break;
                    case "atc": this.OptAtc(); break;

                    case "dtc?": InfoDtc(); break;
                    case "dtc": this.OptDtc(); break;

                    case "crt?": InfoCrt(); break;
                    case "crt": this.OptCrt(); break;

                    case "size?": InfoSize(); break;
                    case "size": this.OptSize(); break;

                    case "drp?": InfoDrp(); break;
                    case "drp": this.OptDrp(); continue;

                    case "cls?": InfoCls(); break;
                    case "cls": OptCls(); break;

                    case "clr?": InfoClr(); break;
                    case "clr": this.OptClr(); continue;

                    case "p1?": InfoP1(); break;
                    case "p1": this.OptP1(); break;

                    case "anvll?": InfoAnvll(); break;
                    case "anvll": this.OptAnvll(); break;

                    case "q?": InfoQ(); break;
                    case "q": this.OptQ(); return;

                    case "db?": InfoDb(); break;
                    case "db": isToChooseDb = OptDb(); break;

                    case "rnd?": InfoRnd(); break;
                    case "rnd": this.OptRnd(); break;

                    case "bkc?": InfoBkc(); break;
                    case "bkc": this.OptBkc(); break;

                    case "bkr?": InfoBkr(); break;
                    case "bkr": this.OptBkr(); break;

                    case "r?": InfoR(); break;
                    case "r": this.OptR(); continue;

                    case "exec": this.OptExec(); break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option.");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            }
        }

        private static void InfoAbk()
        {
        }

        private void OptAbk()
        {
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("e: enabled; d: disabled; anything else: only show status");
                    Console.Write("Type option: ");
                    var key = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (key == 'e' || key == 'd')
                    {
                        this.isFuncBackupEnabled = key == 'e';

                        if (this.isFuncBackupEnabled)
                            File.Create("isFuncBackupEnabled").Close();
                        else
                            File.Delete("isFuncBackupEnabled");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriteException(ex);
                }
                finally
                {
                    this.isFuncBackupEnabled = File.Exists("isFuncBackupEnabled");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Functional backups is ");
                Console.ForegroundColor = this.isFuncBackupEnabled ? ConsoleColor.DarkGreen : ConsoleColor.Gray;
                Console.WriteLine(this.isFuncBackupEnabled ? "enabled" : "disabled");
                Console.ForegroundColor = ConsoleColor.White;

                PressAnyKeyToContinue();
            }
        }

        private static void PressAnyKeyToContinue()
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to continue.");
            Console.ForegroundColor = Console.BackgroundColor;
            Console.ReadKey(true);
            Console.WriteLine();
            Console.ForegroundColor = prevColor;
        }

        private static void InfoUndo()
        {
        }

        private void OptUndo()
        {
            using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
            {
                if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__redo__");
                var fileName = Firestarter.RestoreBackup(db, "__undo__");
                if (fileName != null)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("File name: {0}", fileName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Undone!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Could not undo!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private static void InfoZero()
        {
        }

        private void OptZero()
        {
            using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
            {
                if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                var fileName = Firestarter.RestoreBackup(db, "__zero__");
                if (fileName != null)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("File name: {0}", fileName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Zeroed!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Could not zero database!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private static void InfoRedo()
        {
        }

        private void OptRedo()
        {
            using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
            {
                var fileName = Firestarter.RestoreBackup(db, "__redo__");
                if (fileName != null)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("File name: {0}", fileName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Could not redo!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private static void InfoReset()
        {
        }

        private void OptReset()
        {
            using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
            {
                if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                var fileName = Firestarter.RestoreBackup(db, "__reset__");
                if (fileName != null)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("File name: {0}", fileName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Could not reset database!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private static void InfoAtc()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("When you are done with the current DB, it will be left attached.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptAtc()
        {
            this.detachDbWhenDone = false;
        }

        private static void InfoDtc()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("When you are done with the current DB, it is going to be detached.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptDtc()
        {
            this.detachDbWhenDone = true;
        }

        private static void InfoCrt()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine(@"Loads the file '\DB\Scripts\script.sql',");
                Console.WriteLine("Changes the collation of columns in the script to 'Latin1_General_CI_AI',");
                Console.WriteLine("and then executes the changed script.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptCrt()
        {
            {
                try
                {
                    using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                    {
                        if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                        this.CreateDatabaseUsingScript(db);
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (Exception ex)
                {
                    ConsoleWriteException(ex);
                }
            }
        }

        private static void InfoSize()
        {
        }

        private void OptSize()
        {
            using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
            {
                Console.WriteLine(
                    "SYS_MedicalEntity: Code.Length: min={0}; max={1}",
                    db.SYS_MedicalEntity.Min(x => x.Code.Length),
                    db.SYS_MedicalEntity.Max(x => x.Code.Length));
                Console.WriteLine(
                    "SYS_MedicalEntity: Name.Length: min={0}; max={1}",
                    db.SYS_MedicalEntity.Min(x => x.Name.Length),
                    db.SYS_MedicalEntity.Max(x => x.Name.Length));
                Console.WriteLine(
                    "SYS_MedicalProcedure: Code.Length: min={0}; max={1}",
                    db.SYS_MedicalProcedure.Min(x => x.Code.Length),
                    db.SYS_MedicalProcedure.Max(x => x.Code.Length));
                Console.WriteLine(
                    "SYS_MedicalProcedure: Name.Length: min={0}; max={1}",
                    db.SYS_MedicalProcedure.Min(x => x.Name.Length),
                    db.SYS_MedicalProcedure.Max(x => x.Name.Length));
                Console.WriteLine(
                    "SYS_MedicalSpecialty: Code.Length: min={0}; max={1}",
                    db.SYS_MedicalSpecialty.Min(x => x.Code.Length),
                    db.SYS_MedicalSpecialty.Max(x => x.Code.Length));
                Console.WriteLine(
                    "SYS_MedicalSpecialty: Name.Length: min={0}; max={1}",
                    db.SYS_MedicalSpecialty.Min(x => x.Name.Length),
                    db.SYS_MedicalSpecialty.Max(x => x.Name.Length));
            }
        }

        private static void InfoDrp()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Methods that will be called:");
                Console.WriteLine("    DropAllTables");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptDrp()
        {
            {
                Console.Write("This will drop EVERY table in your DB... are you sure? (y/n): ");
                var userDropAll = Console.ReadLine();
                if (userDropAll != "y" && userDropAll != "n")
                    return;
                bool clearAllData = userDropAll == "y";

                // Doing what the user has told.
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    if (clearAllData)
                    {
                        if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
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
        }

        private static void InfoCls()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Clears the output from all previous commands.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private static void OptCls()
        {
            Console.Clear();
        }

        private static void InfoClr()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Methods that will be called:");
                Console.WriteLine("    ClearAllData");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptClr()
        {
            {
                Console.Write("Clear all data (y/n): ");
                var userClearAll = Console.ReadLine();
                if (userClearAll != "y" && userClearAll != "n")
                    return;
                bool clearAllData = userClearAll == "y";

                // Doing what the user has told.
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    if (clearAllData)
                    {
                        if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                        try
                        {
                            Firestarter.ClearAllData(db);
                        }
                        catch (Exception ex)
                        {
                            ConsoleWriteException(ex);
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
        }

        private static void InfoP1()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Methods that will be called:");
                Console.WriteLine("    Initialize_SYS_MedicalEntity");
                Console.WriteLine("    Initialize_SYS_MedicalSpecialty");
                Console.WriteLine("    Initialize_SYS_Contracts");
                Console.WriteLine("    Initialize_SYS_Cid10");
                Console.WriteLine("    Initialize_SYS_MedicalProcedures");
                Console.WriteLine("    Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel_Thomas");
                Console.WriteLine("    CreateSecretary_Milena");
                Console.WriteLine("    SetupDoctor (for each doctor)");
                Console.WriteLine("    CreateFakePatients (for each doctor)");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptP1()
        {
            try
            {
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");

                    Console.ForegroundColor = ConsoleColor.Gray;

                    // Initializing system tables
                    this.InitSysTables(db);

                    using (RandomContext.Create(this.rndOption))
                        OptionP1(db);

                    if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__reset__");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Partially done!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void InfoAnvll()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Downloads and saves all leaflets from Anvisa official site.");
                Console.WriteLine("A JSON file is going to be saved with all data.");
                Console.WriteLine("This file is used to populate DB later.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptAnvll()
        {
            try
            {
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Saving to: {0}", new FileInfo("medicines.json").FullName);

                    // Downloading data from Anvisa official site.
                    var anvisaHelper = new AnvisaLeafletHelper();
                    var meds = anvisaHelper.DownloadAndCreateMedicinesJson();

                    Console.WriteLine("Total medicines: {0}", meds.Count);
                    Console.WriteLine("Saved to: {0}", new FileInfo("medicines.json").FullName);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done!");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Partially done!");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void InfoQ()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Quits the program.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptQ()
        {
            // Dettaching previous DB if it was attached in this session.
            if (this.detachDbWhenDone)
            {
                bool ok = false;
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                    try
                    {
                        Firestarter.DetachLocalDatabase(db);
                        ok = true;
                    }
                    catch
                    {
                    }

                Console.WriteLine();
                if (ok)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("DB detached.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DB NOT detached.");
                    PressAnyKeyToContinue();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bye!");
        }

        private static void InfoDb()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Changes the current database.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private static bool OptDb()
        {
            bool isToChooseDb;
            isToChooseDb = true;
            Console.WriteLine();

            return isToChooseDb;
        }

        private static void InfoRnd()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Sets a new seed to the random generator.");
                Console.WriteLine("If left empty, uses an unpredictable seed.");
                Console.Write("Default seed is ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(defaultSeed);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine('.');
                Console.WriteLine("A predictable seed will always produce the same set of results,");
                Console.WriteLine("suitable for unit tests, and an unpredictable seed will produce");
                Console.WriteLine("different sets of result each time it is run, being suitable");
                Console.WriteLine("for human interaction tests.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptRnd()
        {
            // Dettaching previous DB if it was attached in this session.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Seed: ");
            var numText = Console.ReadLine();
            this.rndOption = null;
            int num;
            if (int.TryParse(numText, out num))
                this.rndOption = num;
        }

        private static void InfoBkc()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Creates a database backup,");
                Console.WriteLine("that can latter be restored");
                Console.WriteLine("using the 'bkr' command.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptBkc()
        {
            // Dettaching previous DB if it was attached in this session.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Backup name: ");
            var ssName = Console.ReadLine();
            string fileName = null;
            try
            {
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                    if (this.isFuncBackupEnabled) fileName = Firestarter.CreateBackup(db, ssName);
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            if (fileName != null)
                Console.WriteLine("File name: {0}", fileName);

            Console.ForegroundColor = fileName != null ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(fileName != null ? "Done!" : "File not found");

            Console.ForegroundColor = ConsoleColor.White;
            PressAnyKeyToContinue();
        }

        private static void InfoBkr()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Restores a database backup,");
                Console.WriteLine("that was created using the");
                Console.WriteLine("'bkc' command.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptBkr()
        {
            // Dettaching previous DB if it was attached in this session.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Backup name: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            var ssNameRestore = Console.ReadLine();
            string fileName = null;
            bool fileExists;
            try
            {
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    fileExists = Firestarter.BackupExists(db, ssNameRestore);
                    if (fileExists)
                        fileName = Firestarter.RestoreBackup(db, ssNameRestore);
                }
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex);
                return;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            if (fileName != null)
            {
                Console.WriteLine("File name: {0}", fileName);

                Console.ForegroundColor = fileExists ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(fileExists ? "Done!" : "File not found");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed!");
            }

            Console.ForegroundColor = ConsoleColor.White;
            PressAnyKeyToContinue();
        }

        private static void InfoR()
        {
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine("Resets the current database, to the default working state.");
                Console.WriteLine("Work database is dropped, recreated, and populated with p1 option.");
                Console.WriteLine("Test database is dropped, recreated, and populated with all SYS tables.");
                Console.WriteLine();
                PressAnyKeyToContinue();
            }
        }

        private void OptR()
        {
            {
                bool isTestDb = this.connName.ToUpper().Contains("TEST");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("DB type: {0}", isTestDb ? "TEST" : "WORK");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Reset the database (y/n): ");
                var userReset = Console.ReadLine();
                if (userReset != "y" && userReset != "n")
                    return;
                bool resetDb = userReset == "y";

                Console.ForegroundColor = ConsoleColor.Gray;

                // Doing what the user has told.
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    if (resetDb)
                    {
                        if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                        try
                        {
                            Console.WriteLine("DropAllTables");
                            Firestarter.DropAllTables(db);

                            Console.WriteLine("CreateDatabaseUsingScript");
                            this.CreateDatabaseUsingScript(db);

                            this.InitSysTables(db);

                            if (!isTestDb)
                                using (RandomContext.Create(this.rndOption))
                                    OptionP1(db);

                            if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__reset__");
                        }
                        catch (Exception ex)
                        {
                            ConsoleWriteException(ex);

                            Console.WriteLine("Use 'undo' command to restore database to what it was before.");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Partially done!");
                            Console.ForegroundColor = ConsoleColor.White;

                            return;
                        }

                        Console.WriteLine("Use 'zero' command to restore database to a minimal state.");
                        Console.WriteLine("Use 'undo' command to restore database to what it was before.");
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
        }

        private void OptExec()
        {
            try
            {
                Console.Write("Command: ");
                using (var db = new CerebelloEntities(string.Format("name={0}", this.connName)))
                {
                    if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__undo__");
                    new Exec(db).ExecCommand(0, "", null, new Queue<string>());
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex);
            }

            PressAnyKeyToContinue();
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
                    try
                    {
                        return Convert.ChangeType(input, outType);
                    }
                    catch
                    {
                    }

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
                        listParam.Add(this.ExecCommand(level + 1, eachParam.Name, eachParam.ParameterType, commands));
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
                    this.db.DeleteObject(user.Secretary);
                if (user != null && user.Administrator != null)
                    this.db.DeleteObject(user.Administrator);
                if (user != null && user.Doctor != null)
                    this.db.DeleteObject(user.Doctor);
                if (user != null) this.db.DeleteObject(user);
                this.db.SaveChanges();
            }

            public void DelPractice(Practice practice)
            {
                if (practice != null) this.db.DeleteObject(practice);
                this.db.SaveChanges();
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
                this.db.SaveChanges();
            }

            public void DelSecretary(Secretary secretary)
            {
                this.DelUser(secretary.Users.Single());
            }

            public void DelAdministrator(Administrator administrator)
            {
                this.DelUser(administrator.Users.Single());
            }

            public void DelDoctor(Doctor doctor)
            {
                this.DelUser(doctor.Users.Single());
            }

            public Secretary NewSecretary(Practice practice, string username, string password, string name)
            {
                var user = Firestarter.CreateUser(this.db, practice, username, password, name);
                user.Secretary = new Secretary { PracticeId = user.PracticeId };
                return user.Secretary;
            }

            public Secretary NewSecretaryTtMilena(Practice practice)
            {
                return Firestarter.CreateSecretary_Milena(this.db, practice);
            }

            public Secretary SecretaryMilena()
            {
                return this.db.Secretaries.First(x => x.Users.FirstOrDefault().UserName == "milena");
            }

            public Practice PracticeNew(string name, User user, string urlId)
            {
                return Firestarter.CreatePractice(this.db, name, user, urlId);
            }

            public Practice NewPractice(string name, User user, string urlId)
            {
                return Firestarter.CreatePractice(this.db, name, user, urlId);
            }

            public Practice PracticeFirst()
            {
                return this.db.Practices.FirstOrDefault();
            }

            public Practice PracticeDrHouse()
            {
                return this.db.Practices.Single(x => x.UrlIdentifier == "consultoriodrhouse");
            }

            public Practice PracticeByUrlId(string urlIdentifier)
            {
                return this.db.Practices.Single(x => x.UrlIdentifier == urlIdentifier);
            }

            public User UserAndre()
            {
                return this.db.Users.Single(x => x.UserName == "andrerpena");
            }

            public User UserMiguel()
            {
                return this.db.Users.Single(x => x.UserName == "masbicudo");
            }

            public User UserOwner(Practice practice)
            {
                return practice.Owner;
            }

            public Doctor DoctorById(int id)
            {
                return this.db.Doctors.Single(d => d.Id == id);
            }

            public Secretary SecretaryById(int id)
            {
                return this.db.Secretaries.Single(d => d.Id == id);
            }

            public User UserById(int id)
            {
                return this.db.Users.Single(d => d.Id == id);
            }

            public User UserNew(string username, string password, string name, string type)
            {
                var user = Firestarter.CreateUser(this.db, null, username, password, name);
                if (type.Contains("adm"))
                    user.Administrator = new Administrator { PracticeId = user.PracticeId };
                if (type.Contains("sec"))
                    user.Secretary = new Secretary { PracticeId = user.PracticeId };
                if (type.Contains("doc"))
                {
                    user.Doctor = new Doctor { PracticeId = user.PracticeId };
                    user.Doctor.HealthInsurances.Add(new HealthInsurance
                        {
                            PracticeId = user.PracticeId,
                            Name = "Particular",
                            IsActive = true,
                            IsParticular = true,
                        });
                }
                return user;
            }

            public Administrator AdministratorById(int id)
            {
                return this.db.Administrators.Single(d => d.Id == id);
            }

            public void Pwd(string password)
            {
                var passwordSalt = CipherHelper.GenerateSalt();
                var passwordHash = CipherHelper.Hash(password, passwordSalt);
                Console.WriteLine();
                Console.WriteLine(@"PasswordSalt = ""{0}"",", passwordSalt);
                Console.WriteLine(@"Password = ""{0}"", // pwd: '{1}'", passwordHash, password);
                Console.WriteLine();
            }

            // ReSharper restore UnusedParameter.Local
            // ReSharper restore UnusedMember.Local
            // ReSharper restore MemberCanBePrivate.Local

            #endregion
        }

        private static void OptionP1(CerebelloEntities db)
        {
            // Create practice, contract, doctors and other things
            Console.WriteLine("Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel_Thomas");
            var doctorsList = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel_Thomas(db);

            // Create practice, contract, doctors and other things
            Console.WriteLine("CreateSecretary_Milena");
            Firestarter.CreateSecretary_Milena(db, doctorsList[0].Users.First().Practice);

            // Setup doctor schedule and document templates
            Console.WriteLine("SetupDoctor");
            using (var rc = RandomContext.Create())
                foreach (var doctor in doctorsList)
                    Firestarter.SetupDoctor(doctor, db, rc.Random.Next());

            // Create patients
            Console.WriteLine("CreateFakePatients");
            using (RandomContext.Create())
                foreach (var doctor in doctorsList)
                    Firestarter.CreateFakePatients(doctor, db);

            // Create appointments
            Console.WriteLine("CreateFakeAppointments");
            using (var rc = RandomContext.Create())
                foreach (var doctor in doctorsList)
                    Firestarter.CreateFakeAppointments(db, doctor, rc.Random.Next());
        }

        private void InitSysTables(CerebelloEntities db)
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
                progress: ConsoleWriteProgressIntInt);

            Console.WriteLine("Initialize_SYS_MedicalProcedures");
            Firestarter.Initialize_SYS_MedicalProcedures(
                db,
                Path.Combine(this.rootCerebelloPath, @"DB\cbhpm_2010.txt"),
                progress: ConsoleWriteProgressIntInt);

            Console.WriteLine("SaveLeafletsInMedicinesJsonToDb");
            var anvisaHelper = new AnvisaLeafletHelper();
            anvisaHelper.SaveLeafletsInMedicinesJsonToDb(
                db,
                progress: ConsoleWriteProgressIntInt);

            // Creating a minimal DB backup called __zero__.
            if (this.isFuncBackupEnabled) Firestarter.CreateBackup(db, "__zero__");
        }

        private void CreateDatabaseUsingScript(CerebelloEntities db)
        {
            // ToDo: figure out a way to remove this.. we should have a common path or something
            var path = Path.Combine(this.rootCerebelloPath, @"DB\Scripts");
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
                Console.Write("Exception: {0}", ex1.GetType().Name);
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
                        Console.Write("{0:0000}:", fncLevel++);
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
                Console.WriteLine("    {0} of {1} - {2}%", x, y, x * 100 / y);
            Console.ForegroundColor = oldColor;
        }
    }

    // ReSharper restore LocalizableElement
}
