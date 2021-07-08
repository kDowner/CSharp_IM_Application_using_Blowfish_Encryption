using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Blowfish Enryption and KeyGen Classes
using BlowFishCS;

namespace Program
{

   class TestHarness
   {
      static void Main(string[] args)
      {
         // To hold the unencrypted and encrypted strings
         string unencrypted = "";
         string encrypted = "";
         string randKeyGen = "";


         //Provide the key when creating the object.The key can be any size up to 448 bits (NOTE: So 56 Bytes Max for int paramater input)
         //The key can be given as a hex string or an array of bytes.
         BlowFish b = new BlowFish("04B915BA43FEB5B6");

         // NOTE THIS KEY CAN BE MADE INTO ANY RANDOM ITEMS SO EACH KEY IS UNIQUE
         randKeyGen = randomKeyGen(56);
         BlowFish c = new BlowFish(randKeyGen);
         

         //The test string input being used to encrypt
         //Can be either a string or byte array.
         unencrypted = "Dusan is the best programmer in the world!";


         /* ---CBC-- -
          CBC mode encrypts each block of data in succession so that any changes in the data will result in a completly different ciphertext.
          Also, an IV is used so that encrypting the same data with the same key will result in a different ciphertext.
          CBC mode is the most popular mode of operation (NOTE: RESEARCHED OTHER METHODS, THIS IS THE ONE TO USE)*/


         Console.WriteLine("FIRST EXAMPLE USING LITERAL SET KEY (line 25)");
         //Encrypting using CBC Fixed Key
         encrypted = b.Encrypt_CBC(unencrypted);
         //Print Result
         Console.WriteLine($"Encrypted Result: {encrypted}");

         //Decrypting using CBC Fixed Key
         unencrypted = b.Decrypt_CBC(encrypted);
         //Print Result
         Console.WriteLine($"Decrypted Result: {unencrypted}\n");

         Console.WriteLine("SECOND EXAMPLE USING GENERATED KEY FROM 'randomKeyGen' FUNCTION (line 29)");
         //Encrypting using CBC Random Generated 448 Bit Key
         encrypted = c.Encrypt_CBC(unencrypted);
         //Print Result
         Console.WriteLine($"Encrypted Result: {encrypted}");

         //Decrypting using CBC Random Generated 448 Bit Key
         unencrypted = c.Decrypt_CBC(encrypted);
         //Print Result
         Console.WriteLine($"Decrypted Result: {unencrypted}\n");

      }


      //Function:  Creates a random Key ID, based on the maximum 448 Bits allowed with Blowfish
      static string randomKeyGen(int numBytes)
      {
         Random random = new Random();
         string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
         StringBuilder result = new StringBuilder(numBytes);
         for (int i = 0; i < numBytes; i++)
         {
            result.Append(characters[random.Next(characters.Length)]);
         }
         return result.ToString();
      }
   }
}



