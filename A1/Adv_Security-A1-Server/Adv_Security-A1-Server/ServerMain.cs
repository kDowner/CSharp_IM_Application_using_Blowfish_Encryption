//	FILE           : ServerMain.cs
//	PROJECT			: INFO2231 A1, Encrypted Chat
//	PROGRAMMER		: DUSAN SASIC and KEVIN DOWNER
//	FIRST VERSION  : 2021-03-03
//	DESCRIPTION		: The Server application for listening and controlling the flow of initiated chat clients.
//                Starts up and instance of the server module and waits for cloent to connect.
//                All chat connection and dialogue is routed through the server (could be potential logging etc.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Adv_Security_A1_Server
{
   class Program
   {
      //main Server program start
      static void Main(string[] args)
      {
         //Providing arguments
         //Server server = new Server("127.0.0.1", 10400);

         //Default construtor uses localhost and port 10400
         Server server = new Server();

         //Start up the server to listen for clients
         server.Start();
      }
   }
}
