using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Models;

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

            Dictionary<string, string> connStr = new Dictionary<string, string>();
            for (int idxConnStr = 0; idxConnStr < ConfigurationManager.ConnectionStrings.Count; idxConnStr++)
            {
                var connStrSettings = ConfigurationManager.ConnectionStrings[idxConnStr];
                connStr[idxConnStr.ToString()] = connStrSettings.Name;
                Console.WriteLine(string.Format("{0}: {1}", idxConnStr, connStrSettings.Name));
            }

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
                Console.WriteLine("nu  - Create new user.");
                Console.WriteLine("cls - Clear all data.");
                Console.WriteLine("p1  - Populate setup 1.");
                Console.WriteLine("q   - Quit.");
                Console.Write("What would you like to do with the DB: ");
                userOption = Console.ReadLine();

                switch (userOption.Trim().ToLowerInvariant())
                {
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

                                Console.WriteLine(string.Format("PasswordSalt = {0};", user.PasswordSalt));
                                Console.WriteLine(string.Format("Password = {0};", user.Password));
                                Console.ReadKey();
                            }
                        }

                        break;

                    case "cls":
                        Console.Write("Clear all data (y/n): ");
                        var userClearAll = Console.ReadLine();
                        if (userClearAll != "y" && userClearAll != "n")
                            return;
                        bool clearAllData = userClearAll == "y";

                        // Doing what the user has told.
                        using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                        {
                            if (clearAllData)
                                Firestarter.ClearAllData(db);
                        }

                        break;

                    case "p1":
                        using (var db = new CerebelloEntities(string.Format("name={0}", connName)))
                        {
                            Firestarter.InitializeDatabaseWithSystemData(db);

                            var listDoctors = Firestarter.CreateFakeUserAndPractice_2(db);

                            db.SaveChanges();

                            foreach (var doctor in listDoctors)
                                Firestarter.SetupDoctor(doctor, db);

                            db.SaveChanges();

                            foreach (var doctor in listDoctors)
                                Firestarter.SetupUserData(doctor, db);

                            db.SaveChanges();

                            foreach (var doctor in listDoctors)
                                Firestarter.CreateFakePatients(doctor, db);

                            db.SaveChanges();
                        }

                        break;

                    case "q":
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