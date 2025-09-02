using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    namespace VendingMachine.Domain
    {
        // Physical slot holding one product type
        public class Slot
        {
            public string Code { get; private set; }         
            public int Capacity { get; private set; }        
            public int Quantity { get; private set; }          
            public ProductType Product { get; private set; }  
            public int? PriceOverride { get; private set; }    

        // Constructor 
            public Slot(string code, int capacity, int quantity, ProductType product, int? priceOverride)

            { //Input validation = if tjek = Exceptions
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new ArgumentException("Code must not be empty.", "code");
                }
                if (capacity <= 0)
                {
                    throw new ArgumentOutOfRangeException("capacity", "Capacity must be > 0.");
                }
                if (quantity < 0)
                {
                    throw new ArgumentOutOfRangeException("quantity", "Quantity must be ≥ 0.");
                }
                if (quantity > capacity)
                {
                    throw new InvalidOperationException("Initial quantity cannot exceed capacity.");
                }
                if (product == null)
                {
                    throw new ArgumentNullException("product");
                }
                if (priceOverride.HasValue && priceOverride.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("priceOverride", "Price must be ≥ 0.");
                }

                // properties
                Code = code;
                Capacity = capacity;
                Quantity = quantity;
                Product = product;
                PriceOverride = priceOverride;
            }

            // True if at least one item is available
            public bool HasStock()
            {
                return Quantity > 0;
            }

            // Active price (override if present, else base)
            public int GetPrice()
            {
                return PriceOverride.HasValue ? PriceOverride.Value : Product.BasePrice;
            }

            // Remove a single item (after purchase)
            public void RemoveOne()
            {
                if (Quantity <= 0)
                {
                    throw new OutOfStockException("Slot " + Code + " is out of stock.");
                }
                Quantity = Quantity - 1;
            }

            // Refill with count items (admin)
            public void Refill(int count)
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
                }
                if (Quantity + count > Capacity)
                {
                    throw new InvalidOperationException("Cannot exceed slot capacity.");
                }
                Quantity = Quantity + count;
            }

            // Set or clear price override
            public void SetPriceOverride(int? newPrice)
            {
                if (newPrice.HasValue && newPrice.Value < 0)
                {
                    throw new ArgumentOutOfRangeException("newPrice", "Price must be ≥ 0.");
                }
                PriceOverride = newPrice;
            }
        }
    }

