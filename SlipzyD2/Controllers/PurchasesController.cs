
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlipzyD2.Data;
using SlipzyD2.Models;
using SlipzyD2.Repositories.Interfaces;
using SlipzyD2.ViewModels;
using System.Security.Claims;

namespace SlipzyD2.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseRepository _purchases;
        private readonly ICategoryRepository _categories;
        private readonly IWebHostEnvironment _environment;

        public PurchasesController(
            IPurchaseRepository purchases,
            ICategoryRepository categories,
            IWebHostEnvironment environment)
        {
            _purchases = purchases;
            _categories = categories;
            _environment = environment;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not logged in.");

        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            const int pageSize = 8;

            var vm = new PurchaseListViewModel
            {
                Search = search,
                CategoryId = categoryId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = Math.Max(1, page),
                Categories = (await _categories.GetAllAsync()).ToList()
            };

            vm.TotalCount = await _purchases.CountAsync(
                UserId, search, categoryId, fromDate, toDate);

            vm.Purchases = (await _purchases.GetAllAsync(
                UserId, search, categoryId, fromDate, toDate,
                vm.Page, pageSize)).ToList();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categories.GetAllAsync();

            return View(new CreatePurchaseViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseViewModel model)
        {
            if (model.Items == null || model.Items.Count == 0 ||
                model.Items.Any(x => string.IsNullOrWhiteSpace(x.ProductName)))
            {
                ModelState.AddModelError("Items", "Add at least one product.");
            }

            if (model.WarrantyMonths < 0 || model.WarrantyMonths > 1200)
            {
                ModelState.AddModelError(
                    "WarrantyMonths", "Enter a valid warranty duration.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categories.GetAllAsync();
                return View(model);
            }

            string? savedPath = null;

            try
            {
                ReceiptFile? receipt = null;

                if (model.Receipt != null)
                {
                    var file = model.Receipt;

                    var allowedTypes = new[]
                    {
                        "image/jpeg", "image/png", "image/webp"
                    };

                    if (file.Length > 5 * 1024 * 1024 ||
                        !allowedTypes.Contains(file.ContentType))
                    {
                        ModelState.AddModelError(
                            "Receipt", "Upload a JPG, PNG or WEBP image up to 5 MB.");

                        ViewBag.Categories = await _categories.GetAllAsync();
                        return View(model);
                    }

                    var folder = Path.Combine(
                        _environment.WebRootPath, "uploads", "receipts");

                    Directory.CreateDirectory(folder);

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    var fileName = $"{Guid.NewGuid():N}{extension}";

                    var fullPath = Path.Combine(folder, fileName);

                    await using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedPath = fullPath;

                    receipt = new ReceiptFile
                    {
                        OriginalFileName = Path.GetFileName(file.FileName),
                        RelativePath = "/uploads/receipts/" + fileName
                    };
                }

                DateTime? expiry = null;

                if (model.WarrantyStart.HasValue &&
                    model.WarrantyMonths.HasValue &&
                    model.WarrantyMonths.Value > 0)
                {
                    expiry = model.WarrantyStart.Value.Date.AddMonths(
                        model.WarrantyMonths.Value);
                }

                var items = model.Items.Select(x => new PurchaseItem
                {
                    ProductName = x.ProductName.Trim(),
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList();

                var purchase = new Purchase
                {
                    UserId = UserId,
                    StoreName = model.StoreName.Trim(),
                    PurchaseDate = model.PurchaseDate.Date,
                    CategoryId = model.CategoryId,
                    WarrantyStart = model.WarrantyStart,
                    WarrantyMonths = model.WarrantyMonths,
                    WarrantyExpiry = expiry,
                    ReturnDeadline = model.ReturnDeadline,
                    TotalAmount = items.Sum(x => x.Quantity * x.UnitPrice)
                };

                await _purchases.CreateAsync(purchase, items, receipt);

                TempData["Success"] = "Purchase saved successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                if (savedPath != null && System.IO.File.Exists(savedPath))
                    System.IO.File.Delete(savedPath);

                ModelState.AddModelError(
                    "", "The purchase could not be saved. Please try again.");

                ViewBag.Categories = await _categories.GetAllAsync();

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var purchase = await _purchases.GetByIdAsync(id, UserId);

            if (purchase == null)
                return NotFound();

            ViewBag.Categories = await _categories.GetAllAsync();

            return View(purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Purchase model)
        {
            var existing = await _purchases.GetByIdAsync(
                model.PurchaseId, UserId);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categories.GetAllAsync();
                model.Items = existing.Items;
                model.Receipts = existing.Receipts;
                return View(model);
            }

            model.UserId = UserId;

            model.WarrantyExpiry =
                model.WarrantyStart.HasValue &&
                model.WarrantyMonths.HasValue &&
                model.WarrantyMonths.Value > 0
                    ? model.WarrantyStart.Value.AddMonths(model.WarrantyMonths.Value)
                    : null;

            await _purchases.UpdateAsync(model);

            TempData["Success"] = "Purchase updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _purchases.GetByIdAsync(id, UserId);

            if (purchase == null)
                return NotFound();

            return View("Edit", purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _purchases.DeleteAsync(id, UserId);

            if (!deleted)
                return NotFound();

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> Search(string? term)
        {
            var rows = await _purchases.GetAllAsync(
                UserId, term, null, null, null, 1, 8);

            return Json(rows.Select(p => new
            {
                purchaseId = p.PurchaseId,
                storeName = p.StoreName,
                productNames = p.ProductNames,
                totalAmount = p.TotalAmount,
                purchaseDate = p.PurchaseDate.ToString("dd MMM yyyy"),
                categoryName = p.CategoryName
            }));
        }
    }
}
