using System;
using System.Data;
using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    public class CerebelloEntities : CerebelloEntitiesBase
    {
        /// <summary>
        /// Initialize a new CerebelloEntities object.
        /// </summary>
        public CerebelloEntities(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Initialize a new CerebelloEntities object.
        /// </summary>
        public CerebelloEntities()
            : base(DebugConfig.DataBaseConnectionString)
        {
        }

        public override int SaveChanges(System.Data.Objects.SaveOptions options)
        {
            // checking changed elements to see if there is something wrong
            foreach (var objectStateEntry in this.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified))
            {
                var obj = objectStateEntry.Entity;
                if (obj != null)
                {
                    // checking DateTime properties (they must all be UTC)
                    if (DebugConfig.IsDebug)
                    {
                        var changedProperties = objectStateEntry.GetModifiedProperties();

                        foreach (var changedProperty in changedProperties)
                        {
                            var newValue = objectStateEntry.CurrentValues[changedProperty];

                            if (newValue is DateTime)
                            {
                                var value = (DateTime)newValue;
                                if (value.Kind != DateTimeKind.Utc)
                                    throw new Exception("Invalid value for 'DateTime' property: " + changedProperty + ".");
                            }
                        }
                    }
                }
            }

            return base.SaveChanges(options);
        }
    }
}
