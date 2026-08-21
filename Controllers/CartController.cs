using System.Text.Json;
using Book_Store.Models;
using Book_Store.Services.Stripe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Book_Store.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStripeService _stripeService;
        private const string CARTKEY = "cart";

        public CartController(ApplicationDbContext context, IStripeService stripeService)
        {
            _context = context;
            _stripeService = stripeService;
        }

        // ===========================
        // HIỂN THỊ GIỎ HÀNG
        // ===========================
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantityApi([FromBody] UpdateQtyRequest req)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == req.Id);
            if (item != null && req.Quantity > 0)
                item.Quantity = req.Quantity;
            SaveCart(cart);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult RemoveApi([FromBody] RemoveRequest req)
        {
            var cart = GetCart();
            cart.RemoveAll(p => p.ProductId == req.Id);
            SaveCart(cart);
            return Ok(new { success = true });
        }

        // ===========================
        // THÊM SẢN PHẨM (AJAX)
        // ===========================
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                string localReturnUrl = "/";
                var referer = Request.Headers["Referer"].ToString();
                if (
                    !string.IsNullOrEmpty(referer)
                    && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                )
                {
                    localReturnUrl = uri.PathAndQuery;
                }

                var redirectUrl = Url.Action(
                    "AddToCartAndRedirect",
                    "Cart",
                    new
                    {
                        id = id,
                        quantity = quantity,
                        returnUrl = localReturnUrl,
                    }
                );
                return Json(new { success = false, redirectUrl = redirectUrl });
            }

            var cart = GetCart();

            var book = _context
                .Books.Include(b => b.BookImages)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return Json(new { success = true, count = cart.Sum(x => x.Quantity) });

            var item = cart.FirstOrDefault(p => p.ProductId == id);

            if (item == null)
            {
                cart.Add(
                    new CartItem
                    {
                        ProductId = book.BookID,
                        ProductName = book.Title,
                        Price = book.Price,
                        Image =
                            book.BookImages?.FirstOrDefault(i => i.IsPrimary)?.ImagePath
                            ?? "/images/no-image.png",
                        Quantity = quantity,
                    }
                );
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            return Json(new { success = true, count = cart.Sum(x => x.Quantity) });
        }

        // THÊM SẢN PHẨM & CHUYỂN HƯỚNG (SAU KHI ĐĂNG NHẬP)
        [HttpGet]
        public IActionResult AddToCartAndRedirect(
            int id,
            int quantity = 1,
            string? returnUrl = null
        )
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "AddToCartAndRedirect",
                            "Cart",
                            new
                            {
                                id = id,
                                quantity = quantity,
                                returnUrl = returnUrl,
                            }
                        ),
                    }
                );
            }

            var cart = GetCart();
            var book = _context
                .Books.Include(b => b.BookImages)
                .FirstOrDefault(b => b.BookID == id);

            if (book != null)
            {
                var item = cart.FirstOrDefault(p => p.ProductId == id);
                if (item == null)
                {
                    cart.Add(
                        new CartItem
                        {
                            ProductId = book.BookID,
                            ProductName = book.Title,
                            Price = book.Price,
                            Image =
                                book.BookImages?.FirstOrDefault(i => i.IsPrimary)?.ImagePath
                                ?? "/images/no-image.png",
                            Quantity = quantity,
                        }
                    );
                }
                else
                {
                    item.Quantity += quantity;
                }
                SaveCart(cart);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // CẬP NHẬT SỐ LƯỢNG
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductId == id);

            if (item != null && quantity > 0)
                item.Quantity = quantity;

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ===========================
        // XOÁ SẢN PHẨM
        // ===========================
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(p => p.ProductId == id);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // ===========================
        // CHECKOUT
        // ===========================
        public IActionResult Checkout()
        {
            var cart = GetCart();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdVal = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;
                if (int.TryParse(userIdVal, out var userId))
                {
                    var user = _context.Users.FirstOrDefault(u => u.ID == userId);
                    ViewBag.CurrentUser = user;
                }
            }

            return View(cart);
        }

        // ===========================
        // ĐẶT HÀNG (COD + STRIPE + MOMO)
        // ===========================
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            string address,
            string paymentMethod,
            string? phone = null
        )
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Index");

            // --- Resolve current user ---
            string customerName = string.Empty;
            string userPhone = string.Empty;
            int? currentUserId = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdVal = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;
                if (int.TryParse(userIdVal, out var userId))
                {
                    currentUserId = userId;
                    var user = _context.Users.FirstOrDefault(u => u.ID == userId);
                    if (user != null)
                    {
                        customerName = user.FullName;
                        userPhone = user.PhoneNumber ?? string.Empty;
                    }
                }
            }

            var totalAmount = cart.Sum(x => x.Price * x.Quantity);

            // --- Create order record (Pending for all methods until payment confirmed) ---
            var order = new Order
            {
                UserID = currentUserId,
                CustomerName = customerName,
                PhoneNumber = userPhone,
                OrderDate = DateTime.Now,
                ShippingAddress = address,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                _context.OrderDetails.Add(
                    new OrderDetail
                    {
                        OrderID = order.OrderID,
                        BookID = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                    }
                );
            }
            _context.SaveChanges();

            // --- Clear cart immediately for COD; keep it until webhook confirms for Stripe ---
            if (paymentMethod == "Stripe")
            {
                // Build absolute URLs for Stripe redirect targets
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var successUrl = $"{baseUrl}/Stripe/Success?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{baseUrl}/Stripe/Cancel";

                // Build a short item description for the Stripe page
                var description = string.Join(
                    ", ",
                    cart.Select(i => $"{i.ProductName} x{i.Quantity}").Take(3)
                );
                if (cart.Count > 3)
                    description += $" và {cart.Count - 3} sản phẩm khác";

                // Clear cart before redirecting — webhook will handle order status update
                HttpContext.Session.Remove(GetCartKey());

                var stripeUrl = await _stripeService.CreateCheckoutSessionAsync(
                    orderId: order.OrderID,
                    amountVnd: (long)totalAmount,
                    customerName: customerName,
                    description: description,
                    successUrl: successUrl,
                    cancelUrl: cancelUrl
                );

                return Redirect(stripeUrl);
            }



            // COD: mark Paid immediately (no payment gateway involved)
            if (paymentMethod == "COD")
            {
                order.Status = OrderStatus.Pending; // stays Pending until shop confirms delivery
                _context.SaveChanges();
                HttpContext.Session.Remove(GetCartKey());
                return RedirectToAction("Success");
            }

            HttpContext.Session.Remove(GetCartKey());
            return RedirectToAction("Success");
        }

        // ===========================
        // SUCCESS VIEW
        // ===========================
        public IActionResult Success()
        {
            return View();
        }

        // ===========================
        // SESSION HELPERS
        // ===========================
        private string GetCartKey()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return $"cart_{userId}";
            }
            return "cart_guest";
        }

        private List<CartItem> GetCart()
        {
            var session = HttpContext.Session.GetString(GetCartKey());
            if (!string.IsNullOrEmpty(session))
                return JsonSerializer.Deserialize<List<CartItem>>(session) ?? new List<CartItem>();
            return new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(GetCartKey(), JsonSerializer.Serialize(cart));
        }
    }
}

public record UpdateQtyRequest(int Id, int Quantity);

public record RemoveRequest(int Id);
