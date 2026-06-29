using Stripe;
using Stripe.Checkout;

namespace Book_Store.Services.Stripe
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(
            int orderId,
            long amountVnd,
            string customerName,
            string description,
            string successUrl,
            string cancelUrl
        );
    }

    public class StripeService : IStripeService
    {
        private readonly ILogger<StripeService> _logger;

        public StripeService(ILogger<StripeService> logger)
        {
            _logger = logger;
        }

        public async Task<string> CreateCheckoutSessionAsync(
            int orderId,
            long amountVnd,
            string customerName,
            string description,
            string successUrl,
            string cancelUrl
        )
        {
            // VND is a zero-decimal currency in Stripe — pass the raw VND amount directly,
            // no multiplication by 100 needed (unlike USD which uses cents).
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "vnd",
                            UnitAmount = amountVnd,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Đơn hàng #{orderId} — Nhà Sách Hải An",
                                Description = description,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                // Embed orderId in metadata so the webhook knows which order to mark Paid
                Metadata = new Dictionary<string, string> { { "order_id", orderId.ToString() } },
                CustomerEmail = null, // optionally pass currentUser.Email here
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            _logger.LogInformation(
                "Stripe session {SessionId} created for order {OrderId}",
                session.Id,
                orderId
            );
            return session.Url;
        }
    }
}
