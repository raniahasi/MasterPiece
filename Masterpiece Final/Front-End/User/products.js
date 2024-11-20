// Function to display products on the page
function displayProducts(products) {
  console.log("Products received for display:", products); // Debugging: Check the received products

  // Filter out products that are not visible
  const visibleProducts = products.filter(product => product.isVisible);
  console.log("Filtered visible products:", visibleProducts); // Debugging: Check the filtered products

  const productsContainer = document.getElementById("product-list");
  productsContainer.innerHTML = "";

  if (!visibleProducts.length) {
    productsContainer.innerHTML = "<p>No products found.</p>";
    return;
  }

  visibleProducts.forEach((product) => {
    const imageUrl = `https://localhost:7159${product.image}`;
    productsContainer.innerHTML += `
      <div class="col-md-4 col-xs-6 product-item">
        <div class="product">
          <div class="product-img">
            <img src="${imageUrl}" alt="${product.name}" class="card-img-top">
            <div class="product-overlay">
              <button class="add-to-cart-btn" onclick="addToCart(${product.productId}, '${product.name}', ${product.price}, '${imageUrl}')">
                <i class="fa fa-shopping-cart"></i> add to cart
              </button>
            </div>
          </div>
          <div class="product-body">
            <p class="product-category">${product.category ? product.category.name : ""}</p>
            <h3 class="product-name"><a href="#" onclick="redirectToProductDetail(${product.productId})">${product.name}</a></h3>
            <h4 class="product-price">${product.price.toFixed(2)} JD</h4>
          </div>
        </div>
      </div>
    `;
  });
}

// Function to fetch products by category on page load
document.addEventListener("DOMContentLoaded", function () {
  const categoryId = localStorage.getItem("selectedCategoryId");
  console.log("Selected Category ID from localStorage:", categoryId); // Debugging: Check the category ID

  if (categoryId) {
    fetchProductsByCategory(categoryId);
  } else {
    console.log("No category ID found in localStorage."); // Debugging: If no category ID is found
  }

  updateCartCount(); // Update cart count on page load
});



// Function to fetch products by category
function fetchProductsByCategory(categoryId) {
  console.log("Fetching products for category ID:", categoryId); // Debugging: Check the category ID being used in the fetch

  fetch(`https://localhost:7159/api/Products/ByCategoryId/${categoryId}`)
    .then((response) => {
      console.log("API response status:", response.status); // Debugging: Check the response status
      if (!response.ok) throw new Error("Failed to fetch products");
      return response.json();
    })
    .then((products) => {
      console.log("Products received from API:", products); // Debugging: Log the products received from the API
      displayProducts(products);
    })
    .catch((error) => {
      console.error("Error fetching products by category:", error);
      const productsContainer = document.getElementById("product-list");
      productsContainer.innerHTML = "<p>Error loading products.</p>";
    });
}

// Function to fetch categories with product counts for filtering
fetch("https://localhost:7159/api/Categories/categories-with-product-count")
  .then((response) => {
    console.log("Categories API response status:", response.status); // Debugging: Check the response status
    return response.json();
  })
  .then((categories) => {
    console.log("Categories received from API:", categories); // Debugging: Log the categories received
    const categoriesContainer = document.getElementById("category-filter");
    categoriesContainer.innerHTML = "";

    categories.forEach((category) => {
      categoriesContainer.innerHTML += `
        <div class="input-checkbox">
          <input type="checkbox" id="category-${category.categoryId}" data-category-id="${category.categoryId}">
          <label for="category-${category.categoryId}">
            <span></span>
            ${category.name} <small>(${category.productCount})</small>
          </label>
        </div>
      `;
    });

    // Attach event listeners for filter checkboxes
    document.querySelectorAll(".input-checkbox input").forEach((checkbox) => {
      checkbox.addEventListener("change", filterProducts);
    });
  })
  .catch((error) => console.error("Error fetching categories with counts:", error));

// Function to filter products based on checked categories
function filterProducts() {
  const selectedCategories = Array.from(
    document.querySelectorAll(".input-checkbox input:checked")
  ).map((cb) => parseInt(cb.dataset.categoryId));

  console.log("Selected categories for filtering:", selectedCategories); // Debugging: Check selected categories

  fetch("https://localhost:7159/api/products/GetAllProducts")
    .then((response) => response.json())
    .then((allProducts) => {
      console.log("All products received from API:", allProducts); // Debugging: Log all products received

      // Filter products based on categories and visibility
      const filteredProducts = selectedCategories.length
        ? allProducts.filter((product) => selectedCategories.includes(product.categoryId) && product.isVisible)
        : allProducts.filter(product => product.isVisible);

      console.log("Filtered products to display:", filteredProducts); // Debugging: Check the filtered products
      displayProducts(filteredProducts);
    })
    .catch((error) => console.error("Error fetching products:", error));
}

// Function to update the price input when the slider is moved
function updatePriceInput(value) {
  document.getElementById("price-max").value = value;
}

// Function to fetch and filter products by price range
function filterByPrice() {
  // Get the minimum and maximum price values
  const minPrice = parseFloat(document.getElementById("price-min").value);
  const maxPrice = parseFloat(document.getElementById("price-max").value);

  // Log the selected price range for debugging
  console.log("Filtering products by price range:", minPrice, maxPrice);

  // Fetch products within the selected price range from the API
  fetch(`https://localhost:7159/api/Products/ByPriceRange?minPrice=${minPrice}&maxPrice=${maxPrice}`)
    .then((response) => {
      if (!response.ok) throw new Error("Failed to fetch products within the specified price range.");
      return response.json();
    })
    .then((products) => {
      console.log("Products received for price range:", products); // Debugging: Check the products received
      displayProducts(products); // Call the function to display the filtered products
    })
    .catch((error) => {
      console.error("Error fetching products by price range:", error);
      const productsContainer = document.getElementById("product-list");
      productsContainer.innerHTML = "<p>No products found within the specified price range.</p>";
    });
}
// Function to fetch all products
function fetchAllProducts() {
  console.log("Fetching all products..."); // Debugging: Check if the button click is working

  // Make sure the URL is correct
  fetch("https://localhost:7159/api/Products/GetAllProducts")
    .then((response) => {
      console.log("API response status:", response.status); // Debugging: Check the response status
      if (!response.ok) throw new Error("Failed to fetch all products.");
      return response.json();
    })
    .then((products) => {
      console.log("All products received:", products); // Debugging: Check the products received
      displayProducts(products); // Call the function to display all products
    })
    .catch((error) => {
      console.error("Error fetching all products:", error);
      const productsContainer = document.getElementById("product-list");
      productsContainer.innerHTML = "<p>Error loading all products.</p>";
    });
}

// Function to add products to the cart
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