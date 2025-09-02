using System;
using System.Collections.Generic;
using VendingMachine.Domain;
using VendingMachine.Infrastructure;
using VendingMachine.Service;

internal class Program
{
    // service instance used in the whole program
    private static VendingService vendingService;

    private static void Main(string[] args)
    {
        // products - name, price (DKK), category
        ProductType cola = new ProductType(1, "Cola", 15, "Drink");
        ProductType faxeKondi = new ProductType(2, "Faxe Kondi", 15, "Drink");
        ProductType iskaffe = new ProductType(3, "Iskaffe", 20, "Drink");
        ProductType chips = new ProductType(4, "Chips", 12, "Snack");
        ProductType musliBar = new ProductType(5, "Müsli-bar", 10, "Snack");
        ProductType snickers = new ProductType(6, "Snickers", 11, "Snack");
        ProductType water = new ProductType(7, "Vand", 10, "Drink");
        ProductType kitkat = new ProductType(8, "KitKat", 12, "Snack");

        // slots – code, capacity, quantity, product, priceOverride (null = use base price)
        List<Slot> slotList = new List<Slot>();
        slotList.Add(new Slot("A1", 10, 5, cola, null));
        slotList.Add(new Slot("A2", 10, 5, faxeKondi, null));
        slotList.Add(new Slot("A3", 10, 5, iskaffe, null));
        slotList.Add(new Slot("B1", 8, 5, chips, null));
        slotList.Add(new Slot("B2", 8, 5, musliBar, null));
        slotList.Add(new Slot("B3", 8, 5, snickers, null));
        slotList.Add(new Slot("C1", 8, 5, water, null));
        slotList.Add(new Slot("C2", 8, 5, kitkat, null));

        // repositories
        InMemorySlotRepository slotRepository = new InMemorySlotRepository(slotList);

        // money inventory – fill with coins so change can be given
        MoneyInventory moneyInventory = new MoneyInventory();
        moneyInventory.Add(CoinType.Twenty, 5);
        moneyInventory.Add(CoinType.Ten, 8);
        moneyInventory.Add(CoinType.Five, 10);
        moneyInventory.Add(CoinType.Two, 10);
        moneyInventory.Add(CoinType.One, 20);

        InMemoryMoneyRepository moneyRepository = new InMemoryMoneyRepository(moneyInventory);

        // service
        vendingService = new VendingService(slotRepository, moneyRepository);

        // run menu loop
        RunMenu();
    }

