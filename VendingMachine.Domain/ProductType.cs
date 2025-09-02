namespace VendingMachine.Domain
{

    // describes our products 
    public class ProductType
    {
        // Our properties and get/set (and set is private + oop)
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int BasePrice { get; private set; }
        public string Category { get; private set; }

        // our constructor 
        public ProductType(int id, string name, int baseprice, string category)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id", "Id must be > 0.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name must not be empty.", "name");
            }
            if (baseprice < 0)
            {
                throw new ArgumentOutOfRangeException("basePrice", "Price must be ≥ 0.");
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category must not be empty.", "category");
            }

            Id = id;
            Name = name;
            BasePrice = baseprice;
            Category = category;

        }
        // method 
        public string Describe()
        {
            return Name + " (" + BasePrice + " kr.)";
        }
        public override string ToString()
        {
            return "ProductType { Id=" + Id + ", Name=" + Name + ", Price=" + BasePrice + " kr., Category=" + Category + " }";
        }
    }



}
