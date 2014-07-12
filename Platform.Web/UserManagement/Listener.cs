using BaseApp.Environment.Storages;
using Platform.BusinessLogic.DataAccess;

namespace Platform.Web.UserManagement
{
    public class Listener : CUDListener<BaseApp.Reference.User>
    {
        private readonly SessionStorage _session;

        public Listener(SessionStorage session)
        {
            _session = session;
        }

        private void SetUser(BaseApp.Reference.User user)
        {
            if (_session.CurrentUser == null)
                return;
            if (_session.CurrentUser.Id == user.Id)
                _session.CurrentUser = user;
        }

        public override void OnAfterUpdate(BaseApp.Reference.User target)
        {
            SetUser(target);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Listener;
            if (other == null)
                return false;
            return other._session == _session;

        }

        public override int GetHashCode()
        {
            if (_session != null) return _session.GetHashCode();
            return base.GetHashCode();
        }
    }


}