//	FILE           : Client.cs
//	PROJECT			: INFO2231 A1, Encrypted Chat
//	PROGRAMMER		: DUSAN SASIC and KEVIN DOWNER
//	FIRST VERSION  : 2021-03-03
//	DESCRIPTION		: From where the Client module sources the constructor instances and the functions for connecting with
//                the server and other clients.  Functions for connection, sending and receiving messages (enc or plain).

using System;
using System.Net.Sockets;
using System.Threading;
//Microsoft Cryptography Library
using System.Security.Cryptography;
//Blowfish Algorithm Linked
using BlowFishCS;
using System.Text;

namespace Adv_Security_A1_Server
{
   class Client
   {
      //Constants
      private const int BUFFER_CAP = 256;
      private const string KEY = "04B915BA43FEB5B6";

      //Data Members
      private TcpClient client;
      private NetworkStream nStream;
      private BlowFish bf;

      private String Ip;
      private int Port;
      private String UserName;
      private String FriendName;

      //Message and option controllers
      bool sendName = true;
      bool sendFname = true;
      bool useEncryption = false;



      //Default constructor
      /// <summary>
      /// Name:	   Client()
      /// Purpose:   Default constructor.  Starts an object instance of the Client for the user.
      /// Inputs:    None (Default setup parameters)
      /// </summary>
      public Client()
      {
         Ip = "127.0.0.1";
         Port = 10400;
         useEncryption = false;
         UserName = "Not Provided";
         FriendName = "Not Provided";
      }



      //Main constructor
      /// <summary>
      /// Name:      Client(String ip, int port, bool useEnc, String uName, String fName)
      /// Purpose:   Overloaded constructor.  Starts an object instance of the Client for the user.
      /// </summary>
      /// Inputs:
      /// <param name="ip">IP parameter for connection</param>
      /// <param name="port">Port parameter for connection</param>
      /// <param name="useEnc">Use the Blowish encryption or not</param>
      /// <param name="uName">The Name of the user</param>
      /// <param name="fName">The perosn the user wishes to connect to</param>
      public Client(String ip, int port, bool useEnc, String uName, String fName)
      {
         Ip = ip;
         Port = port;
         useEncryption = useEnc;
         UserName = uName;
         FriendName = fName;
      }



      /// <summary>
      /// Name:	   Connect()
      /// Purpose:   Function connects to the server to inititate a chat with another user.
      /// Inputs:    None
      /// Returns:   None void
      /// </summary>
      public void Connect()
      {
         //Attempt to establish a stream connection
         try
         {
            //Assigns IP and Port info. for new instance Client
            client = new TcpClient(Ip, Port);
            //Get the stream connection with the Client setup
            nStream = client.GetStream();
            //Grab the hash ID for the Blowish algorithm
            bf = new BlowFish(KEY);
         }
         //Problem establishing connection
         catch (Exception e)
         {
            Console.WriteLine("[ERROR] - Client cannot be created. Check IP & Port");
            return;
         }

         //Create and establish threads for connection and sending messages 
         ThreadStart ts2 = new ThreadStart(ListenForResponse);
         Thread listenThread = new Thread(ts2);
         ThreadStart ts = new ThreadStart(SendMessage);
         Thread sendThread = new Thread(ts);

         //Start the listen and start threads for the client
         listenThread.Start();
         sendThread.Start();

         //Join with the server to start using the chat features
         listenThread.Join();
         sendThread.Join();
      }



      /// <summary>
      /// Name:	   SendMessage()
      /// Purpose:   Setup for a Client sends a message to another client.
      /// Inputs:    None
      /// Returns:   None void
      /// </summary>
      public void SendMessage()
      {
         //Until otherwise cahnged, keep sending those messages
         while (true)
         {
            //Message is coming from the user
            if (sendName)
            {
               //Buffer the user message and write it to the stream
               Byte[] nameBuffer = System.Text.Encoding.ASCII.GetBytes(UserName);
               nStream.Write(nameBuffer, 0, nameBuffer.Length);
               //Message complete, change flag to false to offer option for message ID for next time
               sendName = false;
            }
            //Message is coming from the Friend
            else if (sendFname)
            {
               //Console.Write("Friend's name:");
               //FriendName = Console.ReadLine();

               //Buffer the friend message and write it to the stream
               Byte[] friendBuffer = System.Text.Encoding.ASCII.GetBytes(FriendName);
               nStream.Write(friendBuffer, 0, friendBuffer.Length);
               //Message complete, change flag to false to offer option for message ID for next time
               sendFname = false;
               //Connection between user and friend established comfirmation
               Console.WriteLine("\n*********************Your Chat With {0}************************", FriendName);
            }

            else
            {
               //Prepare to read the user input
               String msg = Console.ReadLine();

               //Check the length of the user message.  Is it too long?
               if (msg.Length >= BUFFER_CAP)
               {
                  Console.WriteLine("Message cannot be longer than {0} characters.", BUFFER_CAP);
                  continue;
               }

               //For the byte data conversion
               byte[] data;

               //Encryption option chosen for message
               if (useEncryption)
               {
                  //Encrypt the message with the Blowish algorithm
                  string encString = bf.Encrypt_CBC(msg);
                  data = System.Text.Encoding.ASCII.GetBytes(encString);
               }
               else
               {
                  //Unencrypted option chosen:  plain-text message
                  data = System.Text.Encoding.ASCII.GetBytes(msg);
               }

               //Send the messge out
               nStream.Write(data, 0, data.Length);
            }

            //Pause the thread after message sent
            Thread.Sleep(250);
         }
      }



      /// <summary>
      /// Name:	   ListenForResponse()
      /// Purpose:   Setup for a Client to listen for received messages.
      /// Inputs:    None
      /// Returns:   None void
      /// </summary>
      public void ListenForResponse()
      {

         //Main Loop
         while (true)
         {
            //Data holders for response from server
            Byte[] data = new Byte[BUFFER_CAP];
            String responseMessage = String.Empty;
            String decString = String.Empty;

            //Encryption options checked
            try
            {
               //Encryption option yes
               if (useEncryption)
               {
                  //Stream message read
                  int bytes = nStream.Read(data, 0, data.Length);
                  //Convert the byte message to ASCII and store
                  responseMessage = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                  //Decrypt the message sent with the Blowfish algorithm
                  decString = bf.Decrypt_CBC(responseMessage);
                  //Trim any extraneous whitespace
                  decString.Trim();

                  //If not send name or send friend, must be receive
                  if (!sendName && !sendFname)
                  {
                     //Write exnrypted message to chat console
                     Console.WriteLine("{0}(enc): {1}", FriendName, responseMessage);
                     //Write plain-text decrypted message to chat console
                     Console.WriteLine("{0}: {1}", FriendName, decString);
                  }
               }
               //Encryption option no
               else
               {
                  //Stream read
                  int bytes = nStream.Read(data, 0, data.Length);
                  //Converted from byte to ASCII
                  responseMessage = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                  //Trim the extraneous whitespace
                  decString = responseMessage.Trim();

                  //If not send name or send friend, must be receive
                  if (!sendName && !sendFname)
                  {
                     //Write plain-text decrypted message to chat console
                     Console.WriteLine("{0}: {1}", FriendName, decString);
                  }
               }
            }
            //Connection to server lost problem
            catch (Exception e)
            {
               Console.WriteLine("Server Problem. Please try again later.");
               return;
            }
         }
      }
   }
}

