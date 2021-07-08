//	FILE           : ClientMain.cs
//	PROJECT			: INFO2231 A1, Encrypted Chat
//	PROGRAMMER		: DUSAN SASIC and KEVIN DOWNER
//	FIRST VERSION  : 2021-03-03
//	DESCRIPTION		: The Client application for initiating chats with other Clients.
//                It tests encryption by giving the cient an option to send messages, as pain-text or encrypted.
//                Multiple clients can be run that ID themselve by their entered names.
//                They then enter the name of the firend they want to talk to, connecting them to that client
//                for diret interaction.
//                The encryption is handled with the Blowih algorithm.

using Adv_Security_A1_Server;
using System;

namespace Adv_Security_A1_Client
{
   class ClientMain
   {
      //main Client program start
      static void Main(string[] args)
      {
         //Setup holders for Client name and friend to connect to
         String UserName;
         String FriendName;

         //Encryption option presented: y/n?
         bool useEnc = false;
         Console.Write("Enter key 'Y' if you want the messages to be encrypted: ");
         char key = Console.ReadKey().KeyChar;

         //Get clients name
         Console.WriteLine("\n***********************Client Setup***************************");
         Console.Write("Name:");
         UserName = Console.ReadLine();

         //Get client to talk to name
         Console.Write("Friend's name:");
         FriendName = Console.ReadLine();

         //Yes, client wants ecnryption, enable
         if (key == 'y')
         {
            useEnc = true;
         }

         //Client connection attempt made to server to initiate chat conversation
         Client client = new Client("127.0.0.1", 10400, useEnc, UserName, FriendName);
         client.Connect();
      }
   }
}
