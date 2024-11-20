// Function to fetch and display all products
function fetchProducts() {
  fetch("https://localhost:7159/api/products")
    .then(response => response.json())
    .then(products => {
      // Filter out products that are not visible
      const visibleProducts = products.filter(product => product.isVisible);
      displayProducts(visibleProducts);
    })
    .catch(error => console.error("Error fetching products:", error));
}

// Function to display products on the page
function displayProducts(products) {
  const productsContainer = document.getElementById("product-list");
  productsContainer.innerHTML = ""; // Clear existing products

  if (!products.length) {
    productsContainer.innerHTML = "<p>No products found.</p>";
  } else {
    products.forEach((product) => {
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
              <h3 class="product-name"><a href="#" onclick="redirectToProductDetail(${product.productId})">${product.name}</a></h3>
              <h4 class="product-price">JD${product.price.toFixed(2)}</h4>
            </div>
          </div>
        </div>
      `;
    });
  }
}

// Attach event listeners for category filter checkboxes
document.addEventListener("DOMContentLoaded", function () {
  fetchProducts(); // Fetch and display all products initially
  document.querySelectorAll(".input-checkbox input").forEach(checkbox => {
    checkbox.addEventListener("change", filterProducts);
  });
  updateCartCount(); // Update the cart count on page load
  updateBreadcrumb(); // Update breadcrumb based on category if applicable
});

// Function to filter products based on checked categories
function filterProducts() {
  const selectedCategories = Array.from(
    document.querySelectorAll(".input-checkbox input:checked")
  ).map(cb => parseInt(cb.dataset.categoryId));

  fetch("https://localhost:7159/api/products")
    .then(response => response.json())
    .then(allProducts => {
      // Filter products by category and visibility
      const filteredProducts = allProducts.filter(product =>
        selectedCategories.length > 0
          ? selectedCategories.includes(product.categoryId) && product.isVisible
          : product.isVisible
      );
      displayProducts(filteredProducts);
    })
    .catch(error => console.error("Error fetching products:", error));
}

// Function to add products to the cart
function addToCart(productId, name, price, imageUrl) {
  const userId = localStorage.getItem("userId");
  if (!userId) {
    Swal.fire("You must be logged in to add items to the cart.", "", "error");
    return;
  }

  const cartData = {
    ProductId: productId,
    UserId: parseInt(userId),
    Quantity: 1,
  };

  fetch("https://localhost:7159/api/Cart/CreateNewCartItem", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(cartData),
  })
    .then(response => {
      if (!response.ok) throw new Error("Failed to add item to cart.");
      return response.json();
    })
    .then(data => {
      Swal.fire(data.msg || "Item added to cart successfully!", "", "success");
      updateCartCount();
    })
    .catch(error => {
      console.error("Error adding item to cart:", error);
      Swal.fire("Failed to add item to cart.", error.message, "error");
    });
}

// Function to update the cart count
function updateCartCount() {
  const userId = localStorage.getItem("userId");
  if (!userId) return;

  fetch(`https://localhost:7159/api/Cart/GetCartItemCount/${userId}`)
    .then(response => response.json())
    .then(count => {
      const cartCountElement = document.querySelector(".cart-icon .qty");
      if (cartCountElement) {
        cartCountElement.textContent = count; // Update the displayed count
      }
    })
    .catch(error => console.error("Error fetching cart count:", error));
}

// Breadcrumb update function
function updateBreadcrumb() {
  const categoryName = localStorage.getItem("selectedCategoryName");
  const breadcrumbTree = document.querySelector(".breadcrumb-tree");
  if (categoryName) {
    breadcrumbTree.innerHTML = `
      <li><a href="/User/home.html">Home</a></li>
      <li><a href="/User/products.html">Categories</a></li>
      <li class="active">${categoryName}</li>
    `;
  }
}

// Redirect function
function redirectToProductDetail(productId) {
  localStorage.setItem("selectedProductId", productId); // Storing product ID in localStorage
  window.location.href = "/product detail/product-detail.html"; // Redirect to the detail page
}

// Fetch categories with product counts for filtering
fetch("https://localhost:7159/api/Categories/categories-with-product-count")
  .then(response => response.json())
  .then(categories => {
    const categoriesContainer = document.getElementById("category-filter");
    categoriesContainer.innerHTML = "";
    categories.forEach(category => {
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
    document.querySelectorAll(".input-checkbox input").forEach(checkbox => {
      checkbox.addEventListener("change", filterProducts);
    });
  })
  .catch(error => console.error("Error fetching categories with counts:", error));
