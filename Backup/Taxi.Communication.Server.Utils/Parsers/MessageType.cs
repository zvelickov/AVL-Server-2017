using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Parsers
{

    public enum Command
    {
        BINNARY_INTEGER_READ = 1,
        ASCII_INTEGER_READ = 2,
        GENERAL_READ = 3,
        INTEGER_WRITE = 4,
        CHAR_WRITE = 5,
        CHAR_READ = 6
    }
}
