using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cerebello.Model;
using CerebelloWebRole.Models;
using Cerebello.Firestarter;

namespace Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new CerebelloEntities("name=CerebelloEntitiesAzure"))
            {
                Firestarter.SetupDB(db);
                var doctor = Firestarter.CreateFakeUserAndPractice(db);
                db.SaveChanges();

                Firestarter.SetupDoctor(doctor, db);
                db.SaveChanges();

                Firestarter.SetupUserData(doctor, db);
                db.SaveChanges();

                Firestarter.CreateFakePatients(doctor, db);
                db.SaveChanges();
            }
        }
    }
}