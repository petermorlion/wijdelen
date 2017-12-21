using System;

namespace WijDelen.Mobile.Models {
    public class InvalidJwtException : Exception {
        public InvalidJwtException() : base("The provided JSON Web Token is invalid. Please validate if it contains the three necessary parts (header, payload and hash), and that the hash is correct. You can use the debugger on jwt.io to verify the hash.") {
        }
    }
}