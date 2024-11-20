using WeCartFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wecartcore.DTO;
using Microsoft.EntityFrameworkCore;
using WeCartFinal.DTO;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly MyDbContext _db;
        private readonly IConfiguration _configuration; // Inject IConfiguration

        private readonly EmailService _emailService; // Inject the EmailService

        public OrderController(MyDbContext db, EmailService emailService, IConfiguration configuration)
        {
            _db = db;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO orderDTO)
        {
            try
            {
                // Check if the orderDTO or its items are null
                if (orderDTO == null || orderDTO.Items == null || !orderDTO.Items.Any())
                {
                    return BadRequest(new { success = false, message = "Invalid order data. Order items are required." });
                }

                // Create a new Order object
                var newOrder = new Order()
                {
                    UserId = orderDTO.UserId,
                    Amount = orderDTO.Amount,
                    Name = orderDTO.Name,
                    Address = orderDTO.Address,
                    PhoneNumber = orderDTO.PhoneNumber,
                    Email = orderDTO.Email,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Status = "Pending" // Use a default status of "Pending"
                };

                // Add the order to the database and save changes to generate the OrderId
                _db.Orders.Add(newOrder);
                _db.SaveChanges();

                // Loop through each item in the orderDTO and create OrderItems
                foreach (var item in orderDTO.Items)
                {
                    // Check if ProductId or Quantity is invalid
                    if (item.ProductId <= 0 || item.Quantity <= 0)
                    {
                        return BadRequest(new { success = false, message = "Invalid product ID or quantity in order items." });
                    }

                    var orderItem = new OrderItem()
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };
                    _db.OrderItems.Add(orderItem);
                }

                // Clear the user's cart items after saving the order
                var cartItems = _db.CartItems.Where(ci => ci.UserId == newOrder.UserId).ToList();
                if (cartItems.Any())
                {
                    _db.CartItems.RemoveRange(cartItems); // Remove all cart items for the user
                }

                // Save all changes to the database
                _db.SaveChanges();

                // Send an email to the admin
                try
                {
                    // Get the admin email from configuration
                    string adminEmail = _configuration["EmailSettings:AdminEmail"];
                    string subject = "New Order Received";
                    string itemsDetails = "";

                    // Construct the order items details
                    foreach (var item in newOrder.OrderItems)
                    {
                        // Fetch the product name
                        var product = _db.Products.Find(item.ProductId);
                        string productName = product != null ? product.Name : "Unknown Product";

                        itemsDetails += $@"
                    <li>
                        <strong>Product:</strong> {productName} <br>
                        <strong>Quantity:</strong> {item.Quantity} <br>
                    </li>";
                    }

                    string message = $@"
                <html>
                    <body>
                        <h2>New Order Details</h2>
                        <p><strong>Order ID:</strong> {newOrder.OrderId}</p>
                        <p><strong>Customer Name:</strong> {newOrder.Name}</p>
                        <p><strong>Total Amount:</strong> {newOrder.Amount} JD</p>
                        <p><strong>Address:</strong> {newOrder.Address}</p>
                        <p><strong>Phone:</strong> {newOrder.PhoneNumber}</p>
                        <p><strong>Items:</strong> {newOrder.OrderItems.Count} item(s)</p>
                        <ul>{itemsDetails}</ul> <!-- List the items with names and quantities -->
                    </body>
                </html>";

                    // Use your email service to send the email
                    await _emailService.SendEmailAsync(adminEmail, subject, message);

                    // Log a success message
                    Console.WriteLine("Email sent successfully to admin.");
                }
                catch (Exception ex)
                {
                    // Log the error if the email fails to send
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }

                return Ok(new { success = true, message = "Order created successfully.", orderId = newOrder.OrderId });
            }
            catch (Exception ex)
            {
                // Log the error details
                Console.WriteLine($"Error creating order: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Failed to create order." });
            }
        }









        [HttpGet("GetOrdersWithProductsByUser/{userId}")]
        public IActionResult GetOrdersWithProductsByUser(int userId)
        {
            var orders = _db.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    OrderId = o.OrderId,
                    Products = o.OrderItems.Select(oi => new
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        OrderItemId = oi.OrderItemId
                    }).ToList()
                }).ToList();

            if (!orders.Any())
            {
                return NotFound("No orders found for this user.");
            }

            return Ok(orders);
        }






        [HttpGet]
        [Route("AllOrders")]
        public IActionResult GetAllOrders()
        {
            var orders = _db.Orders
                .Select(order => new OrderResponseDto
                {
                    OrderId = order.OrderId,
                    Date = order.Date,

                    // Customer information
                    Customer = new UserDto
                    {
                        Name = order.User.Name
                    },

                    // Calculate the total number of items
                    NumberOfItems = order.OrderItems.Sum(oi => oi.Quantity ?? 0),

                    // Calculate the total price of the order
                    Total = order.OrderItems.Sum(oi => (oi.Quantity ?? 0) * (oi.Product.Price ?? 0)),

                    // Status of the order
                    Status = order.Status,

                    // Map each order item to OrderItemDto
                    OrderItem = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.Product.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Product.Price
                    }).ToList()
                })
                .ToList();

            return Ok(orders);
        }


        [HttpGet]
        [Route("GetOrdersByUser/{userId}")]
        public IActionResult GetOrdersByUserId(int userId)
        {
            // Retrieve all orders that belong to the specified user
            var orders = _db.Orders
                .Where(order => order.UserId == userId) // Filter by UserId
                .Select(order => new OrderResponseDto
                {
                    OrderId = order.OrderId,
                    Date = order.Date,
                    Customer = new UserDto
                    {
                        Name = order.User.Name
                    },
                    NumberOfItems = order.OrderItems.Sum(oi => oi.Quantity ?? 0),
                    Total = order.OrderItems.Sum(oi => (oi.Quantity ?? 0) * (oi.Product.Price ?? 0)),
                    Status = order.Status,
                    OrderItem = order.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.Product.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Product.Price
                    }).ToList()
                })
                .ToList();

            // Check if the user has any orders
            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for this user." });
            }

            return Ok(orders);
        }







        [HttpGet]
        [Route("GetOrderItemsByOrderId/{orderId}")]
        public IActionResult GetOrderItemsByOrderId(int orderId)
        {
            // Retrieve all order items that belong to the specified order
            var orderItems = _db.OrderItems
                .Where(oi => oi.OrderId == orderId) // Filter by OrderId
                .Select(oi => new OrderItemDto
                {
                    ProductId = oi.Product.ProductId,

                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Product.Price
                })
                .ToList();

            // Check if the order has any items
            if (orderItems.Count == 0)
            {
                return NotFound(new { message = "No items found for this order." });
            }

            return Ok(orderItems);
        }






        [HttpPut("UpdateOrderStatus/{orderId}")]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] OrderStatusUpdateDto statusUpdate)
        {
            // Find the order by ID
            var order = _db.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Update the status

            string statusString = statusUpdate.Status == 0 ? "Pending" : "Completed";

            order.Status = statusString;

            // Save changes to the database
            _db.SaveChanges();

            return Ok(new { message = "Order status updated successfully", order });
        }




        [HttpGet]
        [Route("AllCategories")]
        public IActionResult GetAllCategories()
        {
            var data = _db.Categories.ToList();
            return Ok(data);
        }






        [HttpGet("getOrderById/{orderId}")]
        public IActionResult GetOrderById(int orderId)
        {
            var order = _db.Orders
                .Include(o => o.User)  // Include the related User entity
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(order);
        }




        [HttpGet]
        [Route("GetOrderDetails/{orderId}")]
        public IActionResult GetOrderDetails(int orderId)
        {
            var order = _db.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderDetailsDto
                {
                    OrderId = o.OrderId,
                    Date = o.Date,
                    Status = o.Status,
                    TotalAmount = o.OrderItems.Sum(oi => (oi.Quantity ?? 0) * (oi.Product.Price ?? 0)),

                    // Products (OrderItems)
                    OrderItems = o.OrderItems.Select(oi => new OrderDetailsDto.OrderItemDto
                    {
                        ProductId = oi.Product.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity ?? 0,
                        Price = oi.Product.Price ?? 0,
                        ProductImage = oi.Product.Image // Assuming Image field exists in Product
                    }).ToList(),

                    // Customer (User)
                    Customer = new OrderDetailsDto.CustomerDto
                    {
                        Name = o.User.Name,
                        Email = o.User.Email,
                        Image = o.User.Image,
                        PhoneNumber = o.User.PhoneNumber

                    },

                    // التحقق من وجود كوبون وجلب معلوماته
                    Coupon = new OrderDetailsDto.CouponDto
                    {
                        Name = o.Copon.Name ?? "No Copoun Applied",  // Assuming Coupon has Name
                        Amount = o.Copon.Amount ?? 0// Assuming Coupon has DiscountAmount
                    }
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }








    }
}