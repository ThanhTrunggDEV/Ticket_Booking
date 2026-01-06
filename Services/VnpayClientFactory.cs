using VNPAY;
using VNPAY.Extensions;
using VNPAY.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Ticket_Booking.Services
{
    public interface IVnpayClientFactory
    {
        IVnpayClient CreateClient(string? callbackUrl = null);
    }

    public class VnpayClientFactory : IVnpayClientFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IVnpayClient _defaultClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnpayClientFactory(
            IConfiguration configuration, 
            IVnpayClient defaultClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _defaultClient = defaultClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public IVnpayClient CreateClient(string? callbackUrl = null)
        {
            if (string.IsNullOrEmpty(callbackUrl))
            {
                return _defaultClient;
            }

            var vnpayConfig = _configuration.GetSection("VNPAY");
            
            // Create VnpayConfiguration with custom callback URL
            var config = new VnpayConfiguration
            {
                TmnCode = vnpayConfig["TmnCode"]!,
                HashSecret = vnpayConfig["HashSecret"]!,
                CallbackUrl = callbackUrl
            };
            
            var options = Options.Create(config);
            
            // Create new client with custom configuration
            var client = new VnpayClient(options, _httpContextAccessor);
            
            return client;
        }
    }
}

