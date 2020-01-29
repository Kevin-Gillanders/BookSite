using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;



public class Database
{
    private readonly SQLiteConnection conn;

    public Database()
    {
        string connectionString = ConfigurationManager.AppSettings["connectionString"];

        System.IO.Directory.CreateDirectory(ConfigurationManager.AppSettings["connectionLocation"]);
        conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", connectionString));
        CreateDataBase();
    }

    public void CreateDataBase()
    {
        conn.Open();
        SQLiteCommand createQuery = conn.CreateCommand();

        using (var localTransaction = conn.BeginTransaction()) 
        {
            try
            {
                createQuery.CommandText = "CREATE TABLE IF NOT EXISTS Books (" +
                                                  "[ID] [INTEGER] PRIMARY KEY AUTOINCREMENT, " +
                                                  "[BookTitle] [TEXT] NOT NULL," +
                                                  "[Author][TEXT] NOT NULL," +

                                                  "[ISBN] [TEXT]  NOT NULL," +

                                                  "[DateStarted] [TEXT] NOT NULL," + // DATETIME

                                                  "[DateCompleted] [TEXT] NULL," + // DATETIME
                                                  "[Score] [REAL] NULL," +
                                                  "[GoodreadsID] [INTEGER] NOT NULL," +

                                                  "[YearOfPublication] [INTEGER] NULL," +
                                                  "[AmountOfGRReviews] [INTEGER] NULL," +
                                                  "[GRScore] [REAL] NOT NULL," +

                                                  "[ImageURL] [TEXT]  NULL," +

                                                  "[Completed] [INTEGER] NOT NULL," + //Boolean

                                                  "[NumberOfPages] [INTEGER] NULL," +
                                                  "[Genre] [TEXT]  NOT NULL," +

                                                  "[Display] [INTEGER] NOT NULL" + //Boolean
                                            ");";
                //sqlite_cmd.CommandText = "DROP TABLE test;";

                createQuery.ExecuteNonQuery();
                localTransaction.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                localTransaction.Rollback();
            }
            finally 
            {
                
                conn.Close(); 
            }
        }
    }

    public void Update(BookDetail book)
    {
        conn.Open();
        using (var localTransaction = conn.BeginTransaction())
        {

            try
            {
                var createQuery = new SQLiteCommand("UPDATE Books " +
                                                    "set " +
                                                        "DateCompleted = :DateCompleted, " +
                                                        "Score = :Score, " +
                                                        "Completed = :Completed, " +
                                                        "Display = :Display " +
                                                    "WHERE ID = :ID", conn);

                createQuery.Parameters.AddWithValue("DateCompleted", book.DateCompleted);
                createQuery.Parameters.AddWithValue("Score", book.Score);
                createQuery.Parameters.AddWithValue("Completed", book.Completed);
                createQuery.Parameters.AddWithValue("Display", book.Display);
                createQuery.Parameters.AddWithValue("ID", book.ID);

                createQuery.CommandType = CommandType.Text;

                createQuery.ExecuteNonQuery();
                localTransaction.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                localTransaction.Rollback();
            }

            finally
            {
                conn.Close();
            }
        }
    }

    public List<BookDetail> Read()
    {
        conn.Open();
        List<BookDetail> books = new List<BookDetail>();
        try
        {
            using (SQLiteCommand fmd = conn.CreateCommand())
            {
                fmd.CommandText = @"select * from books where Display = 1 ORDER BY ID DESC;";
                fmd.CommandType = CommandType.Text;
                SQLiteDataReader records = fmd.ExecuteReader();
                while (records.Read())
                {
                    books.Add(MapSQLToObject(records));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            conn.Close();
        }
        return books;

    }

    private BookDetail MapSQLToObject(SQLiteDataReader record)
    {
        BookDetail book = new BookDetail();
        for (int cell = 0; cell < record.FieldCount; cell++)
        {
            string name = record.GetName(cell);
            string value = record[cell].ToString();
            //TODO test these
            switch(name)
            {
                case "ID":
                    book.ID = Int32.Parse(value);
                    break;
                case "BookTitle":
                    book.BookTitle = value;
                    break;
                case "Author":
                    book.Author = value;
                    break;
                case "ISBN":
                    book.ISBN = value;
                    break;
                case "DateStarted":
                    Debug.WriteLine(value);
                    Debug.WriteLine(DateTime.Parse(value));
                    book.DateStarted = DateTime.Parse(value);
                    break;
                case "DateCompleted":
                    if(!string.IsNullOrEmpty(value))
                        book.DateCompleted = DateTime.Parse(value);
                    break;
                case "Score":
                    if (!string.IsNullOrEmpty(value) && value != "0")
                        book.Score = float.Parse(value);
                    else
                        book.Score = null;
                    break;
                case "GoodreadsID":
                    book.GoodreadsID = Int32.Parse(value);
                    break;
                case "YearOfPublication":
                    if (!string.IsNullOrEmpty(value))
                        book.YearOfPublication = Int32.Parse(value);
                    break;
                case "AmountOfGRReviews":
                    if (!string.IsNullOrEmpty(value))
                        book.AmountOfGRReviews = Int32.Parse(value);
                    break;
                case "GRScore":
                    book.GRScore = double.Parse(value);
                    break;
                case "ImageURL":
                    book.ImageURL = value;
                    break;
                case "Completed":
                    book.Completed = (Int32.Parse(value) == 1 ? true : false);
                    break;
                case "NumberOfPages":
                    if (!string.IsNullOrEmpty(value))
                        book.NumberOfPages = Int32.Parse(value);
                    break;
                case "Genre":
                    book.Genre = value;
                    break;
                case "Display":
                    book.Display = (Int32.Parse(value) == 1 ? true : false);
                    break;

            }
        }
        return book;
    }

    public int Add(BookDetail book)
    {
        int id;
        conn.Open();
        SQLiteCommand createQuery = conn.CreateCommand();

        using (var localTransaction = conn.BeginTransaction())
        {

            try
            {
                //TODO Paramatarise this as in update()
                string command = "INSERT INTO Books(BookTitle, Author, ISBN, DateStarted, DateCompleted, Score, GoodreadsID," +
                                "YearOfPublication, AmountOfGRReviews, GRScore, ImageURL, Completed, NumberOfPages, Genre, Display)" +
                                "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')";
                command = FormatCommand(book, command);
                createQuery.CommandText = command;
                createQuery.ExecuteNonQuery();
                localTransaction.Commit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                localTransaction.Rollback();
            }

            finally
            {
                id = (int)conn.LastInsertRowId;
                conn.Close();
            }
        }
        return id;
        


    
    }

    private string FormatCommand(BookDetail book, string command)
    {
        return string.Format(command, 
        book.BookTitle, 
        book.Author, 
        book.ISBN, 
        book.DateStarted,
        book.DateCompleted,
        book.Score,
        book.GoodreadsID,
        book.YearOfPublication,
        book.AmountOfGRReviews,
        book.GRScore,
        book.ImageURL,
        (book.Completed ? 1 : 0),
        book.NumberOfPages,
        book.Genre,
        (book.Display ? 1 : 0));
         
    }
}

   