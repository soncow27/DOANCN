using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAnCN.Models
{
    public class LoginModel
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
    }
}