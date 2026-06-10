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

    public class ToggleUserActiveCommand : IRequest<IResult<bool>>
    {
        public string Id { get; set; }

        public class Handler : IRequestHandler<ToggleUserActiveCommand, IResult<bool>>
        {
            private readonly IUserRepository _repo;
            public Handler(IUserRepository repo) => _repo = repo;

            public async Task<IResult<bool>> Handle(
                ToggleUserActiveCommand cmd, CancellationToken ct)
            {
                var result = await _repo.ToggleActiveAsync(cmd.Id);
                return result
                    ? await Result<bool>.SuccessAsync(true)
                    : await Result<bool>.FailAsync("Không thể cập nhật.");
            }
        }
    }
}
