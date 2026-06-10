using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Users.Commands
{
    public class AssignRoleCommand : IRequest<IResult<bool>>
    {
        public string Id { get; set; }
        public string Role { get; set; }

        public class Handler : IRequestHandler<AssignRoleCommand, IResult<bool>>
        {
            private readonly IUserRepository _repo;
            public Handler(IUserRepository repo) => _repo = repo;

            public async Task<IResult<bool>> Handle(
                AssignRoleCommand cmd, CancellationToken ct)
            {
                var result = await _repo.AssignRoleAsync(cmd.Id, cmd.Role);
                return result
                    ? await Result<bool>.SuccessAsync(true, "Đã gán quyền.")
                    : await Result<bool>.FailAsync("Không thể gán quyền.");
            }
        }
    }
}
