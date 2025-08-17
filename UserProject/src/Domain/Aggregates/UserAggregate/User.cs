using Domain.Common;
using System.Text.Json.Serialization;

namespace Domain.Aggregates.UserAggregate
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        private User() : base() { }

        [JsonConstructor]
        public User(Guid id, string name, string email) : base(id)
        {
            Name = name;
            Email = email;
        }

        public static User Create(Guid id, string name, string email)
        {
            // The base class handles ID validation.
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            return new User(id, name, email);
        }

        public void Update(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            Name = name;
            Email = email;
        }
    }
}
