using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    public static class ErrorCodes
    {
        public const int ERROR_SUCCESS = 0;

        // say, when we call Stop() and didn't call Start() before
        public const int NOT_RUNNING = 0x0010;

        // error codes from 0x0100 to 0x01FF
        // are used for file errors
        public const int FILE_NOT_FOUND  = 0x0100;
        public const int FILE_COULDNT_BE_OPEN = 0x0101; // Maybe expand this to ACCESS_DENIED, ALREADY_IN_USE,..
    }
}
