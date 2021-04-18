using System;
using System.Security.Cryptography;
using System.Text;
using PolusggSlim.Utils;
using UnhollowerBaseLib;

namespace PolusggSlim.Auth
{
    public class SigningHelper
    {
        private const byte AuthByte = 0x80;
        private const byte UuidSize = 16;
        private const byte HashSize = 20;

        private readonly AuthContext _authContext;
        private readonly HMAC _hmac;

        public SigningHelper(AuthContext authContext)
        {
            _hmac = HMAC.Create();
            _authContext = authContext;
        }

        public void SignByteArray(ref Il2CppStructArray<byte> bytes)
        {
            _hmac.Key = Encoding.UTF8.GetBytes(_authContext.ClientToken);

            byte[] hash = _hmac.ComputeHash(bytes);
            
            byte[] output = new byte[1 + UuidSize + HashSize + bytes.Length];
            
            output[0] = AuthByte;
            Encoding.UTF8.GetBytes(_authContext.ClientId).CopyTo(output, 1);
            hash.CopyTo(output, 1 + UuidSize);
            bytes.CopyTo(output, 1 + UuidSize + HashSize);
            
            bytes = output;
        }
    }
}