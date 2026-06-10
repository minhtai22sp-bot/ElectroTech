using AspNetCoreHero.Results;
using Entities.ViewModel;
using Interfaces;
using Library;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Users.Queries
{
    public class GetAllPaginatedUsersQuery : IRequest<IResult<PaginatedList<UserIndexModel>>>
    {
        public UserSearch model { get; set; }

        public class Handler
            : IRequestHandler<GetAllPaginatedUsersQuery,
                IResult<PaginatedList<UserIndexModel>>>
        {
            private readonly IUserRepository _repo;

            public Handler(IUserRepository repo) => _repo = repo;

            public async Task<IResult<PaginatedList<UserIndexModel>>> Handle(
                GetAllPaginatedUsersQuery request, CancellationToken ct)
            {
                request.model ??= new UserSearch();
                var data = await _repo.GetAllPaginatedAsync(request.model);
                return await Result<PaginatedList<UserIndexModel>>.SuccessAsync(data);
            }
        }
    }
}
