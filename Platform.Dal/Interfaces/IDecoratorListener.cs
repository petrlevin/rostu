using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators.Abstract;

namespace Platform.Dal.Interfaces
{
    public interface IDecoratorListener
    {
        void OnDecorated(TSqlStatementDecorator sender, EventDatas eventDatas);
    }
}
