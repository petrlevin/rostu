using BaseApp.Common.Interfaces;
using BaseApp.Tablepart;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Rights.Organizational
{
    /// <summary>
    /// 
    /// </summary>
    internal   class OrganizationRightInfo : IOrganizationRightInfo
    {
        public Role_OrganizationRight RoleOrganizationRight { get; set; }
    


        public OrganizationRightInfo()
        {
        }

        public IEntityField Field { get;set;}


        public IEntityField ParentField
        {
            get { return RoleOrganizationRight.ParentField; }
        }

        public int? IdElement
        {
            get {return  RoleOrganizationRight.IdElement; }
        }

        public int IdElementEntity
        {
            get { return  RoleOrganizationRight.IdElementEntity; }
        }

        public IEntity ElementEntity
        {
            get { return RoleOrganizationRight.ElementEntity; }
        }



        public override bool Equals(object obj)
        {
            var other = obj as IOrganizationRightInfo;
            if (other==null)
                return false;
            if ((ParentField == null) && (other.ParentField != null))
                return false;
            if ((ParentField != null) && (other.ParentField == null))
                return false;

            var result = (Field.Id == other.Field.Id) &&

                   (IdElementEntity == other.IdElementEntity) &&
                   (IdElement == other.IdElement) &&
                   (ParentField.Id != null
                       ? (ParentField.Id == other.ParentField.Id)
                       : true);
            return result;

        }


        public override int GetHashCode()
        {
            return Field.Id.GetHashCode();
        }
    }
}
