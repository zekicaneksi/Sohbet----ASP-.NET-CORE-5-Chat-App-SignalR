using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sohbet.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
