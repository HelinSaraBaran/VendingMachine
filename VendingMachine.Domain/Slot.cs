using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine.Domain
{
    public class Slot
    {
        // our properties for slot 
        public string Code { get; private set; }
        public int Capacity { get; private set; }
        public int Quantity { get; private set; }
        public ProductType Product { get; private set; }

        // still a property but int? means it can be null
        public int? PriceOverride { get; private set; }

        // constructor 
        public Slot(string code, int capacity, int quantity, ProductType product, int? priceOverride)
        {
            Code = code;
            Capacity = capacity;
            Quantity = quantity;
            Product = product;
            PriceOverride = priceOverride;
        }

    }
}
