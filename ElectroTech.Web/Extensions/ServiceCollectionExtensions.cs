using ElectroTech.Infrastructure.DbContexts;
using ElectroTech.Infrastructure.SharedServices;
using ElectroTech.Web.Service;
using Entities;
using Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ElectroTech.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddPersistenceContexts(configuration);
            services.AddAuthenticationScheme(configuration); // tắt login
        }

        private static void AddAuthenticationScheme(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(o =>
            {
                //Add Authentication to all Controllers by default.
                //var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                //o.Filters.Add(new AuthorizeFilter(policy));

                o.Conventions.Add(new MyAuthorizeFiltersControllerConvention());
            });
        }
        private static void AddPersistenceContexts(this IServiceCollection services, IConfiguration configuration)
        {

            var connetname = configuration.GetConnectionString("ApplicationConnection");
            services.AddDbContext<IdentityContext>(options => options.UseSqlServer(connetname));
            services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"));
                    options.EnableSensitiveDataLogging();//bật để xem đầy đủ log các cột xung đột
                }
            );

           // services.AddHangfire(x => x
           //.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
           //.UseSimpleAssemblyNameTypeSerializer()
           //.UseRecommendedSerializerSettings()
           ////.UseDefaultTypeSerializer()
           //.UseMemoryStorage()
           //.UseSqlServerStorage(connetname, new SqlServerStorageOptions
           //{
           //    CommandBatchMaxTimeout = TimeSpan.FromMinutes(60),
           //    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(60),
           //    QueuePollInterval = TimeSpan.Zero,
           //    UseRecommendedIsolationLevel = true,
           //    DisableGlobalLocks = true
           //}));
            ///   services.AddHangfireServer(); đóng lại vì dùng worker

            //thêm identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                //options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<IdentityContext>().AddDefaultUI().AddDefaultTokenProviders();
            // cái này cần
            //var tokenValidationParameters = new TokenValidationParameters
            //{
            //    ValidateIssuerSigningKey = true,
            //    ValidateIssuer = true,
            //    ValidateAudience = true,
            //    ValidateLifetime = true,
            //    ClockSkew = TimeSpan.Zero,
            //    ValidIssuer = configuration["JWTSettings:Issuer"],
            //    ValidAudience = configuration["JWTSettings:Audience"],
            //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]))
            //};

           // services.AddSingleton(tokenValidationParameters);
        }
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<JiraSetting>(configuration.GetSection("JiraSetting"));
            services.AddTransient<IAuthenticatedUserService, AuthenticatedUserService>();
        }

    }
}
