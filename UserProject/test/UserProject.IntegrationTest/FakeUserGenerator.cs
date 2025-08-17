

// The collection definition ensures that the test fixture (and the database container)
// is only started once for all tests in this file.
using Application.Features.Users.Commands.Create;
using Bogus;

public class FakeUserGenerator
{
    private static readonly Faker<CreateUserCommand> _faker = new Faker<CreateUserCommand>()
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.Email, f => f.Person.Email);

    public static CreateUserCommand Generate() => _faker.Generate();
}
