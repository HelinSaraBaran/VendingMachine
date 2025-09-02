using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VendingMachine.Domain
{
    public class MoneyInventory
    {
        // property - dictionary keeps count for each cointype
        public Dictionary<CoinType, int> Coins { get; private set; }

        // constructor - initializes dictionary 
        public MoneyInventory()
        {
            // empty dictionary- each key(cointype) and value (int) saves new dictionary in property
            Coins = new Dictionary<CoinType, int>();

            // Ensuring coin types exists from start
            Coins[CoinType.One] = 0;
            Coins[CoinType.Two] = 0;
            Coins[CoinType.Five] = 0;
            Coins[CoinType.Ten] = 0;
            Coins[CoinType.Twenty] = 0;

        }

        // methods 
        public void Add(CoinType type, int count)
        {  // checks the number of coins is not negative
            if (count < 0)
            {   // if count is - it throws an exception for the invalid input
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }
            // updates dictionary, takes current value of coin (type) and adds the new count. 
            Coins[type] = Coins[type] + count;
        }

        // method for coin removal from inventory 
        // samme princip som tidligere metode også
        public void Remove(CoinType type, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count must be ≥ 0.");
            }
            if (Coins[type] < count)
            {
                throw new InvalidOperationException("Not enough coins in inventory");
            }
            Coins[type] = Coins[type] - count;

        }
        // Calculates total of all coins/moneeeyyyyy
        public int Total()
        {
            int sum = 0;
            sum = sum + (Coins[CoinType.One]*1);
            sum = sum + (Coins[CoinType.Two] * 2);
            sum = sum + (Coins[CoinType.Five] * 5);
            sum = sum + (Coins[CoinType.Ten] * 10);
            sum = sum + (Coins[CoinType.Twenty] * 20);
            return sum;
        }

        // checks possibility to give change back
        public bool CanMakeChange(int amount)
        {   // if the requested amount is -, return false. 
            if (amount < 0)
            {
                return false;
            }
            // Defines coin values (largest to smallest).
            int[] values = new int[] { 20, 10,5,2,1 };

            // "pretending" to use coins without changing the real coins 
            int[] tempCounts = new int[5];

            // how many 20 kr do we have? and so on for the rest
            tempCounts[0] = Coins[CoinType.Twenty]; 
            tempCounts[1] = Coins[CoinType.Ten];
            tempCounts[2] = Coins[CoinType.Five];
            tempCounts[3] = Coins[CoinType.Two];
            tempCounts[4] = Coins[CoinType.One];

            // rest = how much change we might still need to cover
            int rest = amount;

            // always try biggest coin first -> greedy algorithm 
            for (int i = 0; i < values.Length; i++)
            {
                // Current coin value (20, 10, 5, 2, 1)
                int denomValue = values[i];

                // how many coins are available
                int available = tempCounts[i];

                // How many coins this type could be used the most?
                int maxByValue = rest / denomValue;

                // takes minimum of what we need and have
                int use = maxByValue <available ? maxByValue : available;

                // Subtracts the value of the coins we used from the rest
                rest = rest - (use * denomValue);

                // updates temporary count
                tempCounts[i] = tempCounts[i] - use;

                // if rest == 0, sucess ( amount covered)
                if (rest == 0)
                {
                    return true;
                }
            }
            // After all coins have been tried, return true if rest is 0
            return rest == 0;


        }

        public Dictionary<CoinType, int> MakeChange(int amount)
        {
            if (amount < 0) 
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be ≥ 0.");
            }

            // Result dictionary to show how many coins were returned 
            Dictionary<CoinType, int> result = new Dictionary<CoinType, int>();
            result[CoinType.Twenty] = 0;
            result[CoinType.Ten] = 0;
            result[CoinType.Five] = 0;
            result[CoinType.Two] = 0;
            result[CoinType.One] = 0;

            int[] values = new int[] {20, 10, 5, 2, 1 };

            int rest = amount;

            // Uses largest coins first 
            for (int i = 0; i < values.Length; i++)
            {
                int denomValue = values[i];
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
                    // reduces real inventory
                    Coins[denom] = Coins[denom] - use;

                    // records given coins 
                    result[denom] = result[denom] + use;

                    rest = rest - (use * denomValue);

                }
                if (rest == 0)
                {
                    break;
                }

            }
            if (rest > 0) 
            {
                throw new ChangeNotAvailableException("Cannot provide correct change.");
            }
            return result;
        }
    }

}
