using Microsoft.AspNetCore.Mvc;
using WeCartFinal.Models; // Adjust the namespace according to your project structure
using System.IO;
using System.Linq;
using System;
using Wecartcore.DTO; // Ensure you have a DTO namespace with the CategoryDTO class
using Microsoft.EntityFrameworkCore;

namespace WeCartCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public CategoriesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }










        // GET: api/Categories/{id}
        [HttpGet("{id}")] // 
        public IActionResult GetCategoryById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID must be greater than 0.");
            }

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }











        // GET: api/Categories/name/{name}
        [HttpGet("name/{name}")]
        public IActionResult GetCategoryByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name cannot be null or empty.");
            }

            var category = _context.Categories.FirstOrDefault(c => c.Name == name);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }





        // DELETE: api/Categories/{id}
        [HttpDelete("{id}")] // Constraint: id must be >= 5
        public IActionResult DeleteCategory(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID must be greater than 0.");
            }

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return NoContent();
        }





        // POST: api/Categories
        [HttpPost]
        public IActionResult AddCategory([FromForm] CategoryDTO categoryDto)
        {
            string fileName = null;

            if (categoryDto.Image != null)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/images");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.Image.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    categoryDto.Image.CopyTo(stream);
                }
            }

            var category = new Category
            {
                Description = categoryDto.Description,
                Name = categoryDto.Name,
                Image = fileName != null ? "/uploads/images/" + fileName : null
            };

            _context.Categories.Add(category);
            _context.SaveChanges();

            return Ok(category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] DTOsCategory dto)
        {
            // Find the category in the database
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                Console.WriteLine("Category not found with ID: " + id);
                return NotFound("Category not found.");
            }

            // Update the fields only if they are provided
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                category.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                category.Description = dto.Description;
            }

            if (dto.Image != null)
            {
                // Log that the image was received
                Console.WriteLine("Image file received: " + dto.Image.FileName);

                // Delete old image if it exists
                if (!string.IsNullOrWhiteSpace(category.Image))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", category.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        Console.WriteLine("Deleting old image: " + oldFilePath);
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save the new image
                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
                var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                category.Image = $"uploads/{newFileName}";
                Console.WriteLine("New image saved at: " + category.Image);
            }

            // Save the changes to the database
            _context.SaveChanges();
            return Ok(category);
        }





        // GET: api/categories-with-product-count
        [HttpGet("categories-with-product-count")]
        public async Task<IActionResult> GetCategoriesWithProductCount()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ProductCount = c.Products.Count() // Count the products in each category
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}
