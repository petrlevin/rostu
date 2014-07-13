using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Registry;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils;

namespace Platform.BusinessLogic.Activity.Operations.Serialization
{
    public static class Columns
    {
        static public Column Data
        {
            get
            {
                return Reflection<SerializedEntityItem>.Property(sei => sei.Data).Name.ToColumn();
            }
        }

        static public Column IdTool
        {
            get
            {
                return Reflection<SerializedEntityItem>.Property(sei => sei.IdTool).Name.ToColumn();
            }
        }

        static public Column IdToolEntity
        {
            get
            {
                return Reflection<SerializedEntityItem>.Property(sei => sei.IdToolEntity).Name.ToColumn();
            }
        }



    }
}
