using Application.Dtos;
using Application.Interfaces;
using Domain.Aggregates.UserAggregate;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users.Queries.GetAll
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public GetAllUsersQuery() { }
    }

    internal sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICachingService _cachingService;

        public GetAllUsersQueryHandler(IUserRepository userRepository, ICachingService cachingService)
        {
            _userRepository = userRepository;
            _cachingService = cachingService;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = "all_users_cache_key";
            var cachedUsers = await _cachingService.GetAsync<IEnumerable<User>>(cacheKey);

            if (cachedUsers != null)
            {
                return cachedUsers.Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email });
            }

            var users = await _userRepository.GetAllAsync();
            if (users == null)
            {
                return Enumerable.Empty<UserDto>();
            }

            await _cachingService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(5));

            return users.Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email });
        }
    }
}
