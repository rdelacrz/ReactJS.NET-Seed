using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public struct EnvironmentSettings
    {
        public static bool IsDev = ConfigurationManager.AppSettings["IsDevelopment"]?.ToString() == "true";

        public struct ContactInfo
        {
            public struct ContactParameters
            {
                public string Name { get; set; }

                public string EmailAddress { get; set; }

                public ContactParameters(string name, string emailAddress)
                {
                    Name = name;
                    EmailAddress = emailAddress;
                }
            }

            public static ContactParameters EMAIL_HELP_DESK = new ContactParameters(
                "SeedProjectName Helpdesk",
                "seedprojectname@inovas.net"
            );
            public static ContactParameters EMAIL_INOVAS_SUPPORT = new ContactParameters(
                "INOVAS Support",
                "support@inovas.net"
            );

            public static string TEST_EMAIL_ADDRESS = "EmailTester@inovas.net";
        }
    }
}
