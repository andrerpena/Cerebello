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
            using (var db = new CerebelloEntities())
            {
                db.ExecuteStoreCommand(@"EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
                db.ExecuteStoreCommand(@"sp_MSForEachTable '
                    IF OBJECTPROPERTY(object_id(''?''), ''TableHasForeignRef'') = 1
                    DELETE FROM ?
                    else 
                    TRUNCATE TABLE ?
                '");
                db.ExecuteStoreCommand(@"sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
                db.ExecuteStoreCommand(@"sp_MSForEachTable ' 
                    IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 
                    DBCC CHECKIDENT (''?'', RESEED, 0) 
                ' ");

                Firestarter.SetupDB(db);

                var listDoctors = Firestarter.CreateFakeUserAndPractice(db);

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
        }
    }
}