    // menu loop – accepts numbers, slot-codes and product names
    private static void RunMenu()
    {
        bool running = true;
        while (running)
        {
            // header + current balance
            Console.WriteLine();
            Console.WriteLine("==== Vending Machine ====");
            Console.WriteLine("Balance: " + vendingService.GetInsertedAmount() + " kr.");
            Console.WriteLine("1) Show products");
            Console.WriteLine("2) Insert coins");
            Console.WriteLine("3) Buy product");
            Console.WriteLine("4) Cancel and refund");
            Console.WriteLine("5) Show inserted amount");
            Console.WriteLine("9) Admin (refill/price/coins/total)");
            Console.WriteLine("Tip: type a slot code (e.g., A1) or a product name (e.g., cola) here to buy directly");
            Console.WriteLine("0) Exit");
            Console.Write("Choose or type code/name: ");
            string userInput = Console.ReadLine();
            Console.WriteLine();

            // empty input
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Console.WriteLine("Unknown choice.");
                continue;
            }

            // normalize input
            string trimmedInput = userInput.Trim();

            // support "buy A1" or "køb A1"
            string[] parts = trimmedInput.Split(' ');
            if (parts.Length == 2 &&
                (parts[0].Equals("buy", StringComparison.OrdinalIgnoreCase) ||
                 parts[0].Equals("køb", StringComparison.OrdinalIgnoreCase)))
            {
                string token = parts[1];

                // try as slot code
                string normalizedSlotCodeForBuy;
                if (TryNormalizeSlotCode(token, out normalizedSlotCodeForBuy))
                {
                    QuickBuy(normalizedSlotCodeForBuy);
                    continue;
                }

                // try as product name
                string codeFromNameForBuy;
                if (TryFindSlotCodeByProductName(token, out codeFromNameForBuy))
                {
                    QuickBuy(codeFromNameForBuy);
                    continue;
                }

                Console.WriteLine("Could not find product or slot '" + token + "'.");
                continue;
            }

            // direct slot code (A1, a1)
            string normalizedSlotCode;
            if (TryNormalizeSlotCode(trimmedInput, out normalizedSlotCode))
            {
                QuickBuy(normalizedSlotCode);
                continue;
            }

            // direct product name (cola, snickers, vand, kitkat)
            string codeFromProductName;
            if (TryFindSlotCodeByProductName(trimmedInput, out codeFromProductName))
            {
                QuickBuy(codeFromProductName);
                continue;
            }

            // normal menu numbers
            if (trimmedInput == "1")
            {
                ShowProducts();
            }
            else if (trimmedInput == "2")
            {
                InsertCoinsFlow();
            }
            else if (trimmedInput == "3")
            {
                PurchaseFlow(null);
            }
            else if (trimmedInput == "4")
            {
                RefundFlow();
            }
            else if (trimmedInput == "5")
            {
                Console.WriteLine("Inserted: " + vendingService.GetInsertedAmount() + " kr.");
            }
            else if (trimmedInput == "9")
            {
                AdminMenu();
            }
            else if (trimmedInput == "0")
            {
                running = false;
            }
            else
            {
                Console.WriteLine("Unknown choice.");
            }
        }
    }

    // quick buy – when we already know the slot code
    private static void QuickBuy(string slotCode)
    {
        Console.WriteLine("Selected: " + slotCode);
        PurchaseFlow(slotCode);
    }

    // show all products
    private static void ShowProducts()
    {
        List<Slot> allSlots = vendingService.GetAllSlots();
        Console.WriteLine("Code | Product      | Price | Qty | Status");
        for (int i = 0; i < allSlots.Count; i++)
        {
            Slot slotItem = allSlots[i];
            string status = slotItem.HasStock() ? "OK" : "Out";
            Console.WriteLine(
                slotItem.Code + "  | " + slotItem.Product.Name + " | " + slotItem.GetPrice() + " kr. | " + slotItem.Quantity + " | " + status
            );
        }
    }

    // insert coins
    private static void InsertCoinsFlow()
    {
        Console.WriteLine("Insert coins:");
        Console.WriteLine("1) 1 kr");
        Console.WriteLine("2) 2 kr");
        Console.WriteLine("5) 5 kr");
        Console.WriteLine("10) 10 kr");
        Console.WriteLine("20) 20 kr");
        Console.Write("Coin value: ");
        string coinValueText = Console.ReadLine();

        CoinType coinType;
        bool coinParsed = TryParseCoin(coinValueText, out coinType);
        if (!coinParsed)
        {
            Console.WriteLine("Invalid coin.");
            return;
        }

        Console.Write("Count: ");
        string countText = Console.ReadLine();
        int count;
        bool countParsed = int.TryParse(countText, out count);
        if (!countParsed)
        {
            Console.WriteLine("Invalid count.");
            return;
        }

        try
        {
            vendingService.InsertCoin(coinType, count);
            Console.WriteLine("Inserted: " + vendingService.GetInsertedAmount() + " kr.");
        }
        catch (Exception exception)
        {
            Console.WriteLine("Error: " + exception.Message);
        }
    }

    // purchase flow – supports preselected slot or asks the user
    private static void PurchaseFlow(string preselectedSlotCode)
    {
        string slotCode = preselectedSlotCode;

        // ask if not given
        if (string.IsNullOrWhiteSpace(slotCode))
        {
            Console.Write("Enter slot code (e.g., A1) or product name: ");
            slotCode = Console.ReadLine();
        }

        // normalize as slot or resolve by product name
        string normalizedSlotCode;
        if (!TryNormalizeSlotCode(slotCode, out normalizedSlotCode))
        {
            string codeFromName;
            if (TryFindSlotCodeByProductName(slotCode, out codeFromName))
            {
                normalizedSlotCode = codeFromName;
            }
            else
            {
                Console.WriteLine("Slot not found.");
                return;
            }
        }

        // load slot
        Slot slot = vendingService.GetSlotByCode(normalizedSlotCode);
        if (slot == null)
        {
            Console.WriteLine("Slot not found.");
            return;
        }

        // stock check
        if (!slot.HasStock())
        {
            Console.WriteLine("Out of stock.");
            return;
        }

        // price and balance
        int price = slot.GetPrice();
        int inserted = vendingService.GetInsertedAmount();
        if (inserted < price)
        {
            int missing = price - inserted;
            Console.WriteLine("Not enough money. Need " + missing + " kr. more.");
            return;
        }

        // can we make change?
        bool canPurchase = vendingService.CanPurchase(normalizedSlotCode);
        if (!canPurchase)
        {
            int changeValue = inserted - price;
            Console.WriteLine("Cannot provide correct change (" + changeValue + " kr). Try exact amount or different coins.");
            return;
        }

        // try to purchase
        try
        {
            PurchaseResult result = vendingService.Purchase(normalizedSlotCode);
            Console.WriteLine("Bought: " + result.Product.Name);

            // show remaining quantity
            int remaining = vendingService.GetSlotByCode(normalizedSlotCode).Quantity;
            Console.WriteLine("Remaining in " + normalizedSlotCode + ": " + remaining);
            if (remaining == 0)
            {
                Console.WriteLine("Note: Slot " + normalizedSlotCode + " is now EMPTY.");
            }

            Console.WriteLine("Change:");
            PrintChange(result.Change);
        }
        catch (Exception exception)
        {
            Console.WriteLine("Error: " + exception.Message);
        }
    }

    // cancel and refund all inserted money
    private static void RefundFlow()
    {
        try
        {
            Dictionary<CoinType, int> refunded = vendingService.CancelAndReturn();
            Console.WriteLine("Refunded:");
            PrintChange(refunded);
        }
        catch (Exception exception)
        {
            Console.WriteLine("Error: " + exception.Message);
        }
    }

    // parse coin from text
    private static bool TryParseCoin(string text, out CoinType coinType)
    {
        coinType = CoinType.One;

        if (text == "1")
        {
            coinType = CoinType.One;
            return true;
        }
        if (text == "2")
        {
            coinType = CoinType.Two;
            return true;
        }
        if (text == "5")
        {
            coinType = CoinType.Five;
            return true;
        }
        if (text == "10")
        {
            coinType = CoinType.Ten;
            return true;
        }
        if (text == "20")
        {
            coinType = CoinType.Twenty;
            return true;
        }
        return false;
    }

    // pretty print change (skip zeros)
    private static void PrintChange(Dictionary<CoinType, int> change)
    {
        bool printedAny = false;

        if (change[CoinType.Twenty] > 0)
        {
            Console.WriteLine("  20 kr: " + change[CoinType.Twenty]);
            printedAny = true;
        }
        if (change[CoinType.Ten] > 0)
        {
            Console.WriteLine("  10 kr: " + change[CoinType.Ten]);
            printedAny = true;
        }
        if (change[CoinType.Five] > 0)
        {
            Console.WriteLine("   5 kr: " + change[CoinType.Five]);
            printedAny = true;
        }
        if (change[CoinType.Two] > 0)
        {
            Console.WriteLine("   2 kr: " + change[CoinType.Two]);
            printedAny = true;
        }
        if (change[CoinType.One] > 0)
        {
            Console.WriteLine("   1 kr: " + change[CoinType.One]);
            printedAny = true;
        }

        if (!printedAny)
        {
            Console.WriteLine("  (no change)");
        }
    }

    // admin menu
    private static void AdminMenu()
    {
        bool adminRunning = true;
        while (adminRunning)
        {
            Console.WriteLine();
            Console.WriteLine("==== Admin ====");
            Console.WriteLine("1) Refill slot");
            Console.WriteLine("2) Set/Clear price override");
            Console.WriteLine("3) Add coins to machine");
            Console.WriteLine("4) Remove coins from machine");
            Console.WriteLine("5) Show machine cash total");
            Console.WriteLine("6) Empty cash box");
            Console.WriteLine("7) Refill slot to capacity");
            Console.WriteLine("8) Refill ALL slots to capacity");
            Console.WriteLine("9) Top up coin float to targets");
            Console.WriteLine("0) Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Slot code: ");
                string code = Console.ReadLine();
                Console.Write("Count: ");
                string countText = Console.ReadLine();
                int count;
                bool ok = int.TryParse(countText, out count);
                if (!ok)
                {
                    Console.WriteLine("Invalid count.");
                    continue;
                }
                try
                {
                    vendingService.RefillSlot(code, count);
                    Console.WriteLine("Refilled.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "2")
            {
                Console.Write("Slot code: ");
                string code = Console.ReadLine();
                Console.Write("New price (or empty to clear): ");
                string priceText = Console.ReadLine();
                try
                {
                    if (string.IsNullOrWhiteSpace(priceText))
                    {
                        vendingService.SetSlotPriceOverride(code, null);
                        Console.WriteLine("Price override cleared.");
                    }
                    else
                    {
                        int newPrice;
                        bool ok = int.TryParse(priceText, out newPrice);
                        if (!ok)
                        {
                            Console.WriteLine("Invalid price.");
                            continue;
                        }
                        vendingService.SetSlotPriceOverride(code, newPrice);
                        Console.WriteLine("Price override set to " + newPrice + " kr.");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "3")
            {
                Console.Write("Coin to add (1,2,5,10,20): ");
                string coinText = Console.ReadLine();
                CoinType coinType;
                bool coinOk = TryParseCoin(coinText, out coinType);
                if (!coinOk)
                {
                    Console.WriteLine("Invalid coin.");
                    continue;
                }
                Console.Write("Count: ");
                string countText = Console.ReadLine();
                int count;
                bool ok = int.TryParse(countText, out count);
                if (!ok)
                {
                    Console.WriteLine("Invalid count.");
                    continue;
                }
                try
                {
                    vendingService.AdminAddCoins(coinType, count);
                    Console.WriteLine("Coins added.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "4")
            {
                Console.Write("Coin to remove (1,2,5,10,20): ");
                string coinText = Console.ReadLine();
                CoinType coinType;
                bool coinOk = TryParseCoin(coinText, out coinType);
                if (!coinOk)
                {
                    Console.WriteLine("Invalid coin.");
                    continue;
                }
                Console.Write("Count: ");
                string countText = Console.ReadLine();
                int count;
                bool ok = int.TryParse(countText, out count);
                if (!ok)
                {
                    Console.WriteLine("Invalid count.");
                    continue;
                }
                try
                {
                    vendingService.AdminRemoveCoins(coinType, count);
                    Console.WriteLine("Coins removed.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "5")
            {
                int total = vendingService.GetMachineCashTotal();
                Console.WriteLine("Machine cash total: " + total + " kr.");
            }
            else if (choice == "6")
            {
                try
                {
                    Dictionary<CoinType, int> removed = vendingService.AdminEmptyCash();
                    Console.WriteLine("Removed:");
                    PrintChange(removed);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "7")
            {
                Console.Write("Slot code: ");
                string slotCode = Console.ReadLine();
                try
                {
                    int added = vendingService.RefillSlotToCapacity(slotCode);
                    Console.WriteLine("Added " + added + " items to " + slotCode + ".");
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "8")
            {
                try
                {
                    int totalAdded = vendingService.RefillAllSlotsToCapacity();
                    Console.WriteLine("Added total " + totalAdded + " items across all slots.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
            }
            else if (choice == "9")
            {
                Console.Write("Target 20-kr coins: ");
                string t20 = Console.ReadLine();
                Console.Write("Target 10-kr coins: ");
                string t10 = Console.ReadLine();
                Console.Write("Target 5-kr coins: ");
                string t5 = Console.ReadLine();
                Console.Write("Target 2-kr coins: ");
                string t2 = Console.ReadLine();
                Console.Write("Target 1-kr coins: ");
                string t1 = Console.ReadLine();

                int targetTwenty;
                int targetTen;
                int targetFive;
                int targetTwo;
                int targetOne;

                bool ok20 = int.TryParse(t20, out targetTwenty);
                bool ok10 = int.TryParse(t10, out targetTen);
                bool ok5 = int.TryParse(t5, out targetFive);
                bool ok2 = int.TryParse(t2, out targetTwo);
                bool ok1 = int.TryParse(t1, out targetOne);

                if (!ok20 || !ok10 || !ok5 || !ok2 || !ok1)
                {
                    Console.WriteLine("Invalid targets.");
                }
                else
                {
                    try
                    {
                        vendingService.AdminTopUpCoinFloat(targetTwenty, targetTen, targetFive, targetTwo, targetOne);
                        Console.WriteLine("Coin float topped up to targets.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Error: " + exception.Message);
                    }
                }
            }
            else if (choice == "0")
            {
                adminRunning = false;
            }
            else
            {
                Console.WriteLine("Unknown choice.");
            }
        }
    }

    // normalize a slot code (A1, a1 -> A1) and verify existence
    private static bool TryNormalizeSlotCode(string input, out string normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string trimmed = input.Trim();
        string upper = trimmed.ToUpperInvariant();

        // accept 2-3 characters ( A1 or B12)
        if (upper.Length < 2 || upper.Length > 3)
        {
            return false;
        }

        // check if slot exists
        Slot slot = vendingService.GetSlotByCode(upper);
        if (slot == null)
        {
            return false;
        }

        normalized = upper;
        return true;
    }

    // find slot code by product name
    private static bool TryFindSlotCodeByProductName(string nameText, out string slotCode)
    {
        slotCode = null;

        if (string.IsNullOrWhiteSpace(nameText))
        {
            return false;
        }

        string needle = nameText.Trim().ToLowerInvariant();

        List<Slot> allSlots = vendingService.GetAllSlots();

        // full match first
        for (int i = 0; i < allSlots.Count; i++)
        {
            string productName = allSlots[i].Product.Name;
            if (productName != null && productName.ToLowerInvariant() == needle)
            {
                slotCode = allSlots[i].Code;
                return true;
            }
        }

        // starts with match as a fallback
        for (int i = 0; i < allSlots.Count; i++)
        {
            string productName = allSlots[i].Product.Name;
            if (productName != null && productName.ToLowerInvariant().StartsWith(needle))
            {
                slotCode = allSlots[i].Code;
                return true;
            }
        }

        return false;
    }
}
