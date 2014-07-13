using System;
using System.Collections.Generic;
using System.Linq;
using Platform.BusinessLogic.Reference;

namespace Platform.BusinessLogic.DataAccess
{
    public class ResponseOperations
    {
        /// <summary>
        /// Button header. Will be shown on a splitbutton
        /// </summary>
        public String caption;

        /// <summary>
        /// List of operations to form the menu
        /// </summary>
        public List<ResponseOperation> list;

        public ResponseOperations(string caption, IQueryable<EntityOperation> operations)
        {
            this.caption = caption;
            list =operations.Select(s => new ResponseOperation{
                                                  id = s.Id,
                                                  name = s.Operation.Name,
												  text = s.Operation != null ? s.Operation.Caption : " ",
                                                  IsAtomic = !s.EditableFields.Any()
                                              }).ToList();

        }
    }
}