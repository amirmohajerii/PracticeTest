using Domain.Common;

namespace Domain.Aggregates.UserAggregate
{

    public class User : Entity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        public User(string name, string email)
        {
            Name = name;
            Email = email;
        }
        private User() { }

        public static User Create(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (!email.Contains("@") || !email.Contains(".")) throw new ArgumentException("Invalid Email Address.", nameof(email));
            return new User(name, email);
        }

        public void Update(string name, string email)
        {
            // Add validation logic here before updating
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (!email.Contains("@") || !email.Contains(".")) throw new ArgumentException("Invalid Email Address.", nameof(email));

            Name = name;
            Email = email;
        }
    }
}
