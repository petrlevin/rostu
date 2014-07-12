using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;

namespace Sbor.Registry
{
    public partial class LimitVolumeAppropriations
    {
        [Control(ControlType.Insert, Sequence.Before)]
        public void AutoInit(DataContext context)
        {
            DateCreate = DateTime.Now;
        }

        /// <summary>
        /// Создать копию без подтягивания связей
        /// </summary>
        /// <returns></returns>
        public LimitVolumeAppropriations Clone()
        {
            return new LimitVolumeAppropriations()
                {
                    IdBudget = IdBudget,
                    IdAuthorityOfExpenseObligation = IdAuthorityOfExpenseObligation,
                    IdEstimatedLine = IdEstimatedLine,
                    IdHierarchyPeriod = IdHierarchyPeriod,
                    IdOKATO = IdOKATO,
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdRegistrator = IdRegistrator,
                    IdRegistratorEntity = IdRegistratorEntity,
                    IdTaskCollection = IdTaskCollection,
                    IdValueType = IdValueType,
                    IdVersion = IdVersion,

                    Value = Value
                };
        }

        /*public LimitVolumeAppropriations(LimitVolumeAppropriations other)
        {
            IdBudget = other.IdBudget;
            IdAuthorityOfExpenseObligation = other.IdAuthorityOfExpenseObligation;
            IdEstimatedLine = other.IdEstimatedLine;
            IdHierarchyPeriod = other.IdHierarchyPeriod;
            IdOKATO = other.IdOKATO;
            IdPublicLegalFormation = other.IdPublicLegalFormation;
            IdRegistrator = other.IdRegistrator;
            IdRegistratorEntity = other.IdRegistratorEntity;
            IdTaskCollection = other.IdTaskCollection;
            IdValueType = other.IdValueType;
            IdVersion = other.IdVersion;

            Value = other.Value;
        }*/
    }
}
