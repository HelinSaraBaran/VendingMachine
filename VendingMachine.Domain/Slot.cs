using System;
using System.Collections.Generic;

namespace VendingMachine.Domain
{
   
    public class Slot
    {
        // code like "A1"
        public string Code { get; private set; }

        // max capacity
        public int Capacity { get; private set; }

        // product type assigned to this slot
        public ProductType Product { get; private set; }

        // optional price override
        public int? PriceOverride { get; private set; }

        // internal queue of physical items 
        private Queue<int> itemQueue;

        // quantity 
        public int Quantity
        {
            get { return itemQueue.Count; }
        }

        // Constructor
        public Slot(string code, int capacity, int initialQuantity, ProductType product, int? priceOverride)
        {
            // input validation = if checks = exceptions
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code must not be empty.", "code");
            }
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "Capacity must be > 0.");
            }
            if (initialQuantity < 0)
            {
                throw new ArgumentOutOfRangeException("initialQuantity", "Initial quantity must be ≥ 0.");
            }
            if (initialQuantity > capacity)
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

            // set properties
            Code = code;
            Capacity = capacity;
            Product = product;
            PriceOverride = priceOverride;

            // initialize queue and preload items
            itemQueue = new Queue<int>();
            for (int itemIndex = 0; itemIndex < initialQuantity; itemIndex = itemIndex + 1)
            {
                // enqueue a dummy token for each physical item
                itemQueue.Enqueue(1);
            }
        }

        // True if at least one item is available
        public bool HasStock()
        {
            return itemQueue.Count > 0;
        }

        // Active price (override if present, else base)
        public int GetPrice()
        {
            return PriceOverride.HasValue ? PriceOverride.Value : Product.BasePrice;
        }

        // Remove a single item (dequeue after purchase)
        public void RemoveOne()
        {
            if (itemQueue.Count == 0)
            {
                throw new OutOfStockException("Slot " + Code + " is out of stock.");
            }
            // FIFO: take the oldest item
            itemQueue.Dequeue();
        }

        // Refill with "count" items (admin)
        public void Refill(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }
            if (itemQueue.Count + count > Capacity)
            {
                throw new InvalidOperationException("Cannot exceed slot capacity.");
            }
            for (int itemIndex = 0; itemIndex < count; itemIndex = itemIndex + 1)
            {
                itemQueue.Enqueue(1);
            }
        }

        // Refill to full capacity (returns how many items were added)
        public int RefillToCapacity()
        {
            int freeSpace = Capacity - itemQueue.Count;
            if (freeSpace <= 0)
            {
                return 0;
            }
            for (int itemIndex = 0; itemIndex < freeSpace; itemIndex = itemIndex + 1)
            {
                itemQueue.Enqueue(1);
            }
            return freeSpace;
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
