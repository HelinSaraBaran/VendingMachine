using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Infrastructure
{
    using VendingMachine.Domain;

    // interface - access the money inventory
    public interface IMoneyRepository
    {
        // returns the single money inventory
        MoneyInventory GetInventory();
    }
}
