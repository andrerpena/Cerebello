using Microsoft.AspNet.SignalR;

namespace CerebelloWebRole.Code
{
    public class CerebelloHub : Hub
    {
        protected readonly CerebelloEntities db;

        public CerebelloHub()
        {
            this.db = this.CreateNewCerebelloEntities();
        }

        public virtual CerebelloEntities CreateNewCerebelloEntities()
        {
            return new CerebelloEntities();
        }
    }
}
