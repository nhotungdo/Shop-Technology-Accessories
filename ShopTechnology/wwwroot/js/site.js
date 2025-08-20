// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Navigation enhancement
document.addEventListener('DOMContentLoaded', function () {
    // Debug navigation links
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            const dataController = this.getAttribute('data-controller');
            const dataAction = this.getAttribute('data-action');
            const onclick = this.getAttribute('onclick');

            console.log('Link clicked:', {
                href: href,
                dataController: dataController,
                dataAction: dataAction,
                onclick: onclick,
                text: this.textContent.trim()
            });

            // If onclick is present, let it handle the navigation
            if (onclick) {
                console.log('Onclick handler found, letting it handle navigation');
                return;
            }

            // If href is empty or #, try to construct from data attributes
            if (!href || href === '#') {
                if (dataController && dataAction) {
                    e.preventDefault();
                    const newHref = `/${dataController}/${dataAction}`;
                    console.log('Redirecting to:', newHref);
                    window.location.href = newHref;
                }
            }
        });
    });

    // Add smooth scrolling to all links
    const links = document.querySelectorAll('a[href^="#"]');
    links.forEach(link => {
        link.addEventListener('click', function (e) {
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

    // Close mobile menu when clicking on a link
    const navbarCollapse = document.querySelector('.navbar-collapse');

    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (navbarCollapse && navbarCollapse.classList.contains('show')) {
                const bsCollapse = new bootstrap.Collapse(navbarCollapse);
                bsCollapse.hide();
            }
        });
    });

    // Add loading indicator for navigation
    const allLinks = document.querySelectorAll('a[href]:not([href^="#"]):not([href^="javascript:"]):not([href^="mailto:"]):not([href^="tel:"])');
    allLinks.forEach(link => {
        link.addEventListener('click', function () {
            // Add loading class to body
            document.body.classList.add('loading');

            // Remove loading class after a short delay
            setTimeout(() => {
                document.body.classList.remove('loading');
            }, 1000);
        });
    });

    // Highlight current page in navigation
    const currentPath = window.location.pathname;

    navLinks.forEach(item => {
        const href = item.getAttribute('href');
        if (href && currentPath.includes(href.split('/')[1])) {
            item.classList.add('active');
        }
    });

    // Debug all navigation links
    console.log('Current path:', currentPath);
    console.log('Navigation links found:', navLinks.length);

    navLinks.forEach((link, index) => {
        const href = link.getAttribute('href');
        const onclick = link.getAttribute('onclick');
        const text = link.textContent.trim();

        console.log(`Link ${index + 1}:`, {
            text: text,
            href: href,
            onclick: onclick,
            hasOnclick: !!onclick
        });

        // Add visual indicator for broken links
        if (!href || href === '#' || href === '') {
            link.style.border = '1px solid red';
            console.warn('Broken link detected:', text);
        }

        // Add click event listener for debugging
        link.addEventListener('click', function (e) {
            console.log('Link clicked:', {
                text: this.textContent.trim(),
                href: this.getAttribute('href'),
                currentPath: window.location.pathname
            });
        });
    });

    // Test URL generation
    console.log('Test URLs:');
    console.log('Home:', '/');
    console.log('Product:', '/Product');
    console.log('Login:', '/Account/Login');
    console.log('Register:', '/Account/Register');
});

// Add loading animation
document.addEventListener('DOMContentLoaded', function () {
    // Show page content with fade-in effect
    document.body.style.opacity = '0';
    setTimeout(() => {
        document.body.style.transition = 'opacity 0.5s ease-in-out';
        document.body.style.opacity = '1';
    }, 100);
});

// Form validation enhancement
document.addEventListener('DOMContentLoaded', function () {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const requiredFields = form.querySelectorAll('[required]');
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
});
