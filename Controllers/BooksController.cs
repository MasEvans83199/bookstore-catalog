using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookCatalog.Data;
using BookCatalog.Models;

namespace BookCatalog.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString, string bookGenre)
        {
            // Get distinct genres for the filter dropdown
            var genreQuery = _context.Books
                .OrderBy(b => b.Genre)
                .Select(b => b.Genre)
                .Distinct();

            // Start with all books
            var books = _context.Books.AsQueryable();

            // Apply search by title or author
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString));
            }

            // Apply genre filter
            if (!string.IsNullOrEmpty(bookGenre))
            {
                books = books.Where(b => b.Genre == bookGenre);
            }

            // Create a view model with books + genre list
            var viewModel = new BookIndexViewModel
            {
                Books = await books.ToListAsync(),
                Genres = new SelectList(await genreQuery.ToListAsync()),
                SearchString = searchString,
                BookGenre = bookGenre
            };

            return View(viewModel);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewBag.Genres = new SelectList(GetGenres());
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,Genre,Quantity,Price,CoverImageUrl")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Genres = new SelectList(GetGenres(), book.Genre);
            return View(book);
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            ViewBag.Query = query;

            if (string.IsNullOrWhiteSpace(query))
                return View(new List<GoogleBookViewModel>());

            var httpClient = new HttpClient();
            var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}";

            var response = await httpClient.GetStringAsync(url);
            var data = JsonDocument.Parse(response);

            var results = new List<GoogleBookViewModel>();

            foreach (var item in data.RootElement.GetProperty("items").EnumerateArray())
            {
                var volume = item.GetProperty("volumeInfo");
                
                var saleInfo = item.GetProperty("saleInfo");

                decimal? price = null;
                if (saleInfo.TryGetProperty("saleability", out var saleStatus) && saleStatus.GetString() == "FOR_SALE")
                {
                    if (saleInfo.TryGetProperty("listPrice", out var listPrice) &&
                        listPrice.TryGetProperty("amount", out var amount))
                    {
                        price = amount.GetDecimal();
                    }
                }

                results.Add(new GoogleBookViewModel
                {
                    Title = volume.GetProperty("title").GetString(),
                    Author = volume.TryGetProperty("authors", out var authors) ? authors[0].GetString() : "Unknown",
                    Thumbnail = volume.TryGetProperty("imageLinks", out var images) && images.TryGetProperty("thumbnail", out var thumb)
                        ? thumb.GetString()
                        : "/images/fallback.jpg",
                    Category = volume.TryGetProperty("categories", out var cats) ? cats[0].GetString() : "Uncategorized",
                    AverageRating = volume.TryGetProperty("averageRating", out var rating) ? rating.GetDouble() : null,
                    Price = price
                });
            }

            return View(results);
        }

        [HttpPost]
        public IActionResult Import(Book importedBook)
        {
            ViewBag.Genres = new SelectList(GetGenres(), importedBook.Genre);
            return View("Create", importedBook); // Preload form with Google data
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Genre,Quantity,Price,CoverImageUrl")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
        
        private List<string> GetGenres()
        {
            return new List<string>
            {
                "Classic",
                "Dystopian",
                "Fiction",
                "Fantasy",
                "Romance",
                "Gothic",
                "Philosophical"
            };
        }
    }
}
