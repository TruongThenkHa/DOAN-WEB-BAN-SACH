using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Book_Store.Controllers
{
    public class StripeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<StripeController> _logger;

        public StripeController(
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<StripeController> logger
        )
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        [Route("Stripe/Success")]
        public IActionResult Success(string session_id)
        {
            _logger.LogInformation("Stripe success redirect for session {SessionId}", session_id);

            return View("~/Views/Cart/Success.cshtml");
        }

        [HttpGet]
        [Route("Stripe/Cancel")]
        public IActionResult Cancel()
        {
            TempData["WarningMessage"] = "Thanh toán đã bị hủy. Đơn hàng của bạn chưa được xử lý.";
            return RedirectToAction("Index", "Cart");
        }

        // -------------------------------------------------------
        // WEBHOOK
        // stripe listen --forward-to localhost:5000/Stripe/Webhook
        // -------------------------------------------------------
        [HttpPost]
        [Route("Stripe/Webhook")]
        public async Task<IActionResult> Webhook()
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("Stripe WebhookSecret is not configured.");
                return StatusCode(500);
            }

            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                json = await reader.ReadToEndAsync();
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(
                    "Stripe webhook signature validation failed: {Message}",
                    ex.Message
                );
                return BadRequest();
            }

            _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                    return Ok();

                // Retrieve our orderId from metadata
                if (
                    !session.Metadata.TryGetValue("order_id", out var orderIdStr)
                    || !int.TryParse(orderIdStr, out var orderId)
                )
                {
                    _logger.LogWarning(
                        "Stripe session {SessionId} has no order_id metadata",
                        session.Id
                    );
                    return Ok();
                }

                await MarkOrderPaidAsync(orderId, session.Id, session.PaymentIntentId);
            }

            // return 200
            return Ok();
        }

        // -------------------------------------------------------
        // HELPER: update order + create payment record in DB
        // -------------------------------------------------------
        private async Task MarkOrderPaidAsync(
            int orderId,
            string sessionId,
            string? paymentIntentId
        )
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null)
            {
                _logger.LogWarning("Webhook: order {OrderId} not found", orderId);
                return;
            }

            if (order.Status == OrderStatus.Paid)
            {
                _logger.LogInformation(
                    "Webhook: order {OrderId} already marked Paid, skipping",
                    orderId
                );
                return; // idempotent — Stripe sometimes sends duplicate events
            }

            order.Status = OrderStatus.Paid;
            _db.Orders.Update(order);

            // Record the payment
            var payment = new Payment
            {
                OrderID = orderId,
                Method = "Stripe",
                Status = "Paid",
                PaidAt = DateTime.UtcNow,
            };
            _db.Payments.Add(payment);

            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Order {OrderId} marked Paid via Stripe session {SessionId} / intent {IntentId}",
                orderId,
                sessionId,
                paymentIntentId ?? "n/a"
            );
        }
    }
}
