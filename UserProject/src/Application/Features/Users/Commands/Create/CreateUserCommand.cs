using Application.Dtos;
using Application.Interfaces;
using Domain.Aggregates.UserAggregate;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands.Create
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    internal sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICachingService _cachingService;

        public CreateUserCommandHandler(IUserRepository userRepository, ICachingService cachingService)
        {
            _userRepository = userRepository;
            _cachingService = cachingService;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = User.Create(request.Name, request.Email);
            await _userRepository.AddAsync(user);

            await _cachingService.RemoveAsync("all_users_cache_key");

            return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
        }
    }
}
