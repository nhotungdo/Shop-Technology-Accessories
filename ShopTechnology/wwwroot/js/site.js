// Site JavaScript - Shop Technology

// Global variables
let cartCount = 0;

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <span class="notification-message">${message}</span>
            <button class="notification-close" onclick="this.parentElement.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Cart functions
async function addToCart(productId, quantity = 1) {
    try {
        const response = await fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({ productId, quantity })
        });
        
        const result = await response.json();
        
        if (result.success) {
            updateCartCount();
            showNotification('Sản phẩm đã được thêm vào giỏ hàng!', 'success');
        } else {
            showNotification('Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.', 'error');
        }
    } catch (error) {
        console.error('Error adding to cart:', error);
        showNotification('Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.', 'error');
    }
}

async function updateCartCount() {
    try {
        const response = await fetch('/Cart/GetCartCount');
        const result = await response.json();
        cartCount = result.count;
        
        const cartCountElement = document.getElementById('cartCount');
        if (cartCountElement) {
            cartCountElement.textContent = cartCount;
        }
    } catch (error) {
        console.error('Error updating cart count:', error);
    }
}

async function removeFromCart(productId) {
    try {
        const response = await fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({ productId })
        });
        
        const result = await response.json();
        
        if (result.success) {
            updateCartCount();
            // Remove the cart item from DOM
            const cartItem = document.querySelector(`[data-product-id="${productId}"]`);
            if (cartItem) {
                cartItem.remove();
            }
            showNotification('Sản phẩm đã được xóa khỏi giỏ hàng.', 'success');
        } else {
            showNotification('Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng.', 'error');
        }
    } catch (error) {
        console.error('Error removing from cart:', error);
        showNotification('Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng.', 'error');
    }
}

async function updateQuantity(productId, quantity) {
    try {
        const response = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({ productId, quantity })
        });
        
        const result = await response.json();
        
        if (result.success) {
            updateCartCount();
            // Update the total price in DOM
            updateCartTotals();
        } else {
            showNotification('Có lỗi xảy ra khi cập nhật số lượng.', 'error');
        }
    } catch (error) {
        console.error('Error updating quantity:', error);
        showNotification('Có lỗi xảy ra khi cập nhật số lượng.', 'error');
    }
}

function updateCartTotals() {
    // This function should recalculate cart totals based on current quantities
    // Implementation depends on the cart page structure
    const cartItems = document.querySelectorAll('.cart-item');
    let subtotal = 0;
    
    cartItems.forEach(item => {
        const quantity = parseInt(item.querySelector('.quantity-input').value);
        const price = parseFloat(item.querySelector('.cart-item-price').dataset.price);
        subtotal += quantity * price;
    });
    
    const subtotalElement = document.querySelector('.cart-subtotal');
    if (subtotalElement) {
        subtotalElement.textContent = formatCurrency(subtotal);
    }
}

// Wishlist functions
async function addToWishlist(productId) {
    try {
        const response = await fetch('/Wishlist/AddToWishlist', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({ productId })
        });
        
        const result = await response.json();
        
        if (result.success) {
            showNotification('Sản phẩm đã được thêm vào wishlist!', 'success');
        } else {
            showNotification('Có lỗi xảy ra khi thêm sản phẩm vào wishlist.', 'error');
        }
    } catch (error) {
        console.error('Error adding to wishlist:', error);
        showNotification('Có lỗi xảy ra khi thêm sản phẩm vào wishlist.', 'error');
    }
}

// Search functions
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const query = this.value.trim();
            
            if (query.length >= 2) {
                searchTimeout = setTimeout(() => {
                    performSearch(query);
                }, 500);
            }
        });
    }
}

async function performSearch(query) {
    try {
        const response = await fetch(`/Product/Search?searchTerm=${encodeURIComponent(query)}`);
        const results = await response.json();
        
        displaySearchResults(results);
    } catch (error) {
        console.error('Error performing search:', error);
    }
}

function displaySearchResults(results) {
    const searchResultsContainer = document.getElementById('searchResults');
    if (!searchResultsContainer) return;
    
    if (results.length === 0) {
        searchResultsContainer.innerHTML = '<p>Không tìm thấy sản phẩm nào.</p>';
        return;
    }
    
    const resultsHtml = results.map(product => `
        <div class="search-result-item">
            <img src="${product.imageUrl}" alt="${product.name}" />
            <div class="search-result-content">
                <h4>${product.name}</h4>
                <p class="price">${formatCurrency(product.price)}</p>
            </div>
        </div>
    `).join('');
    
    searchResultsContainer.innerHTML = resultsHtml;
}

// Form validation
function validateForm(formElement) {
    const inputs = formElement.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

// Image lazy loading
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                observer.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
}

// Mobile menu toggle
function initializeMobileMenu() {
    const menuToggle = document.querySelector('.mobile-menu-toggle');
    const mobileMenu = document.querySelector('.mobile-menu');
    
    if (menuToggle && mobileMenu) {
        menuToggle.addEventListener('click', function() {
            mobileMenu.classList.toggle('active');
            this.classList.toggle('active');
        });
    }
}

// Smooth scrolling
function initializeSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            const targetElement = document.querySelector(targetId);
            
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Product image gallery
function initializeProductGallery() {
    const mainImage = document.querySelector('.main-image');
    const thumbnails = document.querySelectorAll('.thumbnail');
    
    if (mainImage && thumbnails.length > 0) {
        thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', function() {
                const newSrc = this.src;
                mainImage.src = newSrc;
                
                // Update active thumbnail
                thumbnails.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
            });
        });
    }
}

// Quantity selector
function initializeQuantitySelector() {
    const quantitySelectors = document.querySelectorAll('.quantity-selector');
    
    quantitySelectors.forEach(selector => {
        const minusBtn = selector.querySelector('.quantity-btn[data-action="minus"]');
        const plusBtn = selector.querySelector('.quantity-btn[data-action="plus"]');
        const input = selector.querySelector('.quantity-input');
        
        if (minusBtn && plusBtn && input) {
            minusBtn.addEventListener('click', function() {
                const currentValue = parseInt(input.value);
                if (currentValue > 1) {
                    input.value = currentValue - 1;
                    input.dispatchEvent(new Event('change'));
                }
            });
            
            plusBtn.addEventListener('click', function() {
                const currentValue = parseInt(input.value);
                input.value = currentValue + 1;
                input.dispatchEvent(new Event('change'));
            });
        }
    });
}

// Initialize all functions when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    updateCartCount();
    initializeSearch();
    initializeLazyLoading();
    initializeMobileMenu();
    initializeSmoothScrolling();
    initializeProductGallery();
    initializeQuantitySelector();
    
    // Add CSRF token to all AJAX requests
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        // Override fetch to include CSRF token
        const originalFetch = window.fetch;
        window.fetch = function(url, options = {}) {
            if (options.method && options.method !== 'GET') {
                options.headers = {
                    ...options.headers,
                    'RequestVerificationToken': token
                };
            }
            return originalFetch(url, options);
        };
    }
});

// Export functions for global use
window.ShopTechnology = {
    addToCart,
    removeFromCart,
    updateQuantity,
    addToWishlist,
    showNotification,
    formatCurrency,
    validateForm
};
