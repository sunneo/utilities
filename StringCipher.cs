using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Utilities
{
    public sealed class Base32
    {

        // the valid chars for the encoding
        private static string ValidChars = "QAZ2WSX3" + "EDC4RFV5" + "TGB6YHN7" + "UJM8K9LP";

        /// <summary>
        /// Converts an array of bytes to a Base32-k string.
        /// </summary>
        public static string ToBase32String(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();         // holds the base32 chars
            byte index;
            int hi = 5;
            int currentByte = 0;

            while (currentByte < bytes.Length)
            {
                // do we need to use the next byte?
                if (hi > 8)
                {
                    // get the last piece from the current byte, shift it to the right
                    // and increment the byte counter
                    index = (byte)(bytes[currentByte++] >> (hi - 5));
                    if (currentByte != bytes.Length)
                    {
                        // if we are not at the end, get the first piece from
                        // the next byte, clear it and shift it to the left
                        index = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | index);
                    }

                    hi -= 3;
                }
                else if (hi == 8)
                {
                    index = (byte)(bytes[currentByte++] >> 3);
                    hi -= 3;
                }
                else
                {

                    // simply get the stuff from the current byte
                    index = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
                    hi += 5;
                }

                sb.Append(ValidChars[index]);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Converts a Base32-k string into an array of bytes.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Input string <paramref name="s">s</paramref> contains invalid Base32-k characters.
        /// </exception>
        public static byte[] FromBase32String(string str)
        {
            int numBytes = str.Length * 5 / 8;
            byte[] bytes = new Byte[numBytes];

            // all UPPERCASE chars
            str = str.ToUpper();

            int bit_buffer;
            int currentCharIndex;
            int bits_in_buffer;

            if (str.Length < 3)
            {
                bytes[0] = (byte)(ValidChars.IndexOf(str[0]) | ValidChars.IndexOf(str[1]) << 5);
                return bytes;
            }

            bit_buffer = (ValidChars.IndexOf(str[0]) | ValidChars.IndexOf(str[1]) << 5);
            bits_in_buffer = 10;
            currentCharIndex = 2;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)bit_buffer;
                bit_buffer >>= 8;
                bits_in_buffer -= 8;
                while (bits_in_buffer < 8 && currentCharIndex < str.Length)
                {
                    bit_buffer |= ValidChars.IndexOf(str[currentCharIndex++]) << bits_in_buffer;
                    bits_in_buffer += 5;
                }
            }

            return bytes;
        }
    }
    public static class StringCipher
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;
        public static String PassKey;
        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;
        public static string Encrypt(string plainText)
        {
            return Encrypt(plainText, PassKey);
        }
        public static String Decrypt(String cipher)
        {
            return Decrypt(cipher, PassKey);
        }
        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Base32.ToBase32String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Base32.FromBase32String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
    public class Cipher
    {
        public static String Encode(String content, String phaseKey)
        {
            try
            {
                return StringCipher.Encrypt(content, phaseKey);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
                return "ERROR";
            }
        }
        public static String Decode(String content, String phaseKey)
        {
            try
            {
                return StringCipher.Decrypt(content, phaseKey);
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
                return "ERROR";
            }
        }
    }

}
