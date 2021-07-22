using System.Security.Cryptography;
using System.Text;
using UnhollowerBaseLib;

namespace PolusggSlim.Auth
{
    public class SigningHelper
    {
        public const byte AuthByte = 0x80;
        private const byte UuidSize = 16;
        private const byte HashSize = 20;

        private readonly AuthContext _authContext;
        private HMAC _hmac;

        public SigningHelper(AuthContext authContext)
        {
            _hmac = HMAC.Create();
            _authContext = authContext;
        }

        public void SignByteArray(ref Il2CppStructArray<byte> bytes)
        {
            var clientToken = Encoding.UTF8.GetBytes(_authContext.ClientToken);
            if (_hmac.Key != clientToken)
            {
                _hmac = HMAC.Create();
                _hmac.Key = clientToken;
            }

            var hash = _hmac.ComputeHash(bytes);

            var output = new byte[1 + UuidSize + HashSize + bytes.Length];

            output[0] = AuthByte;
            _authContext.ClientId.CopyTo(output, 1);
            hash.CopyTo(output, 1 + UuidSize);
            bytes.CopyTo(output, 1 + UuidSize + HashSize);

            bytes = output;
        }
    }
}