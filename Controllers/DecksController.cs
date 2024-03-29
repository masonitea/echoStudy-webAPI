﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using echoStudy_webAPI.Models;
using echoStudy_webAPI.Data;
using echoStudy_webAPI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using ThirdParty.Ionic.Zlib;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net.Http;

namespace echoStudy_webAPI.Controllers
{
    [ApiController]
    [Route("decks")]
    public class DecksController : EchoUserControllerBase
    {
        private readonly EchoStudyDB _context;

        public DecksController(EchoStudyDB context, UserManager<EchoUser> um)
            : base(um)
        {
            _context = context;
        }

        /**
         * This class should contain all information needed in a GET request
         */
        public class DeckInfo
        {
            public int id { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string access { get; set; }
            public string default_flang { get; set; }
            public string default_blang { get; set; }
            public string ownerId { get; set; }
            public string ownerUserName { get; set; }
            public List<int> cards { get; set; }
            public double studiedPercent { get; set; }
            public double masteredPercent { get; set; }
            public DateTime date_created { get; set; }
            public DateTime date_updated { get; set; }
            public DateTime date_touched { get; set; }
            public int? orig_deck_id { get; set; }
            public string orig_author_id { get; set; }
            public string orig_author_name { get; set; }
            public string owner_profile_pic { get; set; }
            public string orig_author_profile_pic { get; set; }
        }

        /**
         * This class should contain all information needed from the user to create a new deck
         */
        public class PostDeckInfo
        {
            public string title { get; set; }
            public string description { get; set; }
            public string access { get; set; }
            public string default_flang { get; set; }
            public string default_blang { get; set; }
            public List<int> cardIds { get; set; }
        }

        /**
        * Define response type for Deck creation
        */
        public class DeckCreationResponse
        {
            public List<int> ids { get; set; }
            public DateTime dateCreated { get; set; }
        }

        /**
         * Define response type for Deck update
         */
        public class DeckUpdateResponse
        {
            public int id { get; set; }
            public DateTime dateUpdated { get; set; }
        }

