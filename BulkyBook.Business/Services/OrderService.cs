using BulkyBook.Business.IServices;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Drawing;

namespace BulkyBook.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Add(orderHeader);
            await _context.SaveChangesAsync();
            return orderHeader;
        }

        public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync(string? userId = null, string? status = null, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();//  FirstOrDefault(o => o.Id = id);
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product); // ThenInclude: because Product is a property within OrderDetails
            }
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(u => u.ApplicationUserId == userId);
            }
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(u => u.OrderStatus.ToLower() == status.ToLower());
            }
            return await query.ToListAsync();
        }

        public async Task<OrderHeader> GetOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false)
        {
            var query = _context.OrderHeaders.AsQueryable();//  FirstOrDefault(o => o.Id = id);
            if (includeUser)
            {
                query = query.Include(u => u.ApplicationUser);
            }
            if (includeDetails)
            {
                query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product); // ThenInclude: because Product is a property within OrderDetails
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id);

        }

        public async Task UpdateOrderAsync(OrderHeader ordeHeader)
        {
            _context.OrderHeaders.Update(ordeHeader);
            await _context.SaveChangesAsync();

        }

        public async Task UpdateOrderStatusAsync(int id, string orderStatus, string? carrier = null, string? trackingNumber = null)
        {
            var orderHeaderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(u => u.Id == id);
            if (orderHeaderFromDb == null)
            {
                throw new KeyNotFoundException($"Order with id {id} not found");
            }
            orderHeaderFromDb.OrderStatus = orderStatus;
            if (orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.ShippingDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(carrier))
                {
                    orderHeaderFromDb.Carrier = carrier;
                }
                if (!string.IsNullOrEmpty(trackingNumber))
                {
                    orderHeaderFromDb.TrackingNumber = trackingNumber;
                }
                orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            }

            await _context.SaveChangesAsync();

        }

        public async Task UpdateStripePaymentAsync(int orderId, string sessionId, string paymentIntentId)
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with id {orderId} not found");
            }
            if (!string.IsNullOrEmpty(sessionId))
            {
                order.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                order.PaymentIntentId = paymentIntentId;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerifyStripePaymentAsync(OrderHeader orderHeader)
        {
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                await UpdateStripePaymentAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
                await UpdateOrderStatusAsync(orderHeader.Id, SD.StatusApproved);
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<string> CreateStripeCheckoutSessionAsync(OrderHeader orderHeader, IEnumerable<ShoppingCart> cartItems, string domain)
        {
            if (orderHeader == null)
            {
                throw new ArgumentNullException(nameof(orderHeader));
            }
            if (cartItems == null || !cartItems.Any())
            {
                throw new ArgumentException("Cart items cannot be empty.", nameof(cartItems));
            }

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + "Customer/ShoppingCart/OrderConfirmation?orderHeaderId=" + orderHeader.Id,
                CancelUrl = domain + "Customer/ShoppingCart/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", orderHeader.Id.ToString() }
                    }
            };

            foreach (var item in cartItems)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // Stripe expects amount in cents
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }

                    },
                    Quantity = item.Count,

                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            await UpdateStripePaymentAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
            return session.Url;
        }

        public async Task<bool> CancelOrderWithRefundAsync(int orderId)
        {
            var orderHeader = await _context.OrderHeaders.FindAsync(orderId);
            if (orderHeader == null)
            {
                throw new KeyNotFoundException($"Order with id {orderId} not found");
            }
            if (orderHeader.OrderStatus == SD.StatusShipped)
            {
                throw new InvalidOperationException("Cannot cancel orders that have already been shipped. Customer must initiate a refund instead.");
            }
            // Check if the order is already cancelled or refunded
            if (orderHeader.OrderStatus == SD.StatusCancelled || orderHeader.OrderStatus == SD.StatusRefunded)
            {
                throw new InvalidOperationException("This order has already been cancelled or refunded.");
            }

            bool refundIssued = false;

            if (!string.IsNullOrEmpty(orderHeader.PaymentIntentId) &&
                orderHeader.OrderStatus == SD.StatusApproved ||
                orderHeader.OrderStatus == SD.StatusProcessing)
            {
                try
                {
                    // Refund
                    var options = new RefundCreateOptions { PaymentIntent = orderHeader.PaymentIntentId, Reason = RefundReasons.RequestedByCustomer };
                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    if (refund.Status == "succeeded" || refund.Status == "pending")
                    {
                        refundIssued = true;
                        orderHeader.OrderStatus = SD.StatusRefunded;
                    }

                }
                catch (StripeException ex)
                {
                    orderHeader.OrderStatus = SD.StatusCancelled;
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException($"Stripe refund failed: {ex.Message}. Order has been cancelled, but refund could not be processed.");
                }
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
            }
            await _context.SaveChangesAsync();

            return refundIssued;
        }
    }
}
