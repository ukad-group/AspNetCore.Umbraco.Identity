using Microsoft.AspNetCore.Identity;
using AspNetCore.Umbraco.Identity;
using System.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUmbracoIdentity<TUser>(this IServiceCollection services)
            where TUser : class, IUser, new()
        {
            services.AddIdentityCore<TUser>(opt =>
            {
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireDigit = true;
                opt.Password.RequiredLength = 7;
            });

            services.AddScoped(typeof(IUserStore<TUser>), typeof(UmbracoMemberUserStore<TUser>));
            services.AddScoped(typeof(IPasswordHasher<TUser>), typeof(UmbracoPasswordHasher<TUser>));
            return services;
        }
    }
}
