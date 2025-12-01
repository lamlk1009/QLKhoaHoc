// Nút quay lại trang admin (giữ nguyên nếu bạn đang dùng)
document.addEventListener('DOMContentLoaded', function () {
    const backToSiteBtn = document.querySelector('.back-to-site');

    if (backToSiteBtn) {
        backToSiteBtn.addEventListener('click', function (e) {
            console.log('Nút "Về trang chủ" được click');
            console.log('Href:', this.getAttribute('href'));
        });
    }
});

// Toggle sidebar + chart
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('adminSidebar');
    const mainContent = document.getElementById('adminMain');
    const sidebarToggle = document.getElementById('sidebarToggle');

    if (sidebar && mainContent && sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');

            const icon = sidebarToggle.querySelector('svg');
            if (!icon) return;

            if (sidebar.classList.contains('collapsed')) {
                icon.innerHTML = `
                    <line x1="5" y1="12" x2="19" y2="12"></line>
                    <polyline points="12 5 19 12 12 19"></polyline>
                `;
            } else {
                icon.innerHTML = `
                    <line x1="19" y1="12" x2="5" y2="12"></line>
                    <polyline points="12 19 5 12 12 5"></polyline>
                `;
            }
        });
    }

    initializeRegistrationChart();
});

// Biểu đồ tỉ lệ đăng ký
function initializeRegistrationChart() {
    const canvas = document.getElementById('registrationChart');
    if (!canvas || !window.registrationChartLabels) {
        console.warn('Không có dữ liệu cho biểu đồ đăng ký');
        return;
    }

    const labels = window.registrationChartLabels;
    const dataPercents = window.registrationChartPercents;
    const dataRegs = window.registrationChartRegs;

    const ctx = canvas.getContext('2d');

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                label: 'Tỉ lệ đăng ký (%)',
                data: dataPercents,
                borderWidth: 1,
                // có thể tinh chỉnh lại màu nếu muốn
                backgroundColor: [
                    'rgba(59, 130, 246, 0.8)',
                    'rgba(16, 185, 129, 0.8)',
                    'rgba(245, 158, 11, 0.8)',
                    'rgba(239, 68, 68, 0.8)',
                    'rgba(139, 92, 246, 0.8)',
                    'rgba(236, 72, 153, 0.8)'
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const idx = context.dataIndex;
                            const regs = dataRegs[idx] || 0;
                            const percent = dataPercents[idx] || 0;
                            return `${regs.toLocaleString()} lượt đăng ký (${percent.toFixed(2)}%)`;
                        }
                    }
                },
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}