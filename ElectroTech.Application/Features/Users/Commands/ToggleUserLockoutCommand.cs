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
    public class ToggleUserLockoutCommand : IRequest<IResult<bool>>
    {
        public string Id { get; set; }

        public class Handler : IRequestHandler<ToggleUserLockoutCommand, IResult<bool>>
        {
            private readonly IUserRepository _repo;
            public Handler(IUserRepository repo) => _repo = repo;

            public async Task<IResult<bool>> Handle(
                ToggleUserLockoutCommand cmd, CancellationToken ct)
            {
                var result = await _repo.ToggleLockoutAsync(cmd.Id);
                return result
                    ? await Result<bool>.SuccessAsync(true)
                    : await Result<bool>.FailAsync("Không thể cập nhật.");
            }
        }
    }
}
