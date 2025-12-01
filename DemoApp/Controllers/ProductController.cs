using DemoApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DemoApp.Models;

namespace DemoApp.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        // In-memory storage (In production, use a database)
        private static List<Product> _products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop Dell XPS 15", Category = "Electronics", Price = 1299, Stock = 15 },
            new Product { Id = 2, Name = "iPhone 15 Pro", Category = "Electronics", Price = 999, Stock = 30 },
            new Product { Id = 3, Name = "Office Chair Premium", Category = "Furniture", Price = 299, Stock = 50 },
            new Product { Id = 4, Name = "Wireless Mouse", Category = "Electronics", Price = 45, Stock = 8 }
        };

        public IActionResult Index(string searchTerm)
        {
            var products = _products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.TotalProducts = _products.Count;
            ViewBag.TotalValue = _products.Sum(p => p.Price * p.Stock);
            ViewBag.LowStockProducts = _products.Count(p => p.Stock < 10);
            ViewBag.SearchTerm = searchTerm;

            return View(products.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
                _products.Add(product);
                TempData["Success"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Category = product.Category;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;

                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                TempData["Success"] = "Product deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}