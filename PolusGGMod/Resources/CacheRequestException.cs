using System;
using System.Net;

namespace PolusGG.Resources {
    public class CacheRequestException : Exception {
        public HttpStatusCode Code { get; }
        public CacheRequestException(string message, HttpStatusCode code) : base(message) {
            Code = code;
        }
    }
}