        // GET: /Decks
        /// <summary>
        /// Retrieves all Deck objects belonging to the authenticated user
        /// </summary>
        /// <remarks>
        /// User authentication is encoded in the JSON Web Token provided in the Authorization header
        /// </remarks>
        /// <response code="200">Returns the user's Deck objects</response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IQueryable<DeckInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<DeckInfo>>> GetMyDecks()
        {
            // Query the DB for the deck objects
            var query = from d in _context.Decks.Include(d => d.Cards)
                                                .Include(d => d.OrigAuthor)
                                                .Include(d => d.DeckOwner)
                        where d.UserId == _user.Id
                        select d;
            var decks = await query.ToListAsync();

            // Build the deck info objects
            List<DeckInfo> deckInfo = new List<DeckInfo>();
            foreach(Deck d in decks)
            {
                deckInfo.Add(new DeckInfo
                {
                    id = d.DeckID,
                    title = d.Title,
                    description = d.Description,
                    access = d.Access.ToString(),
                    default_flang = d.DefaultFrontLang.ToString(),
                    default_blang = d.DefaultBackLang.ToString(),
                    cards = d.Cards.Select(c => c.CardID).ToList(),
                    ownerId = d.UserId,
                    ownerUserName = d.DeckOwner.UserName,
                    studiedPercent = (double) d.StudyPercent,
                    masteredPercent = (double)d.MasteredPercent,
                    date_created = d.DateCreated,
                    date_touched = d.DateTouched,
                    date_updated = d.DateUpdated,
                    orig_deck_id = d.OrigDeckId,
                    orig_author_id = d.OrigAuthorId,
                    orig_author_name = d.OrigAuthor?.UserName
                });
            }

            return Ok(deckInfo);
        }
        /*
        // ******TODO: Does not depend at all on current user. Move to a "Public" controller?
        // GET: /Decks/Public
        /// <summary>
        /// Retrieves Public Decks
        /// </summary>
        /// <remarks>
        /// All Decks with an access level of Public
        /// </remarks>
        /// <response code="200">Returns all Public Deck objects</response>
        [HttpGet("Public")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IQueryable<DeckInfo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DeckInfo>>> GetPublicDecks()
        {
            var query = from d in _context.Decks
                        where d.Access == Data.Access.Public
                        select new DeckInfo
                        {
                            id = d.DeckID,
                            title = d.Title,
                            description = d.Description,
                            access = d.Access.ToString(),
                            default_flang = d.DefaultFrontLang.ToString(),
                            default_blang = d.DefaultBackLang.ToString(),
                            cards = d.Cards.Select(c => c.CardID).ToList(),
                            ownerId = d.UserId,
                            date_created = d.DateCreated,
                            date_touched = d.DateTouched,
                            date_updated = d.DateUpdated,
                            orig_deck_id = d.OrigDeckId,
                            orig_author_id = d.OrigAuthorId,
                            orig_author_name = d.OrigAuthor.UserName
                        };
            return Ok(await query.ToListAsync());
        }

        // ******TODO: Does not depend at all on current user. Move to a "Public" controller?
        // GET: /Decks/Public
        /// <summary>
        /// Retrieves a user's public decks through email or username
        /// </summary>
        /// <param name="user">Email or username of the target user</param>
        /// <response code="200">Returns the user's Public Deck objects</response>
        /// <response code="401">User was not found</response>
        [HttpGet("Public/{user}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IQueryable<DeckInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IQueryable<DeckInfo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DeckInfo>>> GetPublicUserDecks(string user)
        {
            // Try to find the user
            EchoUser target = null;
            if (user.Contains('@'))
            {
                target = await _um.FindByEmailAsync(user);
            }
            else
            {
                target = await _um.FindByNameAsync(user);
            }

            // Was the user found?
            if(target is null)
            {
                return NotFound();
            }

            var query = from d in _context.Decks
                        where d.UserId == target.Id && d.Access == Access.Public
                        select new DeckInfo
                        {
                            id = d.DeckID,
                            title = d.Title,
                            description = d.Description,
                            access = d.Access.ToString(),
                            default_flang = d.DefaultFrontLang.ToString(),
                            default_blang = d.DefaultBackLang.ToString(),
                            cards = d.Cards.Select(c => c.CardID).ToList(),
                            ownerId = d.UserId,
                            date_created = d.DateCreated,
                            date_touched = d.DateTouched,
                            date_updated = d.DateUpdated
                        };
            return Ok(await query.ToListAsync());
        }
        */
        // GET: /Decks/{id}
        /// <summary>
        /// Retrieves one Deck specified by Id. NOT used to retrieve a public deck owned by another user. See Public controller.
        /// </summary>
        /// <remarks>
        /// <param name="id">Deck ID</param>
        /// </remarks>
        /// <response code="200">Returns the queried Deck object</response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        /// <response code="403">The current user is not authorized to access this deck</response>
        /// <response code="404">Deck object with the given id was not found</response>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Deck), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ForbidResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusCodeResult), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeckInfo>> GetDeck(int id)
        {
            // d.DeckID is unique; queryDeck returns only one deck

            // first query to see if the desired deck exists
            var queryDeck = from d in _context.Decks.Include(d => d.Cards)
                                                    .Include(d => d.OrigAuthor)
                                                    .Include(d => d.DeckOwner)
                            where d.DeckID == id
                            select d;

            Deck deck = await queryDeck.FirstOrDefaultAsync();

            if(deck is null)
            {
                return NotFound();
            }

            // ensure the user is the deck owner
            if(deck.UserId != _user.Id)
            {
                return Forbid();
            }

            return Ok(new DeckInfo
            {
                id = deck.DeckID,
                title = deck.Title,
                description = deck.Description,
                access = deck.Access.ToString(),
                default_flang = deck.DefaultFrontLang.ToString(),
                default_blang = deck.DefaultBackLang.ToString(),
                cards = deck.Cards.Select(c => c.CardID).ToList(),
                ownerId = deck.UserId,
                ownerUserName = deck.DeckOwner.UserName,
                studiedPercent = (double) deck.StudyPercent,
                masteredPercent = (double) deck.MasteredPercent,
                date_created = deck.DateCreated,
                date_touched = deck.DateTouched,
                date_updated = deck.DateUpdated,
                orig_deck_id = deck.OrigDeckId,
                orig_author_id = deck.OrigAuthorId,
                orig_author_name = deck.OrigAuthor?.UserName
            });
        }

        // POST: /Decks/{id}
        /// <summary>
        /// Edits an existing Deck owned by the authenticated user
        /// </summary>
        /// <remarks>
        /// Currently does not support changing its associated cards
        /// </remarks>
        /// <param name="id">ID of the Deck to edit</param>
        /// <param name="deckInfo">
        /// Optional: title, description, default_flang, default_blang, access, cardIds
        /// </param>
        /// <response code="200">Returns the Id and DateUpdated of the edited Deck</response>
        /// <response code="400">Invalid input or input type</response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        /// <response code="403">The current user is not authorized to edit this deck</response>
        /// <response code="404">Object at the deckId provided was not found</response>
        /// <response code="500">Database failed to save despite valid request</response>
        [HttpPost("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DeckUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ForbidResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusCodeResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostDeckEdit(int id, PostDeckInfo deckInfo)
        {
            Deck deck;
            var deckQuery = from d in _context.Decks
                            where d.DeckID == id
                            select d;
            if((deck = await deckQuery.FirstAsync()) is null) return NotFound("Deck id " + id + " not found");


            if (deck.UserId != _user.Id)
            {
                return Forbid();
            }

            if (deckInfo.title is not null)
            { 
                if (deckInfo.title == "") return BadRequest("title cannot be empty");
                deck.Title = deckInfo.title;
            }

            if (deckInfo.description is not null)
            {
                if (deckInfo.description == "") return BadRequest("description cannot be empty");
                deck.Description = deckInfo.description;
            }

            if (deckInfo.access is not null)
            {
                switch (deckInfo.access.ToLower())
                {
                    case "public":
                        deck.Access = Access.Public;
                        break;
                    case "private":
                        deck.Access = Access.Private;
                        break;
                    default:
                        return BadRequest("Valid access parameter values are Public and Private");
                }
            }
            
            if (deckInfo.default_flang is not null)
            {
                switch (deckInfo.default_flang.ToLower())
                {
                    case "english":
                        deck.DefaultFrontLang = Language.English;
                        break;
                    case "spanish":
                        deck.DefaultFrontLang = Language.Spanish;
                        break;
                    case "japanese":
                        deck.DefaultFrontLang = Language.Japanese;
                        break;
                    case "german":
                        deck.DefaultFrontLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
                }
            }

            if (deckInfo.default_blang is not null)
            {
                switch (deckInfo.default_blang.ToLower())
                {
                    case "english":
                        deck.DefaultBackLang = Language.English;
                        break;
                    case "spanish":
                        deck.DefaultBackLang = Language.Spanish;
                        break;
                    case "japanese":
                        deck.DefaultBackLang = Language.Japanese;
                        break;
                    case "german":
                        deck.DefaultBackLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
                }
            }

            // Owner
            deck.UserId = _user.Id;
            
            // Dates
            deck.DateUpdated = DateTime.Now;

            // Deck has been updated, break any connection with original deck if this was a shared deck
            deck.OrigDeckId = null;
            deck.OrigAuthorId = null;


            // Try to save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, e.Message);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, e.Message);
            }

            return Ok(new DeckUpdateResponse
            {
                id = deck.DeckID,
                dateUpdated = deck.DateUpdated
            });
        }

        // POST: /Decks
        /// <summary>
        /// Creates decks for the currently authenticated user through the provided list
        /// </summary>
        /// <param name="decks">
        /// List of decks to create
        /// Required: title, description, default_flang, default_blang -- Optional: access, cardIds
        /// </param>
        /// <remarks>Default access level is Private. The cardIds list currently does nothing.</remarks>
        /// <response code="201">Returns the id and creation date of the created Deck</response>
        /// <response code="400">Invalid input or input type</response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        /// <response code="404">Object at cardId provided was not found</response>
        /// <response code="500">Database failed to save despite valid request</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DeckCreationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusCodeResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostDeckCreate(List<PostDeckInfo> decks)
        {
            HttpResponseMessage x = null;

            List<Deck> createdDecks = new List<Deck>();
            foreach (PostDeckInfo deckInfo in decks)
            {
                // Create and populate a deck with the given info
                Deck deck = new();

                if (String.IsNullOrEmpty(deckInfo.title))
                {
                    return BadRequest("a non-empty title is required at index " + createdDecks.Count);
                }
                if (String.IsNullOrEmpty(deckInfo.description))
                {
                    return BadRequest("a non-empty description is required at index " + createdDecks.Count);
                }
                if (String.IsNullOrEmpty(deckInfo.default_flang))
                {
                    return BadRequest("a non-empty default_flang is required at index " + createdDecks.Count);
                }
                if (String.IsNullOrEmpty(deckInfo.default_blang))
                {
                    return BadRequest("a non-empty default_blang is required  at index " + createdDecks.Count);
                }

                // Add owner
                deck.UserId = _user.Id;

                //----------------Set the rest of the deck info
                deck.Title = deckInfo.title;
                deck.Description = deckInfo.description;
                // Handle the enums with switch cases
                if (deckInfo.access is null) deck.Access = Access.Private;
                else
                {
                    switch (deckInfo.access.ToLower())
                    {
                        case "public":
                            deck.Access = Access.Public;
                            break;
                        case "private":
                            deck.Access = Access.Private;
                            break;
                        default:
                            return BadRequest("access should be public, private, or omitted (default private) at index " + createdDecks.Count);
                    }
                }

                switch (deckInfo.default_flang.ToLower())
                {
                    case "english":
                        deck.DefaultFrontLang = Language.English;
                        break;
                    case "spanish":
                        deck.DefaultFrontLang = Language.Spanish;
                        break;
                    case "japanese":
                        deck.DefaultFrontLang = Language.Japanese;
                        break;
                    case "german":
                        deck.DefaultFrontLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German. Deck at index " + createdDecks.Count + " has a language not listed");
                }
                switch (deckInfo.default_blang.ToLower())
                {
                    case "english":
                        deck.DefaultBackLang = Language.English;
                        break;
                    case "spanish":
                        deck.DefaultBackLang = Language.Spanish;
                        break;
                    case "japanese":
                        deck.DefaultBackLang = Language.Japanese;
                        break;
                    case "german":
                        deck.DefaultBackLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German. Deck at index " + createdDecks.Count + " has a language not listed");
                }

                // Dates
                deck.DateCreated = DateTime.Now;
                deck.DateTouched = DateTime.Now;
                deck.DateUpdated = DateTime.Now;

                await _context.Decks.AddAsync(deck);
                createdDecks.Add(deck);
            }

            // Try to save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, e.Message);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, e.Message);
            }

            // Now that save changes was called the IDs are generated
            List<int> ids = new List<int>();
            foreach (Deck d in createdDecks)
            {
                ids.Add(d.DeckID);
            }

            return CreatedAtAction("PostDeckCreate", new DeckCreationResponse
            {
                ids = ids,
                dateCreated = DateTime.Now
            });
        }

