// Site JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Add loading state to buttons
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function() {
            if (!this.classList.contains('no-loading')) {
                this.classList.add('loading');
                const originalText = this.innerHTML;
                this.innerHTML = '<span class="spinner"></span> Đang xử lý...';
                
                // Reset after 2 seconds (for demo purposes)
                setTimeout(() => {
                    this.classList.remove('loading');
                    this.innerHTML = originalText;
                }, 2000);
            }
        });
    });

    // Add fade-in animation to elements
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe elements for animation
    document.querySelectorAll('.card, .product-card, .category-card').forEach(el => {
        observer.observe(el);
    });

    // Mobile menu toggle
    const navbarToggler = document.querySelector('.navbar-toggler');
    const navbarCollapse = document.querySelector('.navbar-collapse');
    
    if (navbarToggler && navbarCollapse) {
        navbarToggler.addEventListener('click', function() {
            navbarCollapse.classList.toggle('show');
        });

        // Close mobile menu when clicking outside
        document.addEventListener('click', function(e) {
            if (!navbarToggler.contains(e.target) && !navbarCollapse.contains(e.target)) {
                navbarCollapse.classList.remove('show');
            }
        });
    }

    // Search functionality
    const searchInput = document.querySelector('#searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            const productCards = document.querySelectorAll('.product-card');
            
            productCards.forEach(card => {
                const productName = card.querySelector('.card-title').textContent.toLowerCase();
                if (productName.includes(searchTerm)) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }

    // Quantity selector
    document.querySelectorAll('.quantity-selector').forEach(selector => {
        const minusBtn = selector.querySelector('.quantity-btn:first-child');
        const plusBtn = selector.querySelector('.quantity-btn:last-child');
        const input = selector.querySelector('.quantity-input');
        
        if (minusBtn && plusBtn && input) {
            minusBtn.addEventListener('click', function() {
                let value = parseInt(input.value) || 1;
                if (value > 1) {
                    input.value = value - 1;
                }
            });
            
            plusBtn.addEventListener('click', function() {
                let value = parseInt(input.value) || 1;
                input.value = value + 1;
            });
        }
    });

    // Wishlist toggle
    document.querySelectorAll('.wishlist-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const icon = this.querySelector('i');
            if (icon.classList.contains('far')) {
                icon.classList.remove('far');
                icon.classList.add('fas');
                icon.style.color = '#dc3545';
            } else {
                icon.classList.remove('fas');
                icon.classList.add('far');
                icon.style.color = '';
            }
        });
    });

    // Image gallery for product details
    const mainImage = document.querySelector('#mainImage');
    const thumbnails = document.querySelectorAll('.thumbnail');
    
    if (mainImage && thumbnails.length > 0) {
        thumbnails.forEach(thumb => {
            thumb.addEventListener('click', function() {
                const imgSrc = this.querySelector('img').src;
                mainImage.src = imgSrc;
                
                // Update active thumbnail
                thumbnails.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
            });
        });
    }

    // Newsletter subscription
    const newsletterForm = document.querySelector('#newsletterForm');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function(e) {
            e.preventDefault();
            const email = this.querySelector('input[type="email"]').value;
            
            // Show success message
            const successMessage = document.createElement('div');
            successMessage.className = 'alert alert-success mt-3';
            successMessage.textContent = 'Cảm ơn bạn đã đăng ký nhận tin!';
            
            this.appendChild(successMessage);
            this.reset();
            
            // Remove message after 3 seconds
            setTimeout(() => {
                successMessage.remove();
            }, 3000);
        });
    }

    // Back to top button
    const backToTopBtn = document.createElement('button');
    backToTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTopBtn.className = 'btn btn-primary position-fixed';
    backToTopBtn.style.cssText = 'bottom: 20px; right: 20px; z-index: 1000; width: 50px; height: 50px; border-radius: 50%; display: none;';
    document.body.appendChild(backToTopBtn);

    window.addEventListener('scroll', function() {
        if (window.pageYOffset > 300) {
            backToTopBtn.style.display = 'block';
        } else {
            backToTopBtn.style.display = 'none';
        }
    });

    backToTopBtn.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });

    // Lazy loading for images
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));

    // Form validation
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(e) {
            const requiredFields = this.querySelectorAll('[required]');
            let isValid = true;
            
            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    isValid = false;
                    field.classList.add('is-invalid');
                } else {
                    field.classList.remove('is-invalid');
                }
            });
            
            if (!isValid) {
                e.preventDefault();
                alert('Vui lòng điền đầy đủ thông tin bắt buộc.');
            }
        });
    });

    // Price range slider
    const priceRange = document.querySelector('#priceRange');
    const priceValue = document.querySelector('#priceValue');
    
    if (priceRange && priceValue) {
        priceRange.addEventListener('input', function() {
            priceValue.textContent = this.value + ' ₫';
        });
    }

    // Category filter
    document.querySelectorAll('.category-filter').forEach(filter => {
        filter.addEventListener('change', function() {
            const categoryId = this.value;
            const productCards = document.querySelectorAll('.product-card');
            
            if (categoryId === '') {
                productCards.forEach(card => card.style.display = 'block');
            } else {
                productCards.forEach(card => {
                    const cardCategoryId = card.dataset.categoryId;
                    if (cardCategoryId === categoryId) {
                        card.style.display = 'block';
                    } else {
                        card.style.display = 'none';
                    }
                });
            }
        });
    });

    // Sort products
    const sortSelect = document.querySelector('#sortSelect');
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            const sortBy = this.value;
            const productContainer = document.querySelector('.products-grid') || document.querySelector('.row');
            const productCards = Array.from(productContainer.querySelectorAll('.product-card'));
            
            productCards.sort((a, b) => {
                let aValue, bValue;
                
                switch(sortBy) {
                    case 'price-asc':
                        aValue = parseFloat(a.querySelector('.current-price').textContent.replace(/[^\d]/g, ''));
                        bValue = parseFloat(b.querySelector('.current-price').textContent.replace(/[^\d]/g, ''));
                        return aValue - bValue;
                    case 'price-desc':
                        aValue = parseFloat(a.querySelector('.current-price').textContent.replace(/[^\d]/g, ''));
                        bValue = parseFloat(b.querySelector('.current-price').textContent.replace(/[^\d]/g, ''));
                        return bValue - aValue;
                    case 'name-asc':
                        aValue = a.querySelector('.card-title').textContent.toLowerCase();
                        bValue = b.querySelector('.card-title').textContent.toLowerCase();
                        return aValue.localeCompare(bValue);
                    case 'name-desc':
                        aValue = a.querySelector('.card-title').textContent.toLowerCase();
                        bValue = b.querySelector('.card-title').textContent.toLowerCase();
                        return bValue.localeCompare(aValue);
                    default:
                        return 0;
                }
            });
            
            productCards.forEach(card => productContainer.appendChild(card));
        });
    }

    console.log('Site JavaScript loaded successfully!');
});

// Global functions
function addToCart(productId) {
    console.log('Adding product to cart:', productId);
    // Implement AJAX call to add product to cart
    showNotification('Đã thêm sản phẩm vào giỏ hàng!', 'success');
}

function addToWishlist(productId) {
    console.log('Adding product to wishlist:', productId);
    // Implement AJAX call to add product to wishlist
    showNotification('Đã thêm sản phẩm vào danh sách yêu thích!', 'success');
}

function quickView(productId) {
    console.log('Quick view product:', productId);
    window.location.href = `/Product/Details/${productId}`;
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 3000);
}

function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
