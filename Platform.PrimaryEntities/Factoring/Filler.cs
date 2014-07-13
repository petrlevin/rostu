using System.Data;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    public class Filler : IFiller<DataRow>
    {
        public void Fill(IBaseEntity objectToFill, DataRow data)
        {
            objectToFill.FromDataRow(data);
        }
        
    }
}