        // DELETE: /Decks/Delete/{id}
        /// <summary>
        /// Deletes one specific deck owned by the currently authenticated user
        /// </summary>
        /// <param name="id">The deck's ID</param>
        /// <response code="204"></response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        /// <response code="403">The current user is not authorized for this action</response>
        /// <response code="404">Object at deckId was not found</response>
        /// <response code="500">Database failed to save despite valid request</response>
        [HttpPost("delete/{id}")]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ForbidResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusCodeResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDeck(int id)
        {
            var deck = await _context.Decks.FindAsync(id);
            if (deck == null)
            {
                return NotFound("DeckId " + id + " was not found");
            }

            if(deck.UserId != _user.Id)
            {
                return Forbid();
            }

            await SeverShares(new List<int> { id });
            _context.Decks.Remove(deck);

            // Try to save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, e.Message);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, e.Message);
            }

            return NoContent();
        }

        // POST: /Decks/Delete?userId={userID}
        /// <summary>
        /// Delete all of one user's decks
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <response code="204"></response>
        /// <response code="401">A valid, non-expired token was not received in the Authorization header</response>
        /// <response code="403">The current user is not authorized for this action</response>
        /// <response code="404">Object at userId was not found</response>
        /// <response code="500">Database failed to save despite valid request</response>
        [HttpPost("delete")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ForbidResult), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(StatusCodeResult), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserDecks(string userId)
        {
            if(userId != _user.Id)
            {
                return Forbid();
            }

            var query = from d in _context.Decks
                        where d.UserId == _user.Id
                        select d;

            List<Deck> userDecks = await query.ToListAsync();
            List<int> userDeckIds = new();
            foreach(Deck deck in userDecks)
            {
                userDeckIds.Add(deck.DeckID);
            }

            await SeverShares(userDeckIds);

            foreach (Deck deck in userDecks)
            {
                _context.Decks.Remove(deck);
            }

            // Try to save
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(500, e.Message);
            }
            catch (DbUpdateException e)
            {
                return StatusCode(500, e.Message);
            }

            return NoContent();
        }

        // this only tracks the changes to OrigDeckIds. SaveChanges must be executed in the calling function.
        private async Task SeverShares(List<int> deckIds)
        {
            var query = _context.Decks.Where(x => deckIds.Contains(x.DeckID));
            List<Deck> derivedDecksList = await query.ToListAsync();

            foreach(Deck d in derivedDecksList)
            {
                d.OrigDeckId = null;
            }
        }
    }
}
