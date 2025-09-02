using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Infrastructure
{
    using VendingMachine.Domain;

    public class InMemoryMoneyRepository : IMoneyRepository
    {
        // holds the money inventory
        private MoneyInventory inventory;

        // constructor 
        public InMemoryMoneyRepository(MoneyInventory moneyInventory)
        {
            inventory = moneyInventory;
        }

        // returns the single inventory
        public MoneyInventory GetInventory()
        {
            return inventory;
        }
    }
}
