using AspNetCoreHero.Results;
using Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ElectroTech.Application.Features.Auth.Commands
{
    public class RegisterCommand : IRequest<IResult<string>>
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public class RegisterCommandHandler
            : IRequestHandler<RegisterCommand, IResult<string>>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<ApplicationRole> _roleManager;

            public RegisterCommandHandler(
                UserManager<ApplicationUser> userManager,
                RoleManager<ApplicationRole> roleManager)
            {
                _userManager = userManager;
                _roleManager = roleManager;
            }

            public async Task<IResult<string>> Handle(
                RegisterCommand command, CancellationToken cancellationToken)
            {
                if (command.Password != command.ConfirmPassword)
                    return await Result<string>.FailAsync("Mật khẩu xác nhận không khớp.");

                var existingUser = await _userManager.FindByEmailAsync(command.Email);
                if (existingUser is not null)
                    return await Result<string>.FailAsync("Email đã được sử dụng.");

                // ✅ Tạo role Customer nếu chưa có
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new ApplicationRole("Customer")
                    {
                        Code = "Customer"
                    });
                }

                var user = new ApplicationUser
                {
                    FullName = command.FullName,
                    Email = command.Email,
                    UserName = command.Email,
                    PhoneNumber = command.PhoneNumber,
                    IsActive = true,
                    CreatedOn = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, command.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return await Result<string>.FailAsync(errors);
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                return await Result<string>.SuccessAsync(user.Id, "Đăng ký thành công.");
            }
        }
    }
}