using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace VendingMachine.Domain
{
    // Exception: gets thrown when the machine can't provide correct change
    public class ChangeNotAvailableException : Exception
    {
        public ChangeNotAvailableException()
            : base("Cannot provide correct change.") { }

        public ChangeNotAvailableException(string message)
            : base(message) { }
    }
}

