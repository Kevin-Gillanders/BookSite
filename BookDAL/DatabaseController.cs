using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookDAL
{
    public class DatabaseController
    {
        private readonly Database _context;
        public DatabaseController()
        {
            _context =  new Database();
        }
        public void Insert(BookDetail selection)
        {
            try
            {
                selection.ID = _context.Add(selection);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        public List<BookDetail> Get()
        {
            return _context.Read();
        }

        public void Update(BookDetail book)
        {
            try
            {
                _context.Update(book);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        public void Update(int bookID)
        {
            try
            {
                _context.Update(bookID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
