namespace WijDelen.Mobile.Models.Jwt {
    public class Header {
        public string Alg => "HS256";
        public string Typ => "JWT";
    }
}