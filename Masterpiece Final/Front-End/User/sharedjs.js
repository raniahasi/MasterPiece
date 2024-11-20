





// Function to update cart count
function updateCartCount() {
  const cartItems = JSON.parse(localStorage.getItem("cartItems")) || [];
  const cartCount = cartItems.reduce((total, item) => total + item.Quantity, 0);
  const cartCountElement = document.querySelector(".cart-icon .qty");

  if (cartCountElement) {
    cartCountElement.textContent = cartCount;
  }
}


// Account and Logout Logic
document.addEventListener("DOMContentLoaded", function () {
    const accountBtn = document.getElementById("accountBtn");
    const accountText = document.getElementById("accountText");
    const logoutBtn = document.getElementById("logoutBtn");
    const username = localStorage.getItem("username");
  
    if (username) {
      accountText.textContent = username;
      accountBtn.href = "/cart js and css/account.html";
      logoutBtn.style.display = "inline-block";
  
      // Add event listener to the logout link
      logoutBtn.querySelector("a").addEventListener("click", function (event) {
        event.preventDefault();
  
        // Remove username and userId from local storage
        localStorage.removeItem("username");
        localStorage.removeItem("userId"); // Remove userId from local storage
  
        // Redirect to home page
        window.location.href = "home.html";
      });
    } else {
      accountBtn.addEventListener("click", function (event) {
        event.preventDefault();
        window.location.href = "login&register.html";
      });
    }
  });
  
    updateCartCount(); // Update cart count on page load
