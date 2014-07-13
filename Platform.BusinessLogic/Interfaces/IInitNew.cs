using System.Data.Entity;

namespace Platform.BusinessLogic.Interfaces
{
    interface IInitNew
    {
        void Init(DbContext dbContext);
    }
}
