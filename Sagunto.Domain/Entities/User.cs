using System.Globalization;
using System.Text;

namespace Sagunto.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string FirebaseUid { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string? Surname { get; private set; }
        public string SaguntinoCode { get; private set; } = string.Empty;
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }
        public string NormalizedSearch { get; private set; } = string.Empty;

        private User() { }

        public User(string firebaseUid, string email, string name, int roleId, string saguntinoCode, string? surname)
        {
            FirebaseUid = firebaseUid;
            Email = email;
            Name = name;
            Surname = string.IsNullOrEmpty(surname) ? null : surname;
            SaguntinoCode = saguntinoCode;
            RoleId = roleId;
            UpdateNormalizedSearch();
        }

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