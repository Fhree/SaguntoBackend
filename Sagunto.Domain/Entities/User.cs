namespace Sagunto.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Surname { get; private set; }
        public string SaguntinoCode { get; private set; } = string.Empty;
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        private User() { }

        public User(string name, int roleId, string saguntinoCode,string? surname) 
        {
            Name = name;
            Surname = string.IsNullOrEmpty(surname) ? null : surname;
            SaguntinoCode = saguntinoCode;
            RoleId = roleId;
        }

        public void ChangeRole(Role newRole) 
        {
            ArgumentNullException.ThrowIfNull(newRole);

            RoleId = newRole.Id;
            Role = newRole;
        }
    }
}
