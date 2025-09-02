using System;
using System.Collections.Generic;

namespace VendingMachine.Domain
{
    public class MoneyInventory
    {
        // property - dictionary keeps count for each coin type
        public Dictionary<CoinType, int> Coins { get; private set; }

        // constructor - initializes dictionary 
        public MoneyInventory()
        {
            // empty dictionary, each key (coin type) and value (int)
            Coins = new Dictionary<CoinType, int>();

            // ensures all coin types exist from start
            Coins[CoinType.One] = 0;
            Coins[CoinType.Two] = 0;
            Coins[CoinType.Five] = 0;
            Coins[CoinType.Ten] = 0;
            Coins[CoinType.Twenty] = 0;
        }

        // method - add coins to inventory
        public void Add(CoinType type, int count)
        {
            // count cannot be negative
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }
            // update dictionary
            Coins[type] = Coins[type] + count;
        }

        // method - remove coins from inventory
        public void Remove(CoinType type, int count)
        {
            // count cannot be negative
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }
            // check if enough coins exist
            if (Coins[type] < count)
            {
                throw new InvalidOperationException("Not enough coins in inventory");
            }
            // subtract from dictionary
            Coins[type] = Coins[type] - count;
        }

        // method - calculate total amount in DKK
        public int Total()
        {
            int sum = 0;
            sum = sum + (Coins[CoinType.One] * 1);
            sum = sum + (Coins[CoinType.Two] * 2);
            sum = sum + (Coins[CoinType.Five] * 5);
            sum = sum + (Coins[CoinType.Ten] * 10);
            sum = sum + (Coins[CoinType.Twenty] * 20);
            return sum;
        }

        // method - check if we can give correct change
        public bool CanMakeChange(int amount)
        {
            // if negative, return false
            if (amount < 0)
            {
                return false;
            }

            // coin values, largest to smallest
            int[] values = new int[] { 20, 10, 5, 2, 1 };

            // temporary copy of coin counts
            int[] tempCounts = new int[5];
            tempCounts[0] = Coins[CoinType.Twenty];
            tempCounts[1] = Coins[CoinType.Ten];
            tempCounts[2] = Coins[CoinType.Five];
            tempCounts[3] = Coins[CoinType.Two];
            tempCounts[4] = Coins[CoinType.One];

            // rest is amount we still need to cover
            int rest = amount;

            // greedy algorithm, use biggest coin first
            for (int i = 0; i < values.Length; i++)
            {
                int denomValue = values[i];
                int available = tempCounts[i];
                int maxByValue = rest / denomValue;

                // use minimum of what we need and what we have
                int use = maxByValue < available ? maxByValue : available;

                // subtract from rest
                rest = rest - (use * denomValue);

                // update temporary count
                tempCounts[i] = tempCounts[i] - use;

                // if nothing left, success
                if (rest == 0)
                {
                    return true;
                }
            }

            // return true only if we covered all
            return rest == 0;
        }

        // method - actually give change and update real inventory
        public Dictionary<CoinType, int> MakeChange(int amount)
        {
            // amount cannot be negative
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be ≥ 0.");
            }

            // result dictionary for coins we return
            Dictionary<CoinType, int> result = new Dictionary<CoinType, int>();
            result[CoinType.Twenty] = 0;
            result[CoinType.Ten] = 0;
            result[CoinType.Five] = 0;
            result[CoinType.Two] = 0;
            result[CoinType.One] = 0;

            int[] values = new int[] { 20, 10, 5, 2, 1 };
            int rest = amount;

            // greedy algorithm
            for (int i = 0; i < values.Length; i++)
            {
                int denomValue = values[i];

                // map value to enum
                CoinType denom =
                    denomValue == 20 ? CoinType.Twenty :
                    denomValue == 10 ? CoinType.Ten :
                    denomValue == 5 ? CoinType.Five :
                    denomValue == 2 ? CoinType.Two :
                    CoinType.One;

                int available = Coins[denom];
                int maxByValue = rest / denomValue;
                int use = maxByValue < available ? maxByValue : available;

                if (use > 0)
                {
                    // subtract from real inventory
                    Coins[denom] = Coins[denom] - use;

                    // record returned coins
                    result[denom] = result[denom] + use;

                    // subtract value from rest
                    rest = rest - (use * denomValue);
                }

                // if rest is 0, break out
                if (rest == 0)
                {
                    break;
                }
            }

            // if we still owe change, throw exception
            if (rest > 0)
            {
                throw new ChangeNotAvailableException("Cannot provide correct change.");
            }

            return result;
        }
    }
}
