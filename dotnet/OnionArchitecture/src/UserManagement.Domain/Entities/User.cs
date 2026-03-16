using UserManagement.Domain.ValueObjects;

namespace UserManagement.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string UsernameLowercase { get; set; }

        public Email? Email { get; set; }

        public string? HashedPassword { get; set; }

        public bool IsActive { get; set; }
    }
}