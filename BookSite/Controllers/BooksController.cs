using BookDAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace BookSite.Controllers
{
    public class BooksController : Controller
    {
        private readonly string _Key;
        private readonly Dictionary<string, List<string>> _Genres;
        private readonly DatabaseController _DatabaseController;

        public BooksController()
        {
            _DatabaseController = new DatabaseController();
            _Key = ConfigurationManager.AppSettings["key"];
            _Genres = GetGenres();
        }

        private Dictionary<string, List<string>> GetGenres()
        {
            return new Dictionary<string, List<string>>
            {
                { "Fantasy", new List<string>{ "fantasty", "fantastic", "wizard", "magic", "tolkien", "high-fantasy", "epic-fantasy" }},
                { "SciFi", new List<string>{ "scifi", "science", "sci-fi", "science-fiction", "sf", "space", "aliens" } },
                { "Horror", new List<string>{ "horror", "supernatural", "horror-thriller" } },
                { "Western", new List<string>{ "Western", "Cowboy"} },
                { "Thriller", new List<string>{ "mystery", "thriller", "crime", "suspense" } },
                { "N/A", new List<string>{ "NOGENRE" } }
            };

        }

        // GET: Books
        public ActionResult SearchNewBook()
        {
            return View();
        }
        [HttpGet]
        public ActionResult SelectNewBook(string title)
        {
            //qjKmd5mVKWVXN2f5Wm3Lyg

            List<BookDetail> results = QueryGoodReadsAPI(title);
            return View("SelectBook", results);


        }

        private List<BookDetail> QueryGoodReadsAPI(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                title = title.Trim().Replace(' ', '+').Replace("\'", "");
            }

            string url;
            url = "https://www.goodreads.com/search/index.xml?key=" + _Key + "&q=" + title;
            //dynamic res;

            //XDocument doc = new XDocument();
            XDocument doc = XDocument.Load(url);
            List<BookDetail> results = new List<BookDetail>();


            foreach (var result in doc.Descendants("work"))
            {
                BookDetail book = CleanXMLSearchResponse(result);
                book.Display = true;
                GetAdditionalInformationXML(book);

                book.GRScore = Math.Round(book.GRScore, 2);
                results.Add(book);
            }
            return results;
        }

        private void GetAdditionalInformationXML(BookDetail book)
        {
            string url = "https://www.goodreads.com/book/show/" + book.GoodreadsID.ToString() + ".xml?key=" + _Key;

            XDocument doc = XDocument.Load(url);

            string reISBN = @".*\[(\d*).*";
            string rePages = @".*CDATA\[(\d*).*";
            string reGenre = @".*name=""?(.*)"" count.*?";

            book.ISBN = "N/A";
            foreach (var node in doc.Descendants("isbn13").ToList())
            {
                foreach (Match match in Regex.Matches(node.ToString(), reISBN, RegexOptions.IgnoreCase))
                    book.ISBN = match.Groups[1].Value;
                break;
            }

            foreach (var node in doc.Descendants("num_pages").ToList())
                foreach (Match match in Regex.Matches(node.ToString(), rePages, RegexOptions.IgnoreCase))
                {
                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                        book.NumberOfPages = Int32.Parse(match.Groups[1].Value);
                    else
                        book.NumberOfPages = -1;
                }
            book.Genre = "N/A";
            foreach (var node in doc.Descendants("popular_shelves").ToList())
                foreach (Match match in Regex.Matches(node.ToString(), reGenre, RegexOptions.IgnoreCase))
                    if (DetermineGenre(match.Groups[1].Value, book))
                        break;

        }

        private bool DetermineGenre(string value, BookDetail book)
        {
            foreach (KeyValuePair<string, List<string>> entry in _Genres)
            {
                if (entry.Value.Any(value.ToLower().Contains))
                {
                    book.Genre = entry.Key;
                    return true;
                }
            }
            return false;


        }
        [HttpPost]
        public ActionResult AddNewBook(BookDetail selection)
        {

            _DatabaseController.Insert(selection);
            return View("BookSubmitted", selection);
        }
        private BookDetail CleanXMLSearchResponse(XElement book)
        {

            string re = @"<(.*?)(?: .*)?>(.*)</.*>";
            BookDetail bookObj = new BookDetail();
            foreach (var elem in book.Nodes().ToList())
            {
                if ((elem as XElement).Name.LocalName == "best_book")
                {
                    foreach (var bookDetails in (elem as XElement).Nodes())
                    {

                        if ((bookDetails as XElement).Name.LocalName == "author")
                        {
                            foreach (var author in (bookDetails as XElement).Nodes())
                            {
                                foreach (Match match in Regex.Matches(author.ToString(), re, RegexOptions.IgnoreCase))
                                {
                                    string key = match.Groups[1].Value.ToLower();
                                    string val = match.Groups[2].Value;
                                    if (key == "name")
                                        bookObj.Author = val;

                                }
                            }
                        }
                        else
                        {
                            foreach (Match match in Regex.Matches(bookDetails.ToString(), re, RegexOptions.IgnoreCase))
                            {
                                string key = match.Groups[1].Value.ToLower();
                                string val = match.Groups[2].Value;
                                switch (key)
                                {
                                    case "id":
                                        bookObj.GoodreadsID = Int32.Parse(val);
                                        break;
                                    case "title":
                                        bookObj.BookTitle = val;
                                        break;
                                    case "image_url":
                                        bookObj.ImageURL = val;
                                        break;
                                }

                            }
                        }
                    }
                }
                else
                {
                    foreach (Match match in Regex.Matches(elem.ToString(), re, RegexOptions.IgnoreCase))
                    {
                        string key = match.Groups[1].Value.ToLower();
                        string val = match.Groups[2].Value;

                        switch (key)
                        {
                            case "ratings_count":
                                bookObj.AmountOfGRReviews = Int32.Parse(val);
                                break;
                            case "original_publication_year":
                                bookObj.YearOfPublication = Int32.Parse(val);
                                break;
                            case "average_rating":
                                bookObj.GRScore = double.Parse(val);
                                break;
                        }

                    }
                }

            }
            bookObj.DateStarted = DateTime.Now;
            return bookObj;
        }

        [HttpGet]
        public ActionResult Details()
        {
            List<BookDetail> books = new List<BookDetail>();

            books = _DatabaseController.Get();
            // TODO add pagination
            return View(books);
        }

        [HttpPost]
        public ActionResult Completed(BookDetail book)
        {
            if (book.Score == null)
            {
                ViewBag.Err = "Please include a score";
                return View("Error");
            }
            book.DateCompleted = DateTime.Now;
            book.Completed = true;
            _DatabaseController.Update(book);
            return View("Details", new List<BookDetail> { book });
        }

        [HttpPost]
        public ActionResult GiveUp(BookDetail book)
        {
            if (book.Score == null)
            {
                ViewBag.Err = "Please include a score";
                return View("Error");
            }
            book.DateCompleted = DateTime.Now;
            _DatabaseController.Update(book);
            return View("Details", new List<BookDetail> { book });
        }
        [HttpPost]
        public ActionResult Delete(BookDetail book)
        {
            book.Display = false;
            _DatabaseController.Update(book);
            return View("Details", new List<BookDetail> { book });
        }
        [HttpPost]
        public ActionResult DeleteBulk(int book)
        {
            
            _DatabaseController.Update(book);
            return View("Details", new List<BookDetail> {  });
        }

        [HttpPost]
        public ActionResult BulkSelectNewBook(HttpPostedFileBase books)
        {
            string path = UploadFile(books);
            List<BookDetail> bookSelection = new List<BookDetail>();
            IEnumerable<string> lines = System.IO.File.ReadLines(path);
            List<string> keys = new List<string>();

            for (int idx = 0;  idx < lines.Count(); idx++  )
            {

                if (idx == 0)
                {
                    keys = lines.ElementAt(idx).Split(',').ToList<string>();
                    continue;
                }
                else
                {

                    var dict = keys.Zip(lines.ElementAt(idx).Split(',').ToList<string>(), (k, v) => new { k, v })
                               .ToDictionary(x => x.k, x => x.v);

                    var goodreadsResp = QueryGoodReadsAPI(dict["Title"]);

                    BookDetail book = goodreadsResp[0];
                    book.DateStarted = DateTime.Parse(dict["Started"]);
                    book.DateCompleted = (string.IsNullOrEmpty(dict["Finished"]) ? (DateTime?)null : DateTime.Parse(dict["Finished"]));
                    book.Completed = (!string.IsNullOrEmpty(dict["Finished"]) ? true : false);
                    book.Score = (!string.IsNullOrEmpty(dict["Finished"]) ? float.Parse(dict["Score"]) : (float?)null);

                    _DatabaseController.Insert(book);
                    
                    bookSelection.Add(book);
                }
            }

            return View("BulkSelectBook", bookSelection);
        }

        private string UploadFile(HttpPostedFileBase books)
        {
            string path = "";
            if (books != null && books.ContentLength > 0)
                try
                {
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/BookList/"));
                    path = Path.Combine(Server.MapPath("~/BookList/"),
                                               Path.GetFileName(books.FileName));
                    books.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return path;
        }

        [HttpPost]
        public ActionResult BulkUploadNewBook(IEnumerable<Tuple<BookDetail, bool>> selections)
        {
            return View();
        }
    }
}