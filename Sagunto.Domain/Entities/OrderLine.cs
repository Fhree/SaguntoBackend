namespace Sagunto.Domain.Entities
{

    public class OrderLine
    {
        public int Id { get; private set; }
        public int Quantity { get; private set; }
        public decimal PriceSnapshot { get; private set; }
        public int OrderId { get; private set; }
        public Order? Order { get; private set; }
        public int ProductId { get; private set; }
        public Product? Product { get; private set; }

        private OrderLine() { }

        public OrderLine(int orderId, int productId, int quantity, decimal unitPriceSnapshot)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            if (unitPriceSnapshot < 0)
                throw new ArgumentException("Price snapshot cannot be negative.", nameof(unitPriceSnapshot));

            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            PriceSnapshot = unitPriceSnapshot;
        }
    }
}