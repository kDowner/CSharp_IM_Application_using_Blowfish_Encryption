//	FILE           : Server.cs
//	PROJECT			: INFO2231 A1, Encrypted Chat
//	PROGRAMMER		: DUSAN SASIC and KEVIN DOWNER
//	FIRST VERSION  : 2021-03-03
//	DESCRIPTION		: From where the Server module sources the constructor instances and the functions for connecting with
//                other clients.  Functions for connection, sending and receiving messages.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;



namespace Adv_Security_A1_Server
{
   class Server
   {
      //Constants
      private const int BUFFER_CAP = 256;

      //Data members
      private static String Ip;
      private static int Port;
      List<TcpClient> ClientList;
      List<String> Usernames;
      List<String> Friendnames;

      //Complex data members
      TcpListener server;



      //Default constructor
      /// <summary>
      /// Name:	   Server()
      /// Purpose:   Default constructor.  Starts an object instance of the Server for the user.
      /// Inputs:    None (Default setup parameters)
      /// </summary>
      public Server()
      {
         Ip = "127.0.0.1";
         Port = 10400;
         ClientList = new List<TcpClient>();
         Usernames = new List<string>();
         Friendnames = new List<string>();
      }



      //Main constructor
      /// <summary>
      /// Name:	   Server(String address, int portNum)
      /// Purpose:   Default constructor.  Starts an object instance of the Client for the user.
      /// Inputs:    None (Default setup parameters)
      /// </summary>
      /// <param name="address">IP parameter for address for clients</param>
      /// <param name="portNum">Port parameter for connection</param>
      public Server(String address, int portNum)
      {
         Ip = address;
         Port = portNum;
         ClientList = new List<TcpClient>();
         //Holds the chat input user names
         Usernames = new List<string>();
         //Holds the friend input user names
         Friendnames = new List<string>();
      }



      //Start
      /// <summary>
      /// Name:	   Start()
      /// Purpose:   Starts up the Server for Chat clients to connect.
      /// Inputs:    None
      /// Returns:   None void
      /// </summary>
      public void Start()
      {
         //Attempt to initialize server
         try
         {
            //Instance a Server
            server = new TcpListener(IPAddress.Parse(Ip), Port);
            //Startup procedures
            server.Start();
         }
         //Couldn't start, port IP issue
         catch (Exception e)
         {
            Console.WriteLine("[ERROR] - Issue while starting the server. Check IP/Port");
            return;
         }

         //Successful server start, waiting for clients to connect
         Console.WriteLine("Server waiting for connection....");

         //Client connection actions
         ConnectWithClients();
      }



      /// <summary>
      /// Name:	   ConnectWithClients()
      /// Purpose:   Cliennt connect request actions and itemize conenction.
      /// Inputs:    None
      /// Returns:   None void
      /// </summary>
      private void ConnectWithClients()
      {
         while (true)
         {
            //Attempt to establish client connection
            try
            {
               //Accept the client pending connection
               TcpClient connectedClient = server.AcceptTcpClient();
               //Add to the list of clients
               ClientList.Add(connectedClient);
               //Connection ok, notify of success
               Console.WriteLine("Connected clients: {0}", ClientList.Count);
               //TCP 2-way threading
               int index = SetupTwoWay(connectedClient.GetStream());
               //Start the threads for client and listen for data
               ParameterizedThreadStart pts = new ParameterizedThreadStart(ListenForData);
               //Listen thread
               Thread listenThread = new Thread(pts);
               //Start the thread
               listenThread.Start(index);
            }
            //Client unable to connect problem
            catch (Exception e)
            {
               Console.WriteLine("[ERROR] - Client cannot connect");
               return;
            }
         }
      }



