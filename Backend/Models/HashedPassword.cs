using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace FullStackSandbox.Backend.Models
{
    public sealed record HashedPassword
    {
        public const int PasswordByteSize = 20;
        public const int SaltByteSize = 16;

        public byte[] PasswordBytes { get; }
        public byte[] SaltBytes { get; }

        /// <summary>
        /// Constructs a new instance from password- and salt bytes. The password is expected to be salted with <paramref name="salt"/> already.
        /// </summary>
        public HashedPassword(byte[] password, byte[] salt)
        {
            Debug.Assert(password is not null && password.Length == PasswordByteSize);
            Debug.Assert(salt is not null && salt.Length == SaltByteSize);

            PasswordBytes = password;
            SaltBytes = salt;
        }

        /// <summary>
        /// Constructs a hashed password around a combined set of password- and salt bytes. The password part is expected to be salted with the salt part already.
        /// </summary>
        /// <param name="passwordAndSalt">The password and salt combined. It is expected that <c>passwordAndSalt[0 -> PasswordByteSize]</c> contains the password, and that <c>passwordAndSalt[PasswordByteSize -> end]</c> contains the salt.</param>
        public HashedPassword(byte[] passwordAndSalt)
        {
            Debug.Assert(passwordAndSalt is not null && passwordAndSalt.Length == PasswordByteSize + SaltByteSize);

            PasswordBytes = new byte[PasswordByteSize];
            SaltBytes = new byte[SaltByteSize];

            Array.Copy(passwordAndSalt, 0, PasswordBytes, 0, PasswordByteSize);
            Array.Copy(passwordAndSalt, PasswordByteSize, SaltBytes, 0, SaltByteSize);
        }

        /// <summary>
        /// Converts the combined password- and salt bytes into a base64 string.
        /// </summary>
        /// <returns>A base64 string composed of <see cref="SaltBytes"/> appended unto <see cref="PasswordBytes"/>.</returns>
        public string AsBase64String()
        {
            return Convert.ToBase64String(CombinePasswordAndSalt());
        }

        /// <summary>
        /// Determines the equality of this instance with another in the amount of time that equals their password and salt lengths, but not their contents.
        /// </summary>
        public bool SecureEqualTo(HashedPassword other)
        {
            return CryptographicOperations.FixedTimeEquals( CombinePasswordAndSalt(), other.CombinePasswordAndSalt() );
        }

        /// <summary>
        /// Creates a new hashed password from a given password string with randomly generated salt.
        /// </summary>
        /// <param name="passwordString">The string to hash and salt</param>
        /// <returns>A new instance representing the result of hashing <paramref name="passwordString"/> with a random salt value.</returns>
        public static HashedPassword Create(string passwordString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordString);

            byte[] PasswordBytes = Encoding.UTF8.GetBytes(passwordString);
            byte[] Salt = GenerateSalt();

            return new HashedPassword(GetSaltedPassword(PasswordBytes, Salt), Salt);
        }

        /// <summary>
        /// Creates a new hashed password from a given password string that will be salted with a given value.
        /// </summary>
        /// <param name="passwordString">The string to hash and salt</param>
        /// <param name="salt">The value to salt <paramref name="passwordString"/> with.</param>
        /// <returns>A new instance representing the result of hashing <paramref name="passwordString"/> with <paramref name="salt"/>.</returns>
        public static HashedPassword Create(string passwordString, byte[] salt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordString);

            byte[] PasswordBytes = Encoding.UTF8.GetBytes(passwordString);
            return new HashedPassword(GetSaltedPassword(PasswordBytes, salt), salt);
        }

        private static byte[] GetSaltedPassword(byte[] password, byte[] salt)
        {
            using (var Rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return Rfc2898.GetBytes(PasswordByteSize);
            }
        }

        private static byte[] GenerateSalt()
        {
            var ReturnData = new byte[SaltByteSize];
            using (var RNGSecure = RandomNumberGenerator.Create())
            {
                RNGSecure.GetBytes(ReturnData);
                return ReturnData;
            }
        }

        private byte[] CombinePasswordAndSalt()
        {
            byte[] ReturnData = new byte[PasswordByteSize + SaltByteSize];

            Array.Copy(PasswordBytes, 0, ReturnData, 0, PasswordByteSize);
            Array.Copy(SaltBytes, 0, ReturnData, PasswordByteSize, SaltByteSize);

            return ReturnData;
        }
    };
}
