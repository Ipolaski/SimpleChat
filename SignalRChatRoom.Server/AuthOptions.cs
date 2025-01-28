using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AccountServiceGCA.Application.AuthorizeOptions
{	
    public class AuthOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string _key { get; set; }

        public int LifeTime { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_key));
    }
}