using System;

namespace BaseApp.Common.Interfaces.DbDependecy
{

    public interface IResultDbDependence
    {
        string HeadTypeName { get; set; }
        string HeadEntityCaption { get; set; }
        Int32 HeadId { get; set; }
        string HeadCaption { get; set; }

        string TypeName { get; set; }
        string EntityCaption { get; set; }
        Int32 Id { get; set; }
        string Caption { get; set; }
    }
}
