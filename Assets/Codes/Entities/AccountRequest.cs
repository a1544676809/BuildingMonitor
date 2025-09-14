using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Codes.Entities
{
    public class LoginRequest
    {
        public string username;
        public string password;
        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    public class RegistrationRequest
    {
        public string username;
        public string password;
        public RegistrationRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}
