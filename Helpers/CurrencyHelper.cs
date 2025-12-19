using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Ticket_Booking.Services;

namespace Ticket_Booking.Helpers
{
    /// <summary>
    /// HTML Helper extensions for currency display
    /// </summary>
    public static class CurrencyHelper
    {
        /// <summary>
        /// Displays a price with currency conversion based on user's selected currency
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="usdAmount">The amount in USD</param>
        /// <returns>Formatted price string with currency symbol</returns>
        public static async Task<IHtmlContent> DisplayPriceAsync(this IHtmlHelper htmlHelper, decimal usdAmount)
        {
            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var currencyService = serviceProvider.GetRequiredService<ICurrencyService>();
            
            var priceDisplay = await currencyService.FormatPriceAsync(usdAmount, null);
            
            return new HtmlString(priceDisplay.FormattedString);
        }

        /// <summary>
        /// Displays a price with currency conversion (synchronous version for use in Razor)
        /// Note: This uses GetAwaiter().GetResult() which should be used carefully
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="usdAmount">The amount in USD</param>
        /// <returns>Formatted price string with currency symbol</returns>
        public static IHtmlContent DisplayPrice(this IHtmlHelper htmlHelper, decimal usdAmount)
        {
            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var currencyService = serviceProvider.GetRequiredService<ICurrencyService>();
            
            // Use GetAwaiter().GetResult() for synchronous access in Razor views
            // This is acceptable here as the service uses caching and should be fast
            var priceDisplay = currencyService.FormatPriceAsync(usdAmount, null).GetAwaiter().GetResult();
            
            return new HtmlString(priceDisplay.FormattedString);
        }
    }
}

