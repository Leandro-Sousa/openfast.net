using System;
using OpenFAST.Error;
using OpenFAST.Sessions;
using OpenFAST.Sessions.Tcp;

namespace OpenFAST.TCPServer
{
    public class FastServer
    {
        private readonly ISessionProtocol _scpSessionProtocol = SessionConstants.Scp11;
        private static Sessions.FastServer _FastServer;

        public FastServer()
        {
            var endpoint = new TcpEndpoint(16121);

            _FastServer = new Sessions.FastServer("test", _scpSessionProtocol, endpoint) {SessionHandler = new SessionHandler() };

            Global.ErrorHandler = new ServerErrorHandler();

            _FastServer.Listen();
        }

        #region Nested type: ServerErrorHandler

        public class ServerErrorHandler : IErrorHandler
        {
            public void OnError(Exception exception, StaticError error, string format, params object[] args)
            {
                if (!string.IsNullOrEmpty(format))
                    Console.WriteLine(format, args);
                else
                    Console.WriteLine($"{exception?.Message}; {error}");
                _FastServer.Close();
            }

            public void OnError(Exception exception, DynError error, string format, params object[] args)
            {
                if (!string.IsNullOrEmpty(format))
                    Console.WriteLine(format, args);
                else
                    Console.WriteLine($"{exception?.Message}; {error}");
                _FastServer.Close();
            }

            public void OnError(Exception exception, RepError error, string format, params object[] args)
            {
                if (!string.IsNullOrEmpty(format))
                    Console.WriteLine(format, args);
                else
                    Console.WriteLine($"{exception?.Message}; {error}");
                _FastServer.Close();
            }
        }

        #endregion
    }
}