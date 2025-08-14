using System.Security.Cryptography;
using System.Text;

namespace ShopTechnologyAccessories.Services;

public class PasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        var result = new byte[1 + 4 + SaltSize + KeySize];
        result[0] = 0x01; // version
        BitConverter.GetBytes(Iterations).CopyTo(result, 1);
        salt.CopyTo(result, 1 + 4);
        hash.CopyTo(result, 1 + 4 + SaltSize);
        return Convert.ToBase64String(result);
    }

    public bool Verify(string password, string encodedHash)
    {
        try
        {
            var decoded = Convert.FromBase64String(encodedHash);
            if (decoded.Length < 1 + 4 + SaltSize + KeySize || decoded[0] != 0x01)
            {
                return false;
            }
            var iterations = BitConverter.ToInt32(decoded, 1);
            var salt = decoded.AsSpan(1 + 4, SaltSize).ToArray();
            var expected = decoded.AsSpan(1 + 4 + SaltSize, KeySize).ToArray();
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, KeySize);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch
        {
            return false;
        }
    }
}


