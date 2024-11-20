// Function to load categories from the API and display them in the table
async function Categories() {
  const url = 'https://localhost:7159/api/Categories';
  
  try {
    const response = await fetch(url);
    
    if (!response.ok) {
      throw new Error('Failed to fetch categories.'); // Error handling for non-OK response
    }

    const data = await response.json();
    console.log(data);

    const tbody = document.querySelector("tbody");
    let rowHTML = '';

    // Loop through each category and create table rows
    data.forEach(element => {
      rowHTML += `
        <tr>
          <td>
            <div class="form-check">
              <input type="checkbox" class="form-check-input" id="productOne">
              <label class="form-check-label" for="productOne"></label>
              <input type="hidden" id="hidden" class="product-id" value="${element.categoryId}">
            </div>
          </td>
          <td>
            <a href="#" class="text-inherit">
              <div class="d-flex align-items-center">
                <div>
                  <img src="../../../../../../../../../Back-End/E-Commerce/E-Commerce/uploads/${element.image}" alt=""
                    class="img-4by3-md rounded">
                </div>
                <div class="ms-3">
                  <h5 class="mb-0 text-primary-hover">${element.name}</h5>
                </div>
              </div>
            </a>
          </td>
          <td>
            <span class="badge bg-success badge-dot me-1"></span>Active
          </td>
          <td>${element.description}</td>
          <td>
            <span class="dropdown dropstart">
              <a class="btn-icon btn btn-ghost btn-sm rounded-circle" href="#" role="button"
                id="productDropdown1" data-bs-toggle="dropdown" data-bs-offset="-20,20" aria-expanded="false">
                <i class="fe fe-more-vertical"></i>
              </a>
              <span class="dropdown-menu" aria-labelledby="productDropdown1">
                <span class="dropdown-header">Settings</span>
                <a class="dropdown-item" href="#" onclick="navigateEdit(${element.categoryId})">
                  <i class="fe fe-edit dropdown-item-icon"></i>Edit
                </a>
                <button class="dropdown-item" onclick="Delete(${element.categoryId})">
                  <i class="fe fe-trash dropdown-item-icon"></i>Delete
                </button>
              </span>
            </span>
          </td>
        </tr>`;
    });

    // Update the table body with all the rows
    tbody.innerHTML = rowHTML;
  } catch (error) {
    console.error('Error fetching categories:', error);
    alert('Failed to load categories. Please try again later.');
  }
}

// Call the Categories function on page load
Categories();

// Function to navigate to the "Create Category" page
function navigate() {
  window.location.href = "Create.html";
}

// Function to navigate to the "Edit Category" page and save the category ID to localStorage
function navigateEdit(categoryId) {
  localStorage.setItem('EditID', categoryId); // Store the category ID in localStorage
  window.location.href = "editcategory.html"; // Navigate to the edit category page
}

// Function to delete a category by its ID
async function Delete(id) {
  if (confirm("Are you sure you want to delete this category?")) {
    const url = `https://localhost:7159/api/Categories/${id}`;
    
    try {
      const response = await fetch(url, { method: 'DELETE' });
      
      if (response.ok) {
        console.log('Category deleted successfully');
        location.reload(); // Refresh the page to reflect the deleted category
      } else {
        console.error('Failed to delete category');
        alert('Failed to delete category. Please try again.');
      }
    } catch (error) {
      console.error('Error:', error);
      alert('Error deleting category. Please try again.');
    }
  }
}
