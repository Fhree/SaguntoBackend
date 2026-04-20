namespace Sagunto.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Role() { }

        public Role(string name) 
        { 
            Name = name;
        }
    }
}
