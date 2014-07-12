using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Core.DeleteTestData
{
    public class DistributivaDataInfo
    {
        protected bool Equals(DistributivaDataInfo other)
        {
            return IdElement == other.IdElement && IdElementEntity == other.IdElementEntity;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IdElement*397) ^ IdElementEntity;
            }
        }

        public Int32 IdElement { get; set; }
        public Int32 IdElementEntity { get; set; }



        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o)) return false;
            if (ReferenceEquals(this, o)) return true;
            if (o.GetType() != this.GetType()) return false;
            return Equals((DistributivaDataInfo) o);
        }
    }
}
