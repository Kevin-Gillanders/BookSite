//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookDAL
{
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
        public float GRScore { get; set; }
        public string ImageURL { get; set; }
        public bool Completed { get; set; }
        public Nullable<int> NumberOfPages { get; set; }
        public string Genre { get; set; }
        public bool Display { get; set; }
    }
}
