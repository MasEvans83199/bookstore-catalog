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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

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

            var genreQuery = _context.Books
                .OrderBy(b => b.Genre)
                .Select(b => b.Genre)
                .Distinct();
            
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Author.Contains(searchString));
            }
            
            if (!string.IsNullOrEmpty(bookGenre))
            {
                books = books.Where(b => b.Genre == bookGenre);
            }
            
            var viewModel = new BookIndexViewModel
            {
                Books = await books.ToListAsync(),
                StaffPicks = await _context.Books.Where(b => b.IsStaffPick).ToListAsync(),
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
        public async Task<IActionResult> Create([Bind("Id,Title,Author,Genre,Quantity,Price,CoverImageUrl,CoverImagePath")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Genres = new SelectList(GetGenres(), book.Genre);
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
                
                string title = volume.GetProperty("title").GetString();

                // Get author
                string author = volume.TryGetProperty("authors", out var authors)
                    ? authors[0].GetString()
                    : "Unknown";

                // Get genre/category
                string category = volume.TryGetProperty("categories", out var cats)
                    ? cats[0].GetString()
                    : "Uncategorized";

                // Get rating
                double? averageRating = volume.TryGetProperty("averageRating", out var rating)
                    ? rating.GetDouble()
                    : null;

                // Get price (optional: leave as is or use your logic)
                decimal price = 0;

                // Get Google thumbnail
                string googleThumbnail = volume.TryGetProperty("imageLinks", out var images) &&
                                         images.TryGetProperty("thumbnail", out var thumb)
                    ? thumb.GetString()
                    : null;
                
                string isbn = null;
                if (volume.TryGetProperty("industryIdentifiers", out var ids))
                {
                    foreach (var id in ids.EnumerateArray())
                    {
                        if (id.GetProperty("type").GetString() == "ISBN_13")
                        {
                            isbn = id.GetProperty("identifier").GetString();
                            break;
                        }
                    }
                }

                string openLibraryThumbnail = !string.IsNullOrEmpty(isbn)
                    ? $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg"
                    : null;
                
                string finalThumbnail = IsGoogleThumbnailUsable(googleThumbnail)
                    ? googleThumbnail
                    : (!string.IsNullOrEmpty(openLibraryThumbnail) ? openLibraryThumbnail : "/images/fallback.jpg");

                results.Add(new GoogleBookViewModel
                {
                    Title = title,
                    Author = author,
                    Category = category,
                    AverageRating = averageRating,
                    Price = price,
                    ISBN = isbn,
                    GoogleThumbnail = googleThumbnail,
                    Thumbnail = finalThumbnail
                });
            }

            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> Import(Book importedBook, string FallbackImageUrl)
        {
            if (!string.IsNullOrEmpty(importedBook.CoverImageUrl))
            {
                var webClient = new HttpClient();
                try
                {
                    var imageBytes = await webClient.GetByteArrayAsync(importedBook.CoverImageUrl);

                    if (imageBytes.Length <= 100 && !string.IsNullOrEmpty(FallbackImageUrl))
                    {
                        imageBytes = await webClient.GetByteArrayAsync(FallbackImageUrl);
                    }

                    if (imageBytes.Length > 100)
                    {
                        var fileName = $"{Guid.NewGuid()}.jpg";
                        var savePath = Path.Combine("wwwroot", "images", "covers", fileName);
                        var relativePath = $"/images/covers/{fileName}";

                        Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                        await System.IO.File.WriteAllBytesAsync(savePath, imageBytes);

                        importedBook.CoverImagePath = relativePath;
                    }
                    else
                    {
                        importedBook.CoverImagePath = "/images/fallback.jpg";
                    }
                }
                catch
                {
                    importedBook.CoverImagePath = "/images/fallback.jpg";
                }
            }
            else
            {
                importedBook.CoverImagePath = "/images/fallback.jpg";
            }

            ViewBag.Genres = new SelectList(GetGenres(), importedBook.Genre);
            ModelState.Clear();
            return View("Create", importedBook);
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
        
        private bool IsGoogleThumbnailUsable(string url)
        {
            return !string.IsNullOrEmpty(url) && !url.Contains("no_cover_thumb");
        }
    }
}
