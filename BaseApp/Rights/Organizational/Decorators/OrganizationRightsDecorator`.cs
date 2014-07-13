using System;
using System.Collections.Generic;
using System.Linq;
using BaseApp.Common.Interfaces;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.Decorators.Abstract;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.Rights.Organizational.Decorators
{
    /// <summary>
    /// Базовый декоратор добавляющий для работы  с организационными правами
    /// </summary>
    public  class OrganizationRightsDecorator<TImplementor> : SelectDecoratorBase<TImplementor> where TImplementor : Implementation, new()
    {
        private readonly IOrganizationRightData _rightsData;


        protected IOrganizationRightData RightsData
        {
            get { return _rightsData; }
        }

        protected override void InitImplementor(TImplementor implementor)
        {
            implementor.RightsData = RightsData;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightsData"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public OrganizationRightsDecorator(IOrganizationRightData rightsData)
        {
            if (rightsData == null) throw new ArgumentNullException("rightsData");
            _rightsData = rightsData;
        }




    }
}
