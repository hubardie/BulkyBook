using BulkyBook.Business.IServices;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderHeader = await _orderService.GetOrderByIdAsync(orderId, includeUser: true, includeDetails: true);
            return View(OrderHeader);
        }
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)] // Employee and admin can update order status
        public async Task<IActionResult> UpdateOrderDetails() // OrderHeader not required because it is already bound to the property
        {
            // Mapeamos manualmente los campos que queremos actualizar, para evitar que se actualicen campos que no queremos que se actualicen, si no valdría con _orderService.UpdateOrderAsync(OrderHeader);

            var orderHeaderFromDb = await _orderService.GetOrderByIdAsync(OrderHeader.Id);
            orderHeaderFromDb.Name = OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderHeader.City;
            orderHeaderFromDb.State = OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderHeader.Carrier) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.Carrier = OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderHeader.TrackingNumber) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.TrackingNumber = OrderHeader.TrackingNumber;
            }
            await _orderService.UpdateOrderAsync(orderHeaderFromDb);
            TempData["success"] = "Order details updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)] // Employee and admin can update order status
        public async Task<IActionResult> UpdateOrderStatus(string status)
        {
            // Mapeamos manualmente los campos que queremos actualizar, para evitar que se actualicen campos que no queremos que se actualicen, si no valdría con _orderService.UpdateOrderAsync(OrderHeader);

            var orderHeaderFromDb = await _orderService.GetOrderByIdAsync(OrderHeader.Id);
            if(orderHeaderFromDb == null)
            {
                TempData["error"] = "Order not found";
                return RedirectToAction(nameof(Index));
            }
            string successMessage = "";
            switch (status)
            {
                case SD.StatusProcessing:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order processing started successfully";
                    break                        ;

                case SD.StatusCancelled:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order cancelled successfully";
                    break;
                case SD.StatusRefunded:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order refunded successfully";
                    break;

                case SD.StatusShipped:
                    if(string.IsNullOrEmpty(OrderHeader.Carrier) || string.IsNullOrEmpty(OrderHeader.TrackingNumber))
                    {
                        TempData["error"] = "Please provide both Carrier and Tracking Number";
                        return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
                    }

                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status, OrderHeader.Carrier, OrderHeader.TrackingNumber);
                    successMessage = "Order shipped successfully";
                    break;

                default:
                    TempData["error"] = "Invalid status";
                    return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
            }

            TempData["success"] = successMessage;
            return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
        }


        // API CALLS to get all orders to datatable
        #region "API CALLS"
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string? status = null)
        {
            string? userId = null;
            if (!User.IsInRole(SD.RoleAdmin) && !User.IsInRole(SD.RoleEmployee))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();                
                }
            }

            var orders = await _orderService.GetAllOrdersAsync(userId: userId, status: status);
            return Json(new { data = orders });
        }
        #endregion
    }


}
