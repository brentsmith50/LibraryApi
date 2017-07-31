using LibraryApi.Entities;
using LibraryApi.Helpers;
using System;
using System.Collections.Generic;

namespace LibraryApi.Services
{
    public interface ILibraryRepository
    {
        Author GetAuthor(Guid authorId);
        PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        IEnumerable<Book> GetBooksForAuthor(Guid authorId);
        Book GetBookForAuthor(Guid authorId, Guid bookId);
        void AddBookForAuthor(Guid authorId, Book book);
        void UpdateBookForAuthor(Book book);
        void DeleteBook(Book book);
        bool Save();
    }
}
