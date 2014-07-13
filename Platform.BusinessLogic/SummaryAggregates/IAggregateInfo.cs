using Platform.BusinessLogic.DbEnums;

namespace Platform.BusinessLogic.SummaryAggregates
{
    public interface IAggregateInfo
    {
        string Field { get; }
        AggregateFunction Function { get; }
    }
}
