// JavaScript cho Header Navigation
// File: wwwroot/js/header.js

document.addEventListener('DOMContentLoaded', function () {

    // ===== 1. MOBILE MENU TOGGLE =====
    const mobileMenuToggle = document.querySelector('.mobile-menu-toggle');
    const mobileMenu = document.querySelector('.mobile-menu');

    if (mobileMenuToggle && mobileMenu) {
        mobileMenuToggle.addEventListener('click', function () {
            // Toggle active class
            this.classList.toggle('active');
            mobileMenu.classList.toggle('active');

            // Prevent body scroll when menu is open
            if (mobileMenu.classList.contains('active')) {
                document.body.style.overflow = 'hidden';
            } else {
                document.body.style.overflow = '';
            }
        });

        // Close mobile menu when clicking outside
        document.addEventListener('click', function (e) {
            if (!mobileMenuToggle.contains(e.target) && !mobileMenu.contains(e.target)) {
                mobileMenuToggle.classList.remove('active');
                mobileMenu.classList.remove('active');
                document.body.style.overflow = '';
            }
        });
    }

    // ===== 2. ACTIVE NAVIGATION LINK =====
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-link, .mobile-nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href');

        // Exact match for home page
        if (currentPath === '/' && href === '/') {
            link.classList.add('active');
        }
        // Partial match for other pages
        else if (currentPath !== '/' && href !== '/' && currentPath.startsWith(href)) {
            link.classList.add('active');
        }
    });

    // ===== 3. STICKY HEADER ON SCROLL =====
    const header = document.querySelector('.main-header');
    let lastScrollTop = 0;

    window.addEventListener('scroll', function () {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        // Add shadow when scrolled
        if (scrollTop > 0) {
            header.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.15)';
        } else {
            header.style.boxShadow = '0 2px 10px rgba(0, 0, 0, 0.1)';
        }

        // Hide header on scroll down, show on scroll up (optional)
        // Uncomment below if you want this behavior
        /*
        if (scrollTop > lastScrollTop && scrollTop > 100) {
            // Scroll down
            header.style.transform = 'translateY(-100%)';
        } else {
            // Scroll up
            header.style.transform = 'translateY(0)';
        }
        */

        lastScrollTop = scrollTop;
    });

    // ===== 4. USER MENU DROPDOWN =====
    const userButton = document.querySelector('.user-button');
    const dropdownMenu = document.querySelector('.dropdown-menu');

    if (userButton && dropdownMenu) {
        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            const userMenu = document.querySelector('.user-menu');
            if (userMenu && !userMenu.contains(e.target)) {
                dropdownMenu.style.opacity = '0';
                dropdownMenu.style.visibility = 'hidden';
            }
        });

        // Handle keyboard navigation
        userButton.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                dropdownMenu.style.opacity = dropdownMenu.style.opacity === '1' ? '0' : '1';
                dropdownMenu.style.visibility = dropdownMenu.style.visibility === 'visible' ? 'hidden' : 'visible';
            }
        });
    }

    // ===== 5. SMOOTH SCROLL FOR ANCHOR LINKS =====
    const anchorLinks = document.querySelectorAll('a[href^="#"]');

    anchorLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');

            // Skip if href is just "#"
            if (href === '#') {
                e.preventDefault();
                return;
            }

            const target = document.querySelector(href);

            if (target) {
                e.preventDefault();

                // Close mobile menu if open
                if (mobileMenu && mobileMenu.classList.contains('active')) {
                    mobileMenuToggle.classList.remove('active');
                    mobileMenu.classList.remove('active');
                    document.body.style.overflow = '';
                }

                // Smooth scroll to target
                const headerHeight = header.offsetHeight;
                const targetPosition = target.getBoundingClientRect().top + window.pageYOffset - headerHeight;

                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });

    // ===== 6. SEARCH FUNCTIONALITY (if needed) =====
    const searchButton = document.querySelector('.search-button');
    const searchModal = document.querySelector('.search-modal');

    if (searchButton && searchModal) {
        searchButton.addEventListener('click', function () {
            searchModal.classList.add('active');
            const searchInput = searchModal.querySelector('input');
            if (searchInput) {
                searchInput.focus();
            }
        });

        // Close search modal on ESC key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && searchModal.classList.contains('active')) {
                searchModal.classList.remove('active');
            }
        });
    }

    // ===== 7. NOTIFICATION INDICATOR =====
    // Example: Add notification badge to user menu if there are unread notifications
    const notificationCount = 3; // This would come from your backend

    if (notificationCount > 0 && userButton) {
        const badge = document.createElement('span');
        badge.className = 'notification-badge';
        badge.textContent = notificationCount;
        badge.style.cssText = `
            position: absolute;
            top: 0;
            right: 0;
            background: #EF4444;
            color: white;
            font-size: 0.75rem;
            font-weight: 700;
            padding: 0.125rem 0.375rem;
            border-radius: 10px;
            min-width: 18px;
            text-align: center;
        `;
        userButton.style.position = 'relative';
        userButton.appendChild(badge);
    }

    // ===== 8. HANDLE WINDOW RESIZE =====
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            // Close mobile menu on resize to desktop
            if (window.innerWidth > 1024) {
                if (mobileMenuToggle) mobileMenuToggle.classList.remove('active');
                if (mobileMenu) mobileMenu.classList.remove('active');
                document.body.style.overflow = '';
            }
        }, 250);
    });

    // ===== 9. LAZY LOAD IMAGES IN DROPDOWN =====
    const dropdownImages = document.querySelectorAll('.dropdown-menu img[data-src]');

    if ('IntersectionObserver' in window && dropdownImages.length > 0) {
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

        dropdownImages.forEach(img => imageObserver.observe(img));
    }

    // ===== 10. ADD LOADING STATE TO LOGOUT LINK =====
    const logoutLink = document.querySelector('.dropdown-item.logout');

    if (logoutLink) {
        logoutLink.addEventListener('click', function (e) {
            // Show loading state
            const originalText = this.textContent;
            this.textContent = 'Đang đăng xuất...';
            this.style.opacity = '0.6';
            this.style.pointerEvents = 'none';

            // If form submission fails, restore original state
            setTimeout(() => {
                this.textContent = originalText;
                this.style.opacity = '1';
                this.style.pointerEvents = 'auto';
            }, 5000);
        });
    }
});

// ===== UTILITY FUNCTIONS =====

// Debounce function for performance
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

// Throttle function for scroll events
function throttle(func, limit) {
    let inThrottle;
    return function () {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

console.log('✅ Header.js loaded successfully!');