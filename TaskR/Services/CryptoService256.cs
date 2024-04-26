using System.Security.Cryptography;

namespace TaskR.Services;

public class CryptoService256
{
    public byte[] GenerateSalt()
    {
        byte[] salt = new byte[256/8];

        RandomNumberGenerator.Fill(salt);

        return salt;
 
        //Alternativ:
        //return RandomNumberGenerator.GetBytes(32);
    }

    public byte[] SaltString(string str, byte[] salt, System.Text.Encoding encoding)
    {
        //Der String muss in ein Byte-Array umgewandelt werden
        //Wie ein String (bzw. einzelne Zeichen in bytes dargestellt werden hängt vom Encoding ab)
        byte[] stringBytes = encoding.GetBytes(str);

        byte[] saltedString = stringBytes.Concat(salt).ToArray();

        return saltedString;
    }

    public byte[] GetHash(byte[] input)
    {
        SHA256 hasher = SHA256.Create();

        byte[] hashedBytes = hasher.ComputeHash(input);

        return hashedBytes;
    }
}
