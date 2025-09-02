using System;
using System.Collections.Generic;

namespace VendingMachine.Service
{
    using VendingMachine.Domain;
    using VendingMachine.Infrastructure;

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

        // method 
        public int GetMachineCashTotal()
        {
            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();
            return inventory.Total();
        }
        // method - admin: refill a slot to its full capacity, returns how many items were added
        public int RefillSlotToCapacity(string slotCode)
        {
            if (string.IsNullOrWhiteSpace(slotCode))
            {
                throw new ArgumentException("Slot code must not be empty.", "slotCode");
            }

            Slot slot = slotRepositoryInstance.GetByCode(slotCode);
            if (slot == null)
            {
                throw new ArgumentException("Slot not found.", "slotCode");
            }

            int added = slot.RefillToCapacity();
            slotRepositoryInstance.Update(slot);
            return added;
        }

        // method - admin refill all slots to capacity, returns total items added
        public int RefillAllSlotsToCapacity()
        {
            List<Slot> allSlots = slotRepositoryInstance.GetAll();
            int totalAdded = 0;

            for (int slotIndex = 0; slotIndex < allSlots.Count; slotIndex = slotIndex + 1)
            {
                Slot slot = allSlots[slotIndex];
                int added = slot.RefillToCapacity();
                totalAdded = totalAdded + added;
                slotRepositoryInstance.Update(slot);
            }

            return totalAdded;
        }

        // method - admin top up coin float to target levels (ensure minimum counts for each coin)
        public void AdminTopUpCoinFloat(int targetTwenty, int targetTen, int targetFive, int targetTwo, int targetOne)
        {
            if (targetTwenty < 0 || targetTen < 0 || targetFive < 0 || targetTwo < 0 || targetOne < 0)
            {
                throw new ArgumentOutOfRangeException("Targets must be ≥ 0.");
            }

            MoneyInventory moneyInventory = moneyRepositoryInstance.GetInventory();

            EnsureCoinLevel(moneyInventory, CoinType.Twenty, targetTwenty);
            EnsureCoinLevel(moneyInventory, CoinType.Ten, targetTen);
            EnsureCoinLevel(moneyInventory, CoinType.Five, targetFive);
            EnsureCoinLevel(moneyInventory, CoinType.Two, targetTwo);
            EnsureCoinLevel(moneyInventory, CoinType.One, targetOne);
        }

        // helper - make sure a coin count is at least target
        private void EnsureCoinLevel(MoneyInventory moneyInventory, CoinType coinType, int targetCount)
        {
            int current = moneyInventory.Coins[coinType];
            if (current < targetCount)
            {
                int missing = targetCount - current;
                moneyInventory.Add(coinType, missing);
            }
        }
        // method - admin empty all cash and return what was removed
        public Dictionary<CoinType, int> AdminEmptyCash()
        {
            MoneyInventory inventory = moneyRepositoryInstance.GetInventory();

            Dictionary<CoinType, int> removed = new Dictionary<CoinType, int>();
            removed[CoinType.Twenty] = inventory.Coins[CoinType.Twenty];
            removed[CoinType.Ten] = inventory.Coins[CoinType.Ten];
            removed[CoinType.Five] = inventory.Coins[CoinType.Five];
            removed[CoinType.Two] = inventory.Coins[CoinType.Two];
            removed[CoinType.One] = inventory.Coins[CoinType.One];

            if (removed[CoinType.Twenty] > 0) { inventory.Remove(CoinType.Twenty, removed[CoinType.Twenty]); }
            if (removed[CoinType.Ten] > 0) { inventory.Remove(CoinType.Ten, removed[CoinType.Ten]); }
            if (removed[CoinType.Five] > 0) { inventory.Remove(CoinType.Five, removed[CoinType.Five]); }
            if (removed[CoinType.Two] > 0) { inventory.Remove(CoinType.Two, removed[CoinType.Two]); }
            if (removed[CoinType.One] > 0) { inventory.Remove(CoinType.One, removed[CoinType.One]); }

            return removed;
        }

    }
}
