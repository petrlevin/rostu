using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="operation"></param>
    /// <param name="itemId"></param>
    public delegate void CreateUpdateHandler(object sender,CreateUpdateOperation operation , int itemId);

}
