using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnhollowerBaseLib;

namespace PolusggSlim.Auth
{
    public class SigningHelper
    {
        public const byte AUTH_BYTE = 0x80;
        private const byte UUID_SIZE = 16;
        private const byte HASH_SIZE = 20;

        private readonly AuthContext _authContext;
        private HMAC _hmac;

        public SigningHelper(AuthContext authContext)
        {
            _hmac = HMAC.Create();
            _authContext = authContext;
        }

        public void SignByteArray(ref Il2CppStructArray<byte> bytes, ref int length)
        {
            var clientToken = Encoding.UTF8.GetBytes(_authContext.ClientToken);
            if (_hmac.Key != clientToken)
            {
                _hmac = HMAC.Create();
                _hmac.Key = clientToken;
            }

            var hash = _hmac.ComputeHash(bytes);

            var output = new byte[1 + UUID_SIZE + HASH_SIZE + length];

            output[0] = AUTH_BYTE;
            _authContext.ClientId.CopyTo(output, 1);
            hash.CopyTo(output, 1 + UUID_SIZE);
            bytes.ToArray()[..length].CopyTo(output, 1 + UUID_SIZE + HASH_SIZE);

            bytes = output;
            length = output.Length;
        }
    }
}