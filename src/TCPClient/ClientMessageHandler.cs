using OpenFAST.Sessions;

namespace OpenFAST.TCPClient
{
    public class ClientMessageHandler : IMessageListener
    {
        //When the server sends a message this will print that on the console.

        #region MessageListener Members

        public void OnMessage(Session session, Message message)
        {
//            System.Console.WriteLine(message.ToString());//UNCOMMENT
        }

        #endregion
    }
}