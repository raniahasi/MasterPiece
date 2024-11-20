
using WeCartFinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wecartcore.DTO;
using Microsoft.AspNetCore.Identity;
using WeCartFinal.DTO;

namespace Wecartcore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly TokenGenerator _tokenGenerator;

        public UsersController(MyDbContext db, PasswordHasher<User> passwordHasher, TokenGenerator tokenGenerator)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        }

        [HttpGet]
        public IActionResult getAllUsers()
        {
            // Fetch the users from the database
            var Users = _db.Users.ToList();

            // Check if the Users list is null or empty
            if (Users == null || !Users.Any())
            {
                return NoContent(); // Return 204 No Content if no users are found
            }

            // If data is found, return it
            return Ok(Users);
        }

        [HttpGet]
        [Route("GetUserById/{id}")]
        public IActionResult GetUserById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid ID. The ID must be a positive integer." });
            }

            var user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(user);
        }



        [HttpPut]
        [Route("UpdateUser/{id}")]

        public IActionResult UpdateUser(int id, [FromForm] UsersRequestDTO updatedUser)
        {
            var existingUser = _db.Users.Find(id);
            if (existingUser == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }
            existingUser.Name = updatedUser.Name ?? existingUser.Name;

            existingUser.Email = updatedUser.Email ?? existingUser.Email;


            existingUser.PhoneNumber = updatedUser.PhoneNumber ?? existingUser.PhoneNumber;
            existingUser.Password = updatedUser.Password ?? existingUser.Password;

            _db.Users.Update(existingUser);
            _db.SaveChanges();
            return Ok(new { message = "User updated successfully.", user = existingUser });
        }


        [HttpPost]
        [Route("ChangePassword")]
        public IActionResult ChangePassword([FromForm] ChangePasswordDTO request)
        {
            if (request.UserId <= 0)
            {
                return BadRequest(new { message = "Invalid User ID." });
            }

            var user = _db.Users.Find(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!PasswordHasherMethod.VerifyPasswordHash(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { message = "Old password is incorrect." });
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest(new { message = "New password and confirmation do not match." });
            }

            PasswordHasherMethod.CreatePasswordHash(request.NewPassword, out byte[] newPasswordHash, out byte[] newPasswordSalt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = newPasswordSalt;
            user.Password = request.NewPassword;

            try
            {
                _db.Users.Update(user);
                _db.SaveChanges();
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while saving changes." });
            }
        }







        [HttpPost("register")]
        public ActionResult Register([FromForm] UserDTO model)
        {
            // Hash the password
            byte[] passwordHash, passwordSalt;
            PasswordHasher.CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            var user = new User
            {
                Name = model.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Password = model.Password,
                Email = model.Email
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok(user);
        }
        [HttpPost("login")]
        public IActionResult Login([FromForm] DTOsLogin model)
        {

            // Regular email/password login
            var user = _db.Users.FirstOrDefault(x => x.Email == model.Email);
            if (user == null || !PasswordHasher.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Retrieve roles and generate JWT token
            var token = _tokenGenerator.GenerateToken(user.Name);

            return Ok(new { Token = token, userId = user.UserId, userName = user.Name });
        }


        [HttpPost("registerAdmin")]
        public ActionResult RegisterAdmin([FromForm] UserDTO model)
        {
            // Hash the password
            byte[] passwordHash, passwordSalt;
            PasswordHasher.CreatePasswordHash(model.Password, out passwordHash, out passwordSalt);

            var user = new Admin
            {
                Name = model.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = model.Email
            };

            _db.Admins.Add(user);
            _db.SaveChanges();

            return Ok(user);
        }

        [HttpPost("loginAdmin")]
        public IActionResult LoginAdmin([FromForm] DTOsLogin model)
        {

            // Regular email/password login
            var user = _db.Admins.FirstOrDefault(x => x.Email == model.Email);
            if (user == null || !PasswordHasher.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Retrieve roles and generate JWT token
            var token = _tokenGenerator.GenerateToken(user.Name);

            return Ok(new { Token = token, userID = user.AdminId });
        }
    }
}
