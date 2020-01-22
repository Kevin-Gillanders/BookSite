# Booksite

This is a site which I created so that I would be able to keep track of the books I am reading.

The user submits a title, this is then passed to the [Goodreads API](https://www.goodreads.com/api/index "A link to the goodreads API documentation")
where the top selection of results are passed back to my project. The xml response is parsed and the relevant information is extraced and filled 
into an entity framework model. The user then selects the correct result which is then saved to a database recording the date and time.

The books can be viewed in the details section, where the user can 
1. Complete a book. This sets a completion flag and records the date, this requires the user to record a score
2. Give up on a book. The date is recorded and the user must submit a score. This also marks the book as abandoned.
3. Remove a book. If a book was selected by accident, the user can select this option. This does not delete the record from the database only sets a display flag to false on the database.


This project is written in .NET framework 4.7.2 using Visural Studio 2019