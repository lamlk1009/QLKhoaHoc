// JavaScript cho trang đăng ký SMEMBER
// File: wwwroot/js/register.js

document.addEventListener('DOMContentLoaded', function () {

    // ===== 1. CHỨC NĂNG SHOW/HIDE PASSWORD =====
    const togglePasswordButtons = document.querySelectorAll('.toggle-password');

    togglePasswordButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            // Tìm input password trong cùng wrapper
            const wrapper = this.closest('.password-input-wrapper');
            const passwordInput = wrapper.querySelector('.form-input');

            // Toggle type giữa password và text
            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                // Đổi icon thành "eye-off"
                this.innerHTML = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                        <line x1="1" y1="1" x2="23" y2="23"></line>
                    </svg>
                `;
            } else {
                passwordInput.type = 'password';
                // Đổi icon về "eye"
                this.innerHTML = `
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                        <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                `;
            }
        });
    });

    // ===== 2. KIỂM TRA MẠNH ĐỘ MẬT KHẨU =====
    const passwordInput = document.querySelector('input[name="Password"]');

    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            const password = this.value;
            const strength = checkPasswordStrength(password);

            // Log để debug (có thể bỏ trong production)
            console.log('Password strength:', strength);
        });
    }

    // Hàm kiểm tra độ mạnh mật khẩu
    function checkPasswordStrength(password) {
        let strength = 0;

        if (password.length >= 6) strength++;
        if (password.length >= 8) strength++;
        if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
        if (/\d/.test(password)) strength++;
        if (/[@$!%*#?&]/.test(password)) strength++;

        return strength; // 0-5
    }

    // ===== 3. KIỂM TRA KHỚP MẬT KHẨU =====
    const confirmPasswordInput = document.querySelector('input[name="ConfirmPassword"]');

    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('input', function () {
            if (this.value && this.value !== passwordInput.value) {
                this.setCustomValidity('Mật khẩu nhập lại không khớp');
            } else {
                this.setCustomValidity('');
            }
        });

        passwordInput.addEventListener('input', function () {
            if (confirmPasswordInput.value) {
                confirmPasswordInput.dispatchEvent(new Event('input'));
            }
        });
    }

    // ===== 4. FORMAT NGÀY SINH (dd/mm/yyyy) =====
    const dateInput = document.querySelector('input[name="DateOfBirth"]');

    if (dateInput) {
        // Đặt max date là ngày hiện tại (không cho chọn ngày tương lai)
        const today = new Date().toISOString().split('T')[0];
        dateInput.setAttribute('max', today);

        // Đặt min date là 100 năm trước (hợp lý cho tuổi người dùng)
        const minDate = new Date();
        minDate.setFullYear(minDate.getFullYear() - 100);
        dateInput.setAttribute('min', minDate.toISOString().split('T')[0]);

        // Xử lý class empty-date
        if (!dateInput.value) {
            dateInput.classList.add('empty-date');
        }

        dateInput.addEventListener('change', function () {
            if (this.value) {
                this.classList.remove('empty-date');
            } else {
                this.classList.add('empty-date');
            }
        });
    }

    // ===== 5. FORMAT SỐ ĐIỆN THOẠI =====
    const phoneInput = document.querySelector('input[name="PhoneNumber"]');

    if (phoneInput) {
        phoneInput.addEventListener('input', function () {
            // Chỉ cho phép nhập số và dấu +
            this.value = this.value.replace(/[^\d+]/g, '');

            // Giới hạn độ dài
            if (this.value.length > 12) {
                this.value = this.value.slice(0, 12);
            }
        });

        phoneInput.addEventListener('blur', function () {
            // Validate khi blur
            if (this.value && !isValidVietnamesePhone(this.value)) {
                this.setCustomValidity('Số điện thoại không hợp lệ');
            } else {
                this.setCustomValidity('');
            }
        });
    }

    // ===== 6. XỬ LÝ SUBMIT FORM =====
    const registerForm = document.querySelector('form[action*="Register"]');

    if (registerForm) {
        registerForm.addEventListener('submit', function (e) {
            // Kiểm tra checkbox "Đồng ý điều khoản"
            const acceptTermsCheckbox = document.querySelector('input[name="AcceptTerms"]');

            if (acceptTermsCheckbox && !acceptTermsCheckbox.checked) {
                e.preventDefault();
                alert('Bạn cần đồng ý với Điều khoản sử dụng và Chính sách bảo mật để tiếp tục đăng ký.');
                acceptTermsCheckbox.focus();

                // Scroll đến checkbox
                acceptTermsCheckbox.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return false;
            }

            // Thêm loading state vào nút submit
            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent = 'Đang xử lý...';
                submitButton.style.opacity = '0.7';
            }
        });
    }

    // ===== 7. XỬ LÝ NÚT SOCIAL LOGIN =====
    const socialButtons = document.querySelectorAll('.social-btn');

    socialButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const provider = this.textContent.trim();
            console.log(`Đăng ký bằng ${provider}`);

            // Thêm logic đăng ký social ở đây
            alert(`Chức năng đăng ký bằng ${provider} đang được phát triển.`);
        });
    });

    // ===== 8. AUTO-FOCUS VÀO TRƯỜNG ĐẦU TIÊN CÓ LỖI =====
    const firstError = document.querySelector('.field-validation-error:not(:empty)');
    if (firstError) {
        const errorInput = firstError.previousElementSibling;
        if (errorInput && (errorInput.tagName === 'INPUT' || errorInput.classList.contains('password-input-wrapper'))) {
            // Nếu là password wrapper, focus vào input bên trong
            const actualInput = errorInput.classList.contains('password-input-wrapper')
                ? errorInput.querySelector('input')
                : errorInput;

            if (actualInput) {
                actualInput.focus();
                actualInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
    }

    // ===== 9. HIỆU ỨNG KHI SCROLL =====
    const registerContainer = document.querySelector('.register-container');

    if (registerContainer) {
        // Thêm class để animate khi load trang
        setTimeout(() => {
            registerContainer.classList.add('loaded');
        }, 100);
    }

    // ===== 10. XỬ LÝ EMAIL VALIDATION =====
    const emailInput = document.querySelector('input[name="Email"]');

    if (emailInput) {
        emailInput.addEventListener('blur', function () {
            if (this.value && !isValidEmail(this.value)) {
                this.setCustomValidity('Email không hợp lệ');
            } else {
                this.setCustomValidity('');
            }
        });
    }

    // ===== 11. THÔNG BÁO TẠM THỜI (TOAST) =====
    // Hiển thị thông báo thành công/lỗi nếu có TempData
    const successMessage = document.querySelector('[data-success-message]');
    const errorMessage = document.querySelector('[data-error-message]');

    if (successMessage) {
        showToast(successMessage.getAttribute('data-success-message'), 'success');
    }

    if (errorMessage) {
        showToast(errorMessage.getAttribute('data-error-message'), 'error');
    }
});

// ===== UTILITY FUNCTIONS =====

// Validate email format
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Validate phone number (Vietnamese format)
function isValidVietnamesePhone(phone) {
    const phoneRegex = /^(0|\+84)(\d{9,10})$/;
    return phoneRegex.test(phone);
}

// Format date to dd/mm/yyyy
function formatDate(date) {
    if (!(date instanceof Date)) {
        date = new Date(date);
    }

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}

// Hiển thị toast notification
function showToast(message, type = 'info') {
    // Tạo element toast
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 1rem 1.5rem;
        background-color: ${type === 'success' ? '#10B981' : type === 'error' ? '#EF4444' : '#3B82F6'};
        color: white;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        z-index: 9999;
        animation: slideIn 0.3s ease;
    `;

    document.body.appendChild(toast);

    // Tự động ẩn sau 3 giây
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => {
            document.body.removeChild(toast);
        }, 300);
    }, 3000);
}

// CSS animations cho toast
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

console.log('✅ Register.js loaded successfully!');