using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.LongTermGoalProgram
{
    public class DataSetAccomplice: IEquatable<DataSetAccomplice>
    {
        public string Type { get; set; }
        public string Name { get; set; }

        #region Equals и GetHashCode для использования Union
        public bool Equals(DataSetAccomplice other)
        {
            //Check whether the compared object is null. 
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data. 
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether properties are equal. 
            return Type.Equals(other.Type) && Name.Equals(other.Name);
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null. 
            int hashAccompliceName = Name == null ? 0 : Name.GetHashCode();

            //Get hash code for the Type field. 
            int hashAccompliceType = Type.GetHashCode();

            //Calculate the hash code. 
            return hashAccompliceName ^ hashAccompliceType;
        }
        #endregion

    }
}
