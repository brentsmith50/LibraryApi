using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using LibraryApi.Services;
using LibraryApi.DTOs;
using LibraryApi.Entities;
using LibraryApi.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        #region Init
        private ILibraryRepository libraryRepository;
        private ILogger<BooksController> logger;

        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger)
        {
            this.libraryRepository = libraryRepository;
            this.logger = logger;
        }
        #endregion

        #region Get Post Delete
        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = libraryRepository.GetBooksForAuthor(authorId);

            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            return Ok(booksForAuthor);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);

            return Ok(bookForAuthor);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description must be different than the Title.");
            }

            if (!ModelState.IsValid)
            {
                // return a 422 
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);
            libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            libraryRepository.DeleteBook(bookForAuthorFromRepo);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed.");
            }

            logger.LogInformation(100, $"Book {id} for author {authorId} was deleted.");
            return NoContent();
        }
        #endregion

        #region Update

        // ***  This could be heavily refactored ..... THERE IS A TON OF DUPLICATED CODE
        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description must be different than the Title.");
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepository = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepository == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;
                libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }
            Mapper.Map(book, bookForAuthorFromRepository);
            libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepository);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed.");
            }
            return NoContent();
        }

        // ***  This could be heavily refactored ..... THERE IS A TON OF DUPLICATED CODE
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdatedBookForAuthor(Guid authorId, Guid id, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();

                patchDoc.ApplyTo(bookDto, ModelState);
                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto), "The description must be different that the title");
                }
                TryValidateModel(bookDto);
                if (!ModelState.IsValid)
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;

                libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);
            patchDoc.ApplyTo(bookToPatch, ModelState);
            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The description must be different that the title");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            Mapper.Map(bookToPatch, bookForAuthorFromRepo);
            libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }
        #endregion

    }
}
