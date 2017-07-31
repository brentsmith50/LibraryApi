using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApi.Entities;
using LibraryApi.Helpers;
using LibraryApi.DTOs;

namespace LibraryApi.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        #region Init

        private LibraryContext context;
        private IPropertyMappingService propertyMappingService;

        public LibraryRepository(LibraryContext context, IPropertyMappingService propertyMappingService)
        {
            this.context = context;
            this.propertyMappingService = propertyMappingService;
        }
        #endregion

        #region Methods

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();

            context.Authors.Add(author);

            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (var book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            var author = GetAuthor(authorId);
            if (author != null)
            {
                if (book.Id == null)
                {
                    book.Id = Guid.NewGuid();
                }
                author.Books.Add(book);
            }
        }

        public bool AuthorExists(Guid authorId)
        {
            return context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        // ORIGINAL - unpaged list....
        //public IEnumerable<Author> GetAuthors()
        //{
        //    return context.Authors.OrderBy(a => a.FirstName).ThenBy(a => a.LastName);
        //}

        
        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourcesParameters)
        {
            //var collectionBeforePaging = context.Authors.OrderBy(a => a.FirstName)
            //                                            .ThenBy(a => a.LastName)
            //                                            .AsQueryable();

            var collectionBeforePaging = context.Authors.ApplySort(authorsResourcesParameters.OrderBy, 
                                                                   propertyMappingService.GetPropertyMapping<AuthorDto, Author>());

            if (!string.IsNullOrEmpty(authorsResourcesParameters.Genre))
            {
                var genreForWhereClause = authorsResourcesParameters.Genre.Trim().ToLowerInvariant();
                collectionBeforePaging = collectionBeforePaging.Where(a => a.Genre.ToLowerInvariant() == genreForWhereClause);
            }

            if (!string.IsNullOrEmpty(authorsResourcesParameters.SearchQuery))
            {
                var searchQueryForWhereClause = authorsResourcesParameters.SearchQuery.Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging.Where(a => a.Genre.ToLowerInvariant()
                                                               .Contains(searchQueryForWhereClause)
                || a.FirstName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                || a.LastName.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<Author>.Create(collectionBeforePaging, authorsResourcesParameters.PageNumber, authorsResourcesParameters.PageSize);
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();

        }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return context.Books
              .Where(b => b.AuthorId == authorId && b.Id == bookId).FirstOrDefault();
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return context.Books.Where(b => b.AuthorId == authorId).OrderBy(b => b.Title).ToList();
        }

        public bool Save()
        {
            return (context.SaveChanges() >= 0);
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public void UpdateBookForAuthor(Book book)
        {
            // no code in this implementation
        }
        #endregion
    }
}
