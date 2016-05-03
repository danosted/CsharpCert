using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Chapter3
{
    public class EncryptionExample
    {
        public void Run()
        {
            // Initialization vector has to be the same on both ends
            byte[] iv;
            using (var symAlgo = new AesManaged())
            {
                symAlgo.GenerateIV();
                iv = symAlgo.IV;
            }

            var bob = new MessageHolder("Bob", "Hi Alice, I'm Bob. This is supposedly a long message. Here are some private information.", iv);
            var alice = new MessageHolder("Alice", "Hi Bob, I'm Alice. This is supposedly another long message. Here is my personal information.", iv);
            bob.Go(alice);
            alice.Go(bob);
        }

        public class MessageHolder
        {
            private string _name;
            private string _asymPub;
            private string _asymPriv;
            private byte[] _symKey;
            private byte[] _iv;
            private string _symkeyEnc;
            private string _myMessage;

            public MessageHolder(string name, string message, byte[] iv)
            {
                _name = name;
                _myMessage = message;
                _iv = iv;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    // Step 1.
                    _asymPub = rsa.ToXmlString(false);
                    _asymPriv = rsa.ToXmlString(true);
                    Console.WriteLine("Generated asymmetric pub and priv keys for '{0}'.", _name);
                }
                using (var symAlgo = new AesManaged())
                {
                    symAlgo.IV = iv;
                    symAlgo.GenerateKey();
                    _symKey = symAlgo.Key;
                    Console.WriteLine("Generated symmetric key for '{0}'.", _name);
                }
            }

            public void Go(MessageHolder target)
            {
                // Step 2 + 3 + 4 send asymmetric pub key, encrypt symmetric key and send it to one another
                var encSymkey = target.GetEncryptedSymkeyByPublicAsymKey(_asymPub);
                Console.WriteLine("'{0}' recieved encrypted symmetric key.", _name);

                // Step 5 decrypt external symmetric key and use it to encrypt message
                var encMessage = CreateEncryptedMessage(encSymkey);
                Console.WriteLine("'{0}' generated encrypted message using decrypted symmetric key.", _name);

                // Step 5 + 6 send and target decrypts
                target.SetEncryptedMessage(encMessage);
            }
            
            public byte[] GetEncryptedSymkeyByPublicAsymKey(string asymPubXml)
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(asymPubXml);
                    return rsa.Encrypt(_symKey, false);
                }
            }

            private byte[] CreateEncryptedMessage(byte[] encryptedSymKey)
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(_asymPriv);
                    var symKey = rsa.Decrypt(encryptedSymKey, false);
                    Console.WriteLine("'{0}' decrypted symmetric key using private asymmetric key.", _name);

                    using (var symAlgo = new AesManaged())
                    {
                        symAlgo.Key = symKey;
                        symAlgo.IV = _iv;
                        return Encrypt(symAlgo, _myMessage);
                    }
                }
            }

            public void SetEncryptedMessage(byte[] message)
            {
                Console.WriteLine(string.Format("'{0}' recieved encrypted message: '{1}'.", _name, Convert.ToBase64String(message)));
                using (var symAlgo = new AesManaged())
                {
                    symAlgo.Key = _symKey;
                    symAlgo.IV = _iv;
                    var msg = Decrypt(symAlgo, message);
                    Console.WriteLine(string.Format("'{0}' found decrypted message: '{1}'.", _name, msg));
                }
            }

            private static byte[] Encrypt(SymmetricAlgorithm aa, string text)
            {
                var enc = aa.CreateEncryptor(aa.Key, aa.IV);
                using (var msEnc = new MemoryStream())
                {
                    using (var crEnc = new CryptoStream(msEnc, enc, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(crEnc))
                        {
                            sw.Write(text);
                        }
                        return msEnc.ToArray();
                    }
                }
            }

            private static string Decrypt(SymmetricAlgorithm aa, byte[] encryptedText)
            {
                var dec = aa.CreateDecryptor(aa.Key, aa.IV);
                using (var msDec = new MemoryStream(encryptedText))
                {
                    using (var crDec = new CryptoStream(msDec, dec, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(crDec))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
