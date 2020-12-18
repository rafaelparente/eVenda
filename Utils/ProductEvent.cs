namespace Utils
{
    public class ProductEvent
    {
        private ProductEvent() { }

        public ProductEvent(ProductEventType eventType, Product product)
        {
            this.EventType = eventType;
            this.Product = product;
        }
        
        public ProductEventType EventType { get; }
        
        public Product Product { get; }
    }
}
