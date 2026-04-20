namespace Sagunto.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; private set; }
        public decimal Total { get; private set; }
        public bool IsPaid { get; private set; } = false;
        public int UserId { get; private set; }
        public User? User { get; private set; }
        public ICollection<OrderLine> Lines { get; private set; }

        public Order() { }

        public void AddLine(OrderLine line) 
        { 
            ArgumentNullException.ThrowIfNull(line);

            Lines.Add(line);
            Total += line.PriceSnapshot;
        }
    }
}
