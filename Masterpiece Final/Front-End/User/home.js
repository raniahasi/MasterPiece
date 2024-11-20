


(function($) {
    "use strict"

    // Fetch new products dynamically
    function fetchNewProducts() {
        fetch("https://localhost:7159/api/products/GetAllProducts?limit=5&sort=desc")
            .then(response => response.json())
            .then(products => {
                const newProductsList = document.getElementById('new-products-list');
                newProductsList.innerHTML = ''; // Clear any existing content

                products.forEach(product => {
                    const imageUrl = `https://localhost:7159${product.image}`;
                    const productElement = `
                        <div class="product">
                            <div class="product-img">
                                <img src="${imageUrl}" alt="${product.name}">
                                <div class="product-label">
                                </div>
                            </div>
                            <div class="product-body">
                            
                               <h3 class="product-name"><a href="#" onclick="redirectToProductDetail(${
                                product.productId
                              })">${product.name}</a></h3>
                                <h4 class="product-price">${product.price.toFixed(2)} JD
                                    ${product.oldPrice ? `<del class="product-old-price">$${product.oldPrice.toFixed(2)}</del>` : ''}
                                </h4>
                               
                            </div>
                            <div class="add-to-cart">
                                <button class="add-to-cart-btn" onclick="addToCart(${
                                    product.productId
                                  }, '${product.name}', ${
          product.price
        }, '${imageUrl}')"><i class="fa fa-shopping-cart"></i> add to cart</button>
                            </div>
                        </div>
                    `;
                    newProductsList.innerHTML += productElement;
                });

                // Initialize Slick after products are added
                initializeSlick('.products-slick');
            })
            .catch(error => console.error('Error fetching new products:', error));
    }

   

    // Helper function to generate product rating stars
    

    // Initialize slick for dynamic products
    function initializeSlick(selector) {
        $(selector).each(function() {
            var $this = $(this),
                $nav = $this.attr('data-nav');

            $this.slick({
                slidesToShow: 4,
                slidesToScroll: 1,
                autoplay: true,
                infinite: true,
                speed: 900,
                dots: false,
                arrows: true,
                appendArrows: $nav ? $nav : false,
                responsive: [{
                        breakpoint: 991,
                        settings: {
                            slidesToShow: 2,
                            slidesToScroll: 1,
                        }
                    },
                    {
                        breakpoint: 480,
                        settings: {
                            slidesToShow: 1,
                            slidesToScroll: 1,
                        }
                    },
                ]
            });
        });
    }

    // Document ready
    $(document).ready(function() {
        fetchNewProducts();
    });

})(jQuery);



    document.addEventListener('DOMContentLoaded', function() {
    const accountBtn = document.getElementById('accountBtn');
    const accountText = document.getElementById('accountText');
    const logoutBtn = document.getElementById('logoutBtn');
    const username = localStorage.getItem('username');

    if (username) {
        // If logged in, update account text and link
        accountText.textContent = username;
        accountBtn.href = '/cart js and css/account.html';
        // Display Logout Button
        logoutBtn.style.display = 'inline-block'; // Make sure the logout button is visible
        logoutBtn.querySelector('a').addEventListener('click', function(event) {
            event.preventDefault();
            localStorage.removeItem('username'); // Remove username from localStorage
            window.location.href = '/user/home.html'; // Redirect to login page
        });
    } else {
        // If not logged in, redirect to login page on account button click
        accountBtn.addEventListener('click', function(event) {
            event.preventDefault();
            window.location.href = '/user/login&register.html';
        });
    }
});

    



    
    // Fetch categories and display them on the homepage
fetch("https://localhost:7159/api/Categories")
.then(response => response.json())
.then(categories => {
    const categoryCards = document.getElementById('categoryCards');
    categories.forEach(category => {
        const imageUrl = `https://localhost:7159${category.image}`;
        categoryCards.innerHTML += `
            <div class="col-md-3 col-sm-6 mb-4">
                <a href="products.html" class="category-item" onclick="storeCategoryId(${category.categoryId}); storeCategoryName('${category.name}');">
                    <img src="${imageUrl}" alt="${category.name}" class="img-fluid rounded">
                    <p>${category.name}</p>
                </a>
            </div>
        `;
    });
})
.catch(error => console.error("Error fetching categories:", error));

// Function to store the category ID in local storage
function storeCategoryId(categoryId) {
localStorage.setItem('selectedCategoryId', categoryId);
}

function storeCategoryName( categoryName) {
   
    localStorage.setItem('selectedCategoryName', categoryName);
}





function addToCart(productId, name, price, imageUrl) {
    // Check if cart items already exist in local storage
    const cartItems = JSON.parse(localStorage.getItem("cartItems")) || [];
  
    // Check if the product is already in the cart
    const existingItem = cartItems.find((item) => item.ProductId === productId);
    if (existingItem) {
      // Increase the quantity if the item already exists in the cart
      existingItem.Quantity += 1;
    } else {
      // Add the new product to the cart
      cartItems.push({
        ProductId: productId,
        Name: name,
        Price: price,
        ImageUrl: imageUrl,
        Quantity: 1,
      });
    }
  
    // Save updated cart items to local storage
    localStorage.setItem("cartItems", JSON.stringify(cartItems));
  
    // Show a success message
    Swal.fire("Item added to cart successfully!", "", "success");
  
    // Update the cart count (if you have a function to do this)
    updateCartCount();
  }
  
  // Function to update the cart count
  function updateCartCount() {
    const userId = localStorage.getItem("userId");
    if (!userId) return;
  
    fetch(`https://localhost:7159/api/Cart/GetCartItemCount/${userId}`)
      .then((response) => response.json())
      .then((count) => {
        const cartCountElement = document.querySelector(".cart-icon .qty");
        if (cartCountElement) {
          cartCountElement.textContent = count; // Update the displayed count
        }
      })
      .catch((error) => console.error("Error fetching cart count:", error));
  }
  
  // Initial fetch of products
  document.addEventListener("DOMContentLoaded", function () {
    updateCartCount(); // Update the cart count on page load
  });
  
  // Redirect function
  function redirectToProductDetail(productId) {
    localStorage.setItem("selectedProductId", productId); // Storing product ID in localStorage
    window.location.href = "/product detail/product-detail.html"; // Redirect to the detail page
  }