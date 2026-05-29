namespace Sagunto.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        private Role() { }

        public Role(string name) 
        { 
            Name = name;
        }
    }
}
