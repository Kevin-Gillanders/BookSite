using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace BookDAL
{
    public class DatabaseController
    {
        internal DbSet<BookDetail> _dbSet;
        internal BookContext _context;
        public DatabaseController()
        {
            _context = new BookContext();
            _dbSet = _context.Set<BookDetail>();
        }
        public void Insert(BookDetail selection)
        {
            try
            {
                _dbSet.Add(selection);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        public List<BookDetail> Get()
        {
            return _dbSet.Where(x => x.Display).ToList();
        }

        public void Update(BookDetail book)
        {
            try
            {
                var oldBook = _dbSet
                    .Where(x => x.ID == book.ID)
                    .FirstOrDefault();

                if (oldBook == null)
                    throw new ArgumentNullException();

                _context.Entry(oldBook).CurrentValues.SetValues(book);
                _context.SaveChanges();
            }
            catch (Exception ex)
            { 
                Debug.WriteLine(ex.ToString()); 
            }
        }

        public void Delete(BookDetail book)
        {
            try
            {
                _dbSet.Attach(book);
                _dbSet.Remove(book);
                _context.SaveChanges();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
