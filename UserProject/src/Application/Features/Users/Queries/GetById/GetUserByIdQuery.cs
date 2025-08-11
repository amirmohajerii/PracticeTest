using Application.Dtos;
using Application.Interfaces;
using Domain.Aggregates.UserAggregate;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries.GetById
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public Guid Id { get; set; }
    }

    internal sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICachingService _cachingService;

        public GetUserByIdQueryHandler(IUserRepository userRepository, ICachingService cachingService)
        {
            _userRepository = userRepository;
            _cachingService = cachingService;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"user_{request.Id}";
            var cachedUser = await _cachingService.GetAsync<User>(cacheKey);

            if (cachedUser != null)
            {
                return new UserDto { Id = cachedUser.Id, Name = cachedUser.Name, Email = cachedUser.Email };
            }

            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return null;
            }

            await _cachingService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));

            return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
        }
    }
}
