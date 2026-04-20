namespace Sagunto.Domain.Entities
{
    public class Product
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public decimal PriceMember { get; private set; }
        public decimal PriceGuest { get; private set; }

        private Product() { }

        public Product(string name, decimal priceMember, decimal priceGuest)
        {
            Name = name;
            PriceMember = priceMember;
            PriceGuest = priceGuest;
        }

        public void UpdatePrices(decimal priceMember, decimal priceGuest)
        {
            PriceMember = priceMember;
            PriceGuest = priceGuest;
        }
    }
}
