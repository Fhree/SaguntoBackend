using System.Globalization;
using System.Text;

namespace Sagunto.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string? FirebaseUid { get; private set; }
        public string? Email { get; private set; }
        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string SaguntinoCode { get; private set; }
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }
        public string NormalizedSearch { get; private set; }

        private User() { }

        public User(string firebaseUid, string email, string name, int roleId, string saguntinoCode, string surname)
        {
            FirebaseUid = firebaseUid;
            Email = email;
            Name = name;
            Surname = surname;
            SaguntinoCode = saguntinoCode;
            RoleId = roleId;
            UpdateNormalizedSearch();
        }

        public User(string name, int roleId, string saguntinoCode, string surname) : this(null, null, name, roleId, saguntinoCode, surname){}

        public void ChangeRole(Role newRole)
        {
            ArgumentNullException.ThrowIfNull(newRole);
            RoleId = newRole.Id;
            Role = newRole;
        }

        private void UpdateNormalizedSearch()
        {
            var rawString = $"{Name} {Surname}".Trim();
            var normalizedString = rawString.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            NormalizedSearch = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }
    }
}