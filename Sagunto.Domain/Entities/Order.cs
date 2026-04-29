namespace Sagunto.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; private set; }
        public decimal Total { get; private set; }
        public bool IsPaid { get; private set; } = false;
        public int? CustomerId { get; private set; }
        public int UserId { get; private set; }
        public User? User { get; private set; }
        public ICollection<OrderLine> Lines { get; private set; }

        public Order(decimal total, bool isPaid, int userId, int? customerId) 
        { 
            CreatedAt = DateTime.UtcNow;
            Total = total;
            IsPaid = isPaid;
            UserId = userId;
            CustomerId = customerId;
            Lines = [];
        }

        public void AddLine(OrderLine line) 
        { 
            ArgumentNullException.ThrowIfNull(line);

            Lines.Add(line);
            Total += line.PriceSnapshot;
        }

        public void Pay() 
        {
            IsPaid = true;
        }
    }
}
