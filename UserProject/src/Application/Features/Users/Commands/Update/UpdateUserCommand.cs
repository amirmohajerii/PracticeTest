using Application.Dtos;
using Application.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands.Update
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    internal sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICachingService _cachingService;

        public UpdateUserCommandHandler(IUserRepository userRepository, ICachingService cachingService)
        {
            _userRepository = userRepository;
            _cachingService = cachingService;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return null;
            }

            user.Update(request.Name, request.Email);
            await _userRepository.UpdateAsync(user);

            await _cachingService.RemoveAsync($"user_{user.Id}");
            await _cachingService.RemoveAsync("all_users_cache_key");

            return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
        }
    }
}
