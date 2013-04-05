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
    }
}
