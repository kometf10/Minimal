using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Validators
{
    public class Message
    {
        public static class Messages
        {
            public static string REQUIRED = "Required field";
            public static string EMAIL = "Not a valid email address";
            public static string ENGLISH = "Not a valid english letters";
            public static string PHONE = "Not a valid phone number";
            public static string MOBILE = "Not a valid Mobile number";
            public static string NATIONALNUM = "Not a valid national number";
            public static string NUMBER = "Not a valid number";
            public static string SALARY = "Not a valid Salaty";
            public static string MONY = "Not a valid Amount";
            public static string USERNAME = "Not a valid user name";
            public static string PASSCONFIRM = "Password confirmation miss match";
            public static string AGEOVER18 = "Age shoud be over 18";
            public static string NOTVALID = "Not a valid value";
            public static string PRECENTAGE = "Not a valid Precentage";
        }
    }
}
