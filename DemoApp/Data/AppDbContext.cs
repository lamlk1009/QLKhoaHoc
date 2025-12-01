using System;
using Microsoft.EntityFrameworkCore;
using DemoApp.Models;

namespace DemoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; } = default!;
        public DbSet<KhoaHoc> KhoaHoc { get; set; } = default!;
        public DbSet<BaiHoc> BaiHoc { get; set; } = default!;
        public DbSet<DangKyKhoaHoc> DangKyKhoaHoc { get; set; } = default!;
        public DbSet<DanhMuc> DanhMuc { get; set; } = default!;
        public DbSet<Role> Role { get; set; } = default!;
        public DbSet<TienDoHocTap> TienDoHocTap { get; set; } = default!;
        public DbSet<Cart> Cart { get; set; } = default!;
        public DbSet<BuoiHoc> BuoiHoc { get; set; } = default!;
        public DbSet<DiemDanh> DiemDanh { get; set; } = default!;
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== User =====
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.RoleId);

                entity.HasMany(u => u.KhoaHoc)
                      .WithOne(kh => kh.user)
                      .HasForeignKey(kh => kh.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.TienDoHocTap)
                      .WithOne(td => td.User)
                      .HasForeignKey(td => td.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.DangKyKhoaHocs)
                      .WithOne(dk => dk.user)
                      .HasForeignKey(dk => dk.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Carts)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany<DiemDanh>()
                      .WithOne(dd => dd.User!)
                      .HasForeignKey(dd => dd.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Role =====
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // ===== DanhMuc =====
            modelBuilder.Entity<DanhMuc>(entity =>
            {
                entity.Property(e => e.TrangThai).HasDefaultValue(true);

                entity.HasMany(dm => dm.KhoaHoc)
                      .WithOne(kh => kh.DanhMuc)
                      .HasForeignKey(kh => kh.DanhMucId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== KhoaHoc =====
            modelBuilder.Entity<KhoaHoc>(entity =>
            {
                entity.HasIndex(e => e.MaKhoaHoc).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.DanhMucId);
                entity.HasIndex(e => e.TrangThai);

                entity.Property(e => e.CapDo).HasDefaultValue("CoBan");
                entity.Property(e => e.TrangThai).HasDefaultValue("BanNhap");
                entity.Property(e => e.GiaTien).HasPrecision(18, 2).HasDefaultValue(0);

                entity.HasMany(kh => kh.TienDoHocTap)
                      .WithOne(td => td.KhoaHoc)
                      .HasForeignKey(td => td.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(kh => kh.BaiHoc)
                      .WithOne(bh => bh.KhoaHoc)
                      .HasForeignKey(bh => bh.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(kh => kh.DangKyKhoaHoc)
                      .WithOne(dk => dk.KhoaHoc)
                      .HasForeignKey(dk => dk.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(kh => kh.user)
                      .WithMany(u => u.KhoaHoc)
                      .HasForeignKey(kh => kh.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(kh => kh.DanhMuc)
                      .WithMany(dm => dm.KhoaHoc)
                      .HasForeignKey(kh => kh.DanhMucId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany<BuoiHoc>()
                      .WithOne(b => b.KhoaHoc!)
                      .HasForeignKey(b => b.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany<DiemDanh>()
                      .WithOne(dd => dd.KhoaHoc!)
                      .HasForeignKey(dd => dd.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== BaiHoc =====
            modelBuilder.Entity<BaiHoc>(entity =>
            {
                entity.HasIndex(e => e.KhoaHocId);
                entity.Property(e => e.LoaiNoiDung).HasDefaultValue("Video");

                entity.HasMany(bh => bh.TienDoHocTap)
                      .WithOne(td => td.BaiHoc)
                      .HasForeignKey(td => td.BaiHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(bh => bh.KhoaHoc)
                      .WithMany(kh => kh.BaiHoc)
                      .HasForeignKey(bh => bh.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== DangKyKhoaHoc =====
            modelBuilder.Entity<DangKyKhoaHoc>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.KhoaHocId);
                entity.HasIndex(e => new { e.UserId, e.KhoaHocId }).IsUnique();

                entity.Property(e => e.TrangThai)
                      .HasMaxLength(20)
                      .HasDefaultValue("DangHoc");

                entity.HasOne(dk => dk.user)
                      .WithMany(u => u.DangKyKhoaHocs)
                      .HasForeignKey(dk => dk.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dk => dk.KhoaHoc)
                      .WithMany(kh => kh.DangKyKhoaHoc)
                      .HasForeignKey(dk => dk.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== TienDoHocTap =====
            modelBuilder.Entity<TienDoHocTap>(entity =>
            {
                entity.HasOne(td => td.User)
                      .WithMany(u => u.TienDoHocTap)
                      .HasForeignKey(td => td.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(td => td.KhoaHoc)
                      .WithMany(kh => kh.TienDoHocTap)
                      .HasForeignKey(td => td.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(td => td.BaiHoc)
                      .WithMany(bh => bh.TienDoHocTap)
                      .HasForeignKey(td => td.BaiHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Cart =====
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Carts)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== BuoiHoc =====
            modelBuilder.Entity<BuoiHoc>(entity =>
            {
                entity.HasIndex(e => e.KhoaHocId);

                entity.HasOne(bh => bh.KhoaHoc)
                      .WithMany(kh => kh.BuoiHocs!)
                      .HasForeignKey(bh => bh.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(bh => bh.DiemDanhs!)
                      .WithOne(dd => dd.BuoiHoc!)
                      .HasForeignKey(dd => dd.BuoiHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== DiemDanh =====
            modelBuilder.Entity<DiemDanh>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.KhoaHocId);
                entity.HasIndex(e => e.BuoiHocId);

                entity.Property(e => e.TrangThai)
                      .HasMaxLength(20)
                      .HasDefaultValue("present");

                entity.HasOne(dd => dd.User)
                      .WithMany(u => u.DiemDanhs!)
                      .HasForeignKey(dd => dd.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dd => dd.KhoaHoc)
                      .WithMany(kh => kh.DiemDanhs!)
                      .HasForeignKey(dd => dd.KhoaHocId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(dd => dd.BuoiHoc)
                      .WithMany(bh => bh.DiemDanhs!)
                      .HasForeignKey(dd => dd.BuoiHocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.CartId);

                entity.HasOne(c => c.User)
                    .WithMany()                  // nếu User có ICollection<Cart> thì đổi sang .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.CartItemId);

                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.KhoaHoc)
                    .WithMany()                  // nếu KhoaHoc có ICollection<CartItem> thì đổi sang .WithMany(k => k.CartItems)
                    .HasForeignKey(ci => ci.KhoaHocId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // ===== Role =====
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "User" }
            );

            // ===== User =====
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@khoahoc.vn",
                    Password = "admin123",
                    FullName = "Quản Trị Viên",
                    RoleId = 1,
                    Address = "Hà Nội",
                    NumberPhone = "0123456789"
                },
                new User
                {
                    UserId = 2,
                    Username = "user01",
                    Email = "user01@khoahoc.vn",
                    Password = "user123",
                    FullName = "Nguyễn Văn A",
                    RoleId = 2,
                    Address = "TP HCM",
                    NumberPhone = "0987654321"
                },
                new User
                {
                    UserId = 3,
                    Username = "user02",
                    Email = "user02@gmail.com",
                    Password = "user321",
                    FullName = "Lê Văn C",
                    RoleId = 2,
                    Address = "Đà Nẵng",
                    NumberPhone = "0912345678"
                }
            );

            // ===== DanhMuc =====
            modelBuilder.Entity<DanhMuc>().HasData(
                new DanhMuc
                {
                    Id = 1,
                    TenDanhMuc = "Lập Trình Web",
                    MoTa = "Học phát triển website",
                    ThuTuHienThi = 1,
                    TrangThai = true
                },
                new DanhMuc
                {
                    Id = 2,
                    TenDanhMuc = "Lập Trình Mobile",
                    MoTa = "Phát triển ứng dụng di động",
                    ThuTuHienThi = 2,
                    TrangThai = true
                },
                new DanhMuc
                {
                    Id = 3,
                    TenDanhMuc = "Cơ Sở Dữ Liệu & Backend",
                    MoTa = "Cơ sở dữ liệu, SQL, backend",
                    ThuTuHienThi = 3,
                    TrangThai = true
                },
                new DanhMuc
                {
                    Id = 4,
                    TenDanhMuc = "Frontend Framework",
                    MoTa = "React, Vue, Angular...",
                    ThuTuHienThi = 4,
                    TrangThai = true
                }
            );

            // ===== KhoaHoc (4 khóa theo yêu cầu) =====
            modelBuilder.Entity<KhoaHoc>().HasData(
                new KhoaHoc
                {
                    Id = 1,
                    MaKhoaHoc = "CPP001",
                    TenKhoaHoc = "Lập trình C++",
                    MoTaNgan = "Khóa học lập trình C++ cho người mới",
                    AnhBia = "cpp.jpg",
                    UserId = 1,
                    DanhMucId = 3,
                    CapDo = "CoBan",
                    GiaTien = 150000,
                    TrangThai = "DaXuatBan",
                    NgayTao = DateTime.Now
                },
                new KhoaHoc
                {
                    Id = 2,
                    MaKhoaHoc = "HTMLCSS001",
                    TenKhoaHoc = "Lập trình HTML và CSS",
                    MoTaNgan = "Khóa học HTML CSS cơ bản",
                    AnhBia = "htmlcss.jpg",
                    UserId = 1,
                    DanhMucId = 1,
                    CapDo = "CoBan",
                    GiaTien = 300000,
                    TrangThai = "DaXuatBan",
                    NgayTao = DateTime.Now
                },
                new KhoaHoc
                {
                    Id = 3,
                    MaKhoaHoc = "SQL001",
                    TenKhoaHoc = "Lập trình SQL",
                    MoTaNgan = "SQL cho người mới bắt đầu",
                    AnhBia = "sql.jpg",
                    UserId = 1,
                    DanhMucId = 3,
                    CapDo = "CoBan",
                    GiaTien = 250000,
                    TrangThai = "DaXuatBan",
                    NgayTao = DateTime.Now
                },
                new KhoaHoc
                {
                    Id = 4,
                    MaKhoaHoc = "REACT001",
                    TenKhoaHoc = "Lập trình React",
                    MoTaNgan = "Khóa học ReactJS cơ bản",
                    AnhBia = "react.jpg",
                    UserId = 1,
                    DanhMucId = 4,
                    CapDo = "CoBan",
                    GiaTien = 500000,
                    TrangThai = "DaXuatBan",
                    NgayTao = DateTime.Now
                }
            );

            // ===== BaiHoc (YouTube URL) =====
            modelBuilder.Entity<BaiHoc>().HasData(
                // 1. Lập trình C++
                new BaiHoc
                {
                    Id = 1,
                    KhoaHocId = 1,
                    TenBaiHoc = "Lộ Trình Học Lập Trình Cho Người Mới",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=S3nx34WFXjI&list=PLZPZq0r_RZOMHoXIcxze_lP97j2Ase2on",
                    ThuTuHienThi = 1,
                    ThoiLuong = 0
                },
                new BaiHoc
                {
                    Id = 2,
                    KhoaHocId = 1,
                    TenBaiHoc = "Làm Quen Với Ngôn Ngữ Lập Trình C++",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=74B6PXO97Tw&list=PLux-_phi0Rz0Hq9fDP4TlOulBl8APKp79&index=2",
                    ThuTuHienThi = 2,
                    ThoiLuong = 0
                },
                new BaiHoc
                {
                    Id = 3,
                    KhoaHocId = 1,
                    TenBaiHoc = "Toàn Tập Về Các Toán Tử Cơ Bản Trong C++ (Phần 1)",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=y-_fNgvSfjc&list=PLux-_phi0Rz0Hq9fDP4TlOulBl8APKp79&index=3",
                    ThuTuHienThi = 3,
                    ThoiLuong = 0
                },
                new BaiHoc
                {
                    Id = 4,
                    KhoaHocId = 1,
                    TenBaiHoc = "Toàn Tập Về Các Toán Tử Cơ Bản Trong C++ (Phần 2)",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=y-_fNgvSfjc&list=PLux-_phi0Rz0Hq9fDP4TlOulBl8APKp79&index=3",
                    ThuTuHienThi = 4,
                    ThoiLuong = 0
                },

                // 2. Lập trình HTML và CSS
                new BaiHoc
                {
                    Id = 5,
                    KhoaHocId = 2,
                    TenBaiHoc = "Tổng quan về khóa học HTML CSS",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=R6plN3FvzFY&list=PL_-VfJajZj0U9nEXa4qyfB4U5ZIYCMPlz",
                    ThuTuHienThi = 1,
                    ThoiLuong = 0
                },
                new BaiHoc
                {
                    Id = 6,
                    KhoaHocId = 2,
                    TenBaiHoc = "HTML CSS là gì? | Ví dụ trực quan về HTML & CSS",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "http://youtube.com/watch?v=zwsPND378OQ&list=PL_-VfJajZj0U9nEXa4qyfB4U5ZIYCMPlz&index=2",
                    ThuTuHienThi = 2,
                    ThoiLuong = 0
                },
                new BaiHoc
                {
                    Id = 7,
                    KhoaHocId = 2,
                    TenBaiHoc = "Làm quen với Dev tools",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=7BJiPyN4zZ0&list=PL_-VfJajZj0U9nEXa4qyfB4U5ZIYCMPlz&index=3",
                    ThuTuHienThi = 3,
                    ThoiLuong = 0
                },

                // 3. Lập trình SQL
                new BaiHoc
                {
                    Id = 8,
                    KhoaHocId = 3,
                    TenBaiHoc = "SQL cho người mới bắt đầu",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=oPV2sjMG53U&list=PLZPZq0r_RZOMskz6MdsMOgxzheIyjo-BZ",
                    ThuTuHienThi = 1,
                    ThoiLuong = 0
                },

                // 4. Lập trình React
                new BaiHoc
                {
                    Id = 9,
                    KhoaHocId = 4,
                    TenBaiHoc = "ReactJS là gì | Tại sao nên học ReactJS | Khóa học ReactJS miễn phí",
                    LoaiNoiDung = "Video",
                    DuongDanNoiDung = "https://www.youtube.com/watch?v=x0fSBAgBrOQ&list=PL_-VfJajZj0UXjlKfBwFX73usByw3Ph9Q",
                    ThuTuHienThi = 1,
                    ThoiLuong = 0
                }
            );

            // ===== DangKyKhoaHoc: user 2 đăng ký khóa C++ =====
            modelBuilder.Entity<DangKyKhoaHoc>().HasData(
                new DangKyKhoaHoc
                {
                    Id = 1,
                    UserId = 2,
                    KhoaHocId = 1,
                    NgayDangKy = DateTime.Now,
                    TrangThai = "DangHoc"
                }
            );

            // TienDoHocTap & DiemDanh để rỗng, user sẽ tạo trong quá trình học.
        }
    }
}
