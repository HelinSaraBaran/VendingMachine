namespace VendingMachine.Domain
{

    // describes our products 
    public class ProductType
    {
        // Our properties and get/set (and set is private + oop)
        public int Id {  get; private set; }
        public string Name { get; private set; }
        public int BasePrice { get; private set; }
        public string Category { get; private set; }

        // our constructor 
        public ProductType(int id, string name, int baseprice, string category)
        {
            Id = id;
            Name = name;
            BasePrice = baseprice;
            Category = category;

        }
        // method 
        public string Describe()
        {
            return Name +" (" + BasePrice + " kr.)";
        }
        
    }

    public class Slot
    {
    // our properties for slot 
    public string Code { get; private set; }
    public int Capacity { get; private set; }
    public int Quantity {  get; private set; }
    public ProductType Product { get; private set; }

    // still a property but int? means it can be null
    public int? PriceOverride { get; private set; }

    // constructor 
    public Slot (string code, int capacity, int quantity, ProductType product,int? priceOverride)
        {
            Code = code;
            Capacity = capacity;
            Quantity = quantity;
            Product = product;
            PriceOverride = priceOverride;
        }

    }
    
    public enum CoinType
    {
    One = 1,
    Two = 2,
    Five = 5,
    Ten = 10,
    Twenty = 20
    
    }
    public class MoneyInventory
    {
        // property (coins) for our MoneyInventory 
        public Dictionary<CoinType, int> Coins{ get; private set; }

        // constructor 
        public MoneyInventory()
        {

        }

        // methods 
        public void Remove (CoinType type, int count)
        {

        }
        public int Total()
        {
            return 0;
        }

        // checks if it can give the amount back
        public bool CanMakeChange(int amount)
        {
            return false;
        }

        public Dictionary<CoinType,int> MakeChange(int amount)
        {
            return new Dictionary<CoinType, int>();
        }
    }
}