      /// <summary>
      /// Name:	   ListenForData(object indexClient)
      /// Purpose:   Function grabs the byte data form cients and send to their friend.
      /// Returns:   None void
      /// </summary>
      /// <param name="indexClient">object indexClient for client list checks</param>
      public void ListenForData(object indexClient)
      {
         while (true)
         {
            //Local Variables
            String recMessage = String.Empty;
            int bytesRead;
            //Prepare the buffer for messaages
            Byte[] buffer = new Byte[BUFFER_CAP];

            //Get the client that send messages and transfer it to client that needs to recieve them
            int index = (int)indexClient;
            TcpClient client = ClientList[index];
            NetworkStream nStream = client.GetStream();

            //client back and forth messaging
            try
            {
               // Loop to receive all the data sent by the client.
               bytesRead = nStream.Read(buffer, 0, buffer.Length);

               recMessage = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
               Console.WriteLine("\nServer received: {0}", recMessage);

               //Send to client 2
               SendMessage(index, recMessage);
            }
            //Client has left the server
            catch (Exception e)
            {
               Console.WriteLine("=======Client has disconnected!=====");
               //Remove client from indexed clients
               int indexToRemove = ClientList.IndexOf(client);
               if (indexToRemove != -1)
               {
                  //Remove stored data for connect
                  ClientList.Remove(client);
                  Usernames.RemoveAt(indexToRemove);
                  Friendnames.RemoveAt(indexToRemove);
               }

               break;
            }
         }
      }



      /// <summary>
      /// Name:	   SendMessage(int userIndex, String msg)
      /// Purpose:   Function connects to the server to inititate a chat with another user.
      /// Returns:   None void
      /// </summary>
      /// <param name="userIndex">Users count for the server to manage</param>
      /// <param name="msg">Messages sent and received</param>
      public void SendMessage(int userIndex, String msg)
      {
         //Establish the indexes for Name and Friend lists
         String UserName = Usernames[userIndex];
         String FriendName = Friendnames[userIndex];
         //Add the friend to the index
         int friendPrimaryIndex = Usernames.IndexOf(FriendName);
         
         //friend check online for message receive
         if (friendPrimaryIndex != -1)
         {
            //Make the friend client
            TcpClient friendClient = ClientList[friendPrimaryIndex];
            //Stream obtained
            NetworkStream nStream = friendClient.GetStream();
            //Encode the data for transfer
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            //Send it out
            nStream.Write(data, 0, data.Length);
         }
         //Friend not online to talk to
         else
         {
            NetworkStream initatorStream = ClientList[userIndex].GetStream();
            //Prep friend not there message and send
            Byte[] data = System.Text.Encoding.ASCII.GetBytes("[ is offline. ]");
            initatorStream.Write(data, 0, data.Length);
         }
      }



      /// <summary>
      /// Name:	   SetupTwoWay(NetworkStream nStream)
      /// Purpose:   Function for communication 2-way interaction between clients.
      /// Returns:   int currIndex for the index of chat clients
      /// </summary>
      /// <param name="nStream">The network connection stream</param>
      /// <returns></returns>
      public int SetupTwoWay(NetworkStream nStream)
      {
         //Local variables
         String recName = String.Empty;
         String recFriendName = String.Empty;
         Byte[] buffer = new Byte[BUFFER_CAP];
         int bytesRead = 0;


         //Recieve the User Name
         bytesRead = nStream.Read(buffer, 0, buffer.Length);
         //Buffer the name ID and write it to the stream
         recName = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
         //Confirmation of name
         Console.WriteLine("User Name: {0}", recName);
         //Add the name
         Usernames.Add(recName);


         //Recieve the Friend Name
         Array.Clear(buffer, 0, buffer.Length);
         bytesRead = nStream.Read(buffer, 0, buffer.Length);
         //Buffer the friend ID and write it to the stream
         recFriendName = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
         //Confirmation of friend
         Console.WriteLine("Friend Name: {0}", recFriendName);
         //Add the friend
         Friendnames.Add(recFriendName);

         //Index names for check
         int currIndex = Usernames.IndexOf(recName);

         //Friend check, not there to connect to
         if (currIndex == -1)
         {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(recFriendName + "is offline. ");
            nStream.Write(data, 0, data.Length);
         }

         return currIndex;
      }
   }
}
