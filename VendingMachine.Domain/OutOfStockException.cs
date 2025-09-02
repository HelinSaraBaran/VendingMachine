using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace VendingMachine.Domain
{
    // Exception: thrown when a slot has no more items
    public class OutOfStockException : Exception
    {
        public OutOfStockException()
            : base("Slot is out of stock.") { }

        public OutOfStockException(string message)
            : base(message) { }
    }
}

