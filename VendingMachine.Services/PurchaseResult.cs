using System.Collections.Generic;

namespace VendingMachine.Service
{
    using VendingMachine.Domain;

    // result object after a purchase
    public class PurchaseResult
    {
        // the product that was bought
        public ProductType Product { get; private set; }

        // change returned to the customer
        public Dictionary<CoinType, int> Change { get; private set; }

        // constructor
        public PurchaseResult(ProductType product, Dictionary<CoinType, int> change)
        {
            Product = product;
            Change = change;
        }
    }
}
