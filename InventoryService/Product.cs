namespace InventoryService
{
    public class Product
    {
        private Product() { }

        public Product(string code, string name, double price = 0.0, int quantity = 0)
        {
            this.Code = code;
            this.Name = name;
            this.Price = price;
            this.Quantity = quantity;
        }

        public int Id { get; set; }
        
        public string Code { get; set; }
        
        public string Name { get; set; }

        private double _price;
        public double Price
        {
            get => _price;
            set
            {
                if (value < 0) return;
                _price = value;
            }
        }

        private int _quantity;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0) return;
                _quantity = value;
            }
        }

        public bool IsValid()
        {
            return Price >= 0 && Quantity >= 0;
        }

        public override string ToString()
        {
            return $"Há {Quantity} unidades do produto {Name} - {Code}, cujo preço é de R$ {Price}";
        }
    }
}
