using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.CaptionExpressions.Extensions
{
    public static class EntityFieldExtensions
    {
        public static string GetEvaluatedCaption(this IEntityField entityField)
        {
            return CaptionEvaluator.CalculateCaptionExpression(entityField.Caption);
        }
    }
}
