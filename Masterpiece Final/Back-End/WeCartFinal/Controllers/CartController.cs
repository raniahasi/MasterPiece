
using WeCartFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wecartcore.DTO;
using Microsoft.EntityFrameworkCore;

namespace Wecartcore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly MyDbContext _db;
        private readonly ILogger<CartController> _logger;


        public CartController(MyDbContext db, ILogger<CartController> logger)
        {
            _db = db;
            _logger = logger;
        }



        ////////////////////////// Get All CART Items >>> Response //////////////////////////

        [HttpGet("GetAllCartItems")]
        public IActionResult GetAllCartItems()
        {

            var cartItems = _db.CartItems.Select(c =>
            new CartItemResponseDTOs
            {
                CartItemId = c.CartItemId,
                ProductId = c.ProductId,
                UserId = c.UserId,
                Quantity = c.Quantity,

                prodcutDTO = new ProductResponseDTO
                {
                    Name = c.Product.Name,
                    Description = c.Product.Description,
                    Image = c.Product.Image,
                    Price = c.Product.Price,
                    Color = c.Product.Color,
                    ProductColorId = c.Product.ProductColorId,
                    PriceWithDiscount = c.Product.PriceWithDiscount,

                }
            }
            );

            if (!cartItems.Any())

            {
                return BadRequest("No Cart Items Found");
            }

            return Ok(cartItems);
        }







        ////////////////////////// Get NEW CART Item By User ID >>> Response //////////////////////////

        [HttpGet("GetAllCartItemsByUserId/{userId:int}")]

        public IActionResult GetCartItemsByUserId(int userId)
        {



            if (userId <= 0)
            {
                return BadRequest($"User Id must be greater than 0");
            }

            var cartItems = _db.CartItems.Where(c => c.UserId == userId).Select(c =>

            new CartItemResponseDTOs
            {
                CartItemId = c.CartItemId,
                ProductId = c.ProductId,
                UserId = c.UserId,
                Quantity = c.Quantity,

                prodcutDTO = new ProductResponseDTO
                {
                    Name = c.Product.Name,
                    Description = c.Product.Description,
                    Image = c.Product.Image,
                    Price = c.Product.Price,
                    Color = c.Product.Color,
                    ProductColorId = c.Product.ProductColorId,
                    PriceWithDiscount = c.Product.PriceWithDiscount,

                }
            }

            );



            if (!cartItems.Any())
            {
                return NotFound($"No Cart Items Found for user with id {userId}");

            }

            return Ok(cartItems);

        }



        ////////////////////////// CREATE new CART Item>> Request //////////////////////////


        [HttpPost("CreateNewCartItem")]
        public IActionResult CreateNewCartItem([FromBody] CartItemRequestDTOs CartItemDTO)
        {
            if (CartItemDTO == null)
            {
                return BadRequest("Your request doesn't contain data!");
            }

            if (!ModelState.IsValid || CartItemDTO.ProductId == null)
            {
                return BadRequest("Invalid data.");
            }

            var productExists = _db.Products.Any(p => p.ProductId == CartItemDTO.ProductId);
            if (!productExists)
            {
                return BadRequest("Product does not exist.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingCartItem = _db.CartItems.FirstOrDefault(c => c.UserId == CartItemDTO.UserId && c.ProductId == CartItemDTO.ProductId);

                if (existingCartItem != null)
                {
                    // Item exists, update the quantity
                    existingCartItem.Quantity += CartItemDTO.Quantity;
                    _db.CartItems.Update(existingCartItem);
                    _db.SaveChanges();

                    return Ok(new { item = existingCartItem, msg = "Item quantity updated" });
                }

                var newCartItem = new CartItem
                {
                    ProductId = CartItemDTO.ProductId,
                    UserId = CartItemDTO.UserId,
                    Quantity = CartItemDTO.Quantity,
                };
                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();

                var returnContent = new
                {
                    item = newCartItem,
                    msg = "New Item Added"
                };

                return Ok(returnContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }








        [HttpPut("UpdateCartItem")]
        public IActionResult UpdateCartItem([FromBody] CartItemRequestDTOs updateRequest)
        {
            if (updateRequest == null)
            {
                return BadRequest("Invalid request data.");
            }

            var cartItem = _db.CartItems.FirstOrDefault(c => c.CartItemId == updateRequest.CartItemId);
            if (cartItem == null)
            {
                return NotFound("Cart item not found.");
            }

            cartItem.Quantity += updateRequest.QuantityChange;
            if (cartItem.Quantity <= 0)
            {
                _db.CartItems.Remove(cartItem); // Optionally remove the item if the quantity falls to zero or less
            }
            else
            {
                _db.CartItems.Update(cartItem);
            }
            _db.SaveChanges();

            return Ok(new { success = true, message = "Cart updated successfully." });
        }



        [HttpDelete("DeleteCartItem/{cartItemId:int}")]
        public IActionResult DeleteCartItem(int cartItemId)
        {
            if (cartItemId <= 0)
            {
                return BadRequest("CartItemId must be greater than 0.");
            }

            var cartItem = _db.CartItems.FirstOrDefault(c => c.CartItemId == cartItemId);

            if (cartItem == null)
            {
                return NotFound($"No cart item found with id {cartItemId}");
            }

            _db.CartItems.Remove(cartItem);
            _db.SaveChanges();

            return Ok(new { msg = "Cart item deleted successfully" });
        }


        [HttpDelete("ClearCartByUserId/{userId:int}")]
        public IActionResult ClearCartByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid User ID.");
            }

            var cartItems = _db.CartItems.Where(c => c.UserId == userId);
            if (!cartItems.Any())
            {
                return NotFound("No cart items found for the user.");
            }

            _db.CartItems.RemoveRange(cartItems);
            _db.SaveChanges();

            return Ok("All cart items cleared successfully.");
        }

        [HttpGet("GetCartItemCount/{userId}")]
        public ActionResult<int> GetCartItemCount(int userId)
        {
            var count = _db.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => c.Quantity); // Assuming 'Quantity' reflects the number of each item in the cart

            return Ok(count);
        }

    }
}
