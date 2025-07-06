using Hydra.AccessManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.IdentityAndAccess
{
    public interface ISessionContext
    {
        void Set(SessionInformation? session);
        SessionInformation? GetCurrent();
        void Clear();
    }

    public class SessionContext : ISessionContext
    {
        private static readonly AsyncLocal<SessionInformation?> _currentSession = new();

        public void Set(SessionInformation? session)
        {
            _currentSession.Value = session;
        }

        public SessionInformation? GetCurrent()
        {
            return _currentSession.Value;
        }

        public void Clear()
        {
            _currentSession.Value = null;
        }
    }

}
