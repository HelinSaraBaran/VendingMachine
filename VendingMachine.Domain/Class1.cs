namespace VendingMachine.Domain
{

    // describes our products 
    public class ProductType
    {
        // Our properties and get/set (and set is private oop)
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
            return Name +" (" + BasePrice + "kr.)";
        }
        


    }
}
