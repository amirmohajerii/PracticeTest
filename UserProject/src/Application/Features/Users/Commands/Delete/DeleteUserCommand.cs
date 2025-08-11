using Application.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands.Delete
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICachingService _cachingService;

        public DeleteUserCommandHandler(IUserRepository userRepository, ICachingService cachingService)
        {
            _userRepository = userRepository;
            _cachingService = cachingService;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);

            await _cachingService.RemoveAsync($"user_{user.Id}");
            await _cachingService.RemoveAsync("all_users_cache_key");

            return true;
        }
    }
}
