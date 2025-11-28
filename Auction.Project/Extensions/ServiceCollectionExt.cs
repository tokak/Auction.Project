using Auction.Business.Abstraction;
using Auction.Business.Concrete;
using Auction.Core.MailHelper;
using Auction.Core.Models;

namespace Auction.Project.Extensions
{
    public static class ServiceCollectionExt
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services,IConfiguration configuration)
        {
            #region services
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IBidService, BidService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped(typeof(ApiResponse));
            return services;
            #endregion
        }
    }
}
