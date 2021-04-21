using System;
using System.Net;

namespace PolusGG.Resources {
    public class CacheRequestException : Exception {
        public CacheRequestException(string message, HttpStatusCode code) : base(message) {
            Code = code;
        }

        public HttpStatusCode Code { get; }
    }
}