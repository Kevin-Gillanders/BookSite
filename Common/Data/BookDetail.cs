using System;
using System.Collections.Generic;

public partial class BookDetail
{
    public int ID { get; set; }
    public string BookTitle { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public System.DateTime DateStarted { get; set; }
    public Nullable<System.DateTime> DateCompleted { get; set; }
    public Nullable<float> Score { get; set; }
    public int GoodreadsID { get; set; }
    public Nullable<int> YearOfPublication { get; set; }
    public Nullable<int> AmountOfGRReviews { get; set; }
    public double GRScore { get; set; }
    public string ImageURL { get; set; }
    public bool Completed { get; set; }
    public Nullable<int> NumberOfPages { get; set; }
    public string Genre { get; set; }
    public bool Display { get; set; }
}