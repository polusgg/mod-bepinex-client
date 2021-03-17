using System;
using System.Net;

namespace PolusApi.Resources {
    public class CacheRequestException : Exception {
        public HttpStatusCode Code { get; }
        public CacheRequestException(string message, HttpStatusCode code) : base(message) {
            Code = code;
        }
    }
}