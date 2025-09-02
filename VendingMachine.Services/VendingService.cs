using System;
using System.Collections.Generic;

namespace VendingMachine.Service
{
    using VendingMachine.Domain;
    using VendingMachine.Infrastructure;

    // vending logic
    public class VendingService
    {
        // repositories 
        private ISlotRepository slotRepositoryInstance;
        private IMoneyRepository moneyRepositoryInstance;

        // tracks how much the customer has inserted 
        private int insertedAmount;

        // constructor
        public VendingService(ISlotRepository slotRepository, IMoneyRepository moneyRepository)
        {
            // input validation + exception
            if (slotRepository == null)
            {
                throw new ArgumentNullException("slotRepository");
            }
            if (moneyRepository == null)
            {
                throw new ArgumentNullException("moneyRepository");
            }

            // assign fields
            slotRepositoryInstance = slotRepository;
            moneyRepositoryInstance = moneyRepository;
            insertedAmount = 0;
        }

        // method - insert coins (count can be > 1)
        public void InsertCoin(CoinType coin, int count)
        {
            // count must be positive
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be > 0.");
            }

            // add to machine inventory
            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            inventory.Add(coin, count);

            // update inserted amount 
            insertedAmount = insertedAmount + ((int)coin * count);
        }

        // method - how much has been inserted
        public int GetInsertedAmount()
        {
            return insertedAmount;
        }

        // method - cancel and return all inserted coins as change
        public Dictionary<CoinType, int> CancelAndReturn()
        {
            if (insertedAmount == 0)
            {
                // return empty change
                Dictionary<CoinType, int> empty = new Dictionary<CoinType, int>();
                empty[CoinType.Twenty] = 0;
                empty[CoinType.Ten] = 0;
                empty[CoinType.Five] = 0;
                empty[CoinType.Two] = 0;
                empty[CoinType.One] = 0;
                return empty;
            }

            // try to make change equal to inserted amount
            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            Dictionary<CoinType, int> change = inventory.MakeChange(insertedAmount);

            // reset inserted amount after returning
            insertedAmount = 0;

            return change;
        }

        // method - buy product from a slot
        public PurchaseResult Purchase(string slotCode)
        {
            // slotCode must not be empty
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                throw new ArgumentException("Slot code must not be empty.", "slotCode");
            }

            // find the slot
            Slot slot = slotRepositoryInstance.GetByCode(slotCode);
            if (slot == null)
            {
                throw new ArgumentException("Slot not found.", "slotCode");
            }

            // check stock
            if (!slot.HasStock())
            {
                throw new OutOfStockException("Slot " + slot.Code + " is out of stock.");
            }

            // price to pay
            int price = slot.GetPrice();

            // check money
            if (insertedAmount < price)
            {
                throw new InvalidOperationException("Not enough money inserted.");
            }

            // compute change to return
            int changeValue = insertedAmount - price;

            // make change from inventory 
            Dictionary<CoinType, int> change;
            if (changeValue == 0)
            {
                // build empty change
                change = new Dictionary<CoinType, int>();
                change[CoinType.Twenty] = 0;
                change[CoinType.Ten] = 0;
                change[CoinType.Five] = 0;
                change[CoinType.Two] = 0;
                change[CoinType.One] = 0;
            }
            else
            {
                MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
                change = inventory.MakeChange(changeValue);
            }

            // remove one product from the slot
            slot.RemoveOne();
            slotRepositoryInstance.Update(slot);

            // reset inserted amount after purchase
            insertedAmount = 0;

            // return result with the product and the change
            PurchaseResult result = new PurchaseResult(slot.Product, change);
            return result;
        }

        // method - list slots 
        public List<Slot> GetAllSlots()
        {
            return slotRepositoryInstance.GetAll();
        }

        // method 
        public Slot GetSlotByCode(string slotCode)
        {
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                return null;
            }
            return slotRepositoryInstance.GetByCode(slotCode);
        }

        // method - check if purchase is possible 
        public bool CanPurchase(string slotCode)
        {
            
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                return false;
            }

            // find slot
            Slot slot = slotRepositoryInstance.GetByCode(slotCode);
            if (slot == null)
            {
                return false;
            }

            // stock check
            if (!slot.HasStock())
            {
                return false;
            }

            // money check
            int price = slot.GetPrice();
            if (insertedAmount < price)
            {
                return false;
            }

            // change check 
            int changeValue = insertedAmount - price;
            if (changeValue == 0)
            {
                return true;
            }

            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            return inventory.CanMakeChange(changeValue);
        }

        // method for refil
        public void RefillSlot(string slotCode, int count)
        {
            // input checks
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                throw new ArgumentException("Slot code must not be empty.", "slotCode");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }

            // find slot
            Slot slot = slotRepositoryInstance.GetByCode(slotCode);
            if (slot == null)
            {
                throw new ArgumentException("Slot not found.", "slotCode");
            }

            // do refill and persist
            slot.Refill(count);
            slotRepositoryInstance.Update(slot);
        }

        // method 
        public void SetSlotPriceOverride(string slotCode, int? newPrice)
        {
            // input checks
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                throw new ArgumentException("Slot code must not be empty.", "slotCode");
            }
            if (newPrice.HasValue && newPrice.Value < 0)
            {
                throw new ArgumentOutOfRangeException("newPrice", "Price must be ≥ 0.");
            }

            // find slot
            Slot slot = slotRepositoryInstance.GetByCode(slotCode);
            if (slot == null)
            {
                throw new ArgumentException("Slot not found.", "slotCode");
            }

            // set override and persist
            slot.SetPriceOverride(newPrice);
            slotRepositoryInstance.Update(slot);
        }

        // method - add coins
        public void AdminAddCoins(CoinType coin, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }

            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            inventory.Add(coin, count);
        }

        // method - remove coiiinnnnnnssss
        public void AdminRemoveCoins(CoinType coin, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }

            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            inventory.Remove(coin, count);
        }

        // method - admin: total cash inside the machine (DKK)
        public int GetMachineCashTotal()
        {
            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            return inventory.Total();
        }
    }
}
