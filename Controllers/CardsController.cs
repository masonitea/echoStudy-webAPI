using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using echoStudy_webAPI.Models;
using echoStudy_webAPI.Data;
using Microsoft.AspNetCore.Identity;
using echoStudy_webAPI.Areas.Identity.Data;
using System.Net.Http;

namespace echoStudy_webAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly EchoStudyDB _context;
        private readonly UserManager<EchoUser> _userManager;

        public CardsController(EchoStudyDB context, UserManager<EchoUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /**
         * This class should contain all information that should be returned to the user in a GET request
         */
        public class CardInfo
        {
            public int id { get; set; }
            public string ftext { get; set; }
            public string btext { get; set; }
            public string faud { get; set; }
            public string baud { get; set; }
            public string flang { get; set; }
            public string blang { get; set; }
            public int deckId { get; set; }
            public int score { get; set; }
            public string ownerId { get; set; }
            public DateTime date_created { get; set; }
            public DateTime date_updated { get; set; }
            public DateTime date_touched { get; set; }
        }

        /**
         * This class should contain all information that should be provided in order to create or update a card
         */
        public class PostCardInfo
        {
            public string frontText { get; set; }
            public string backText { get; set; }
            public string frontLang { get; set; }
            public string backLang { get; set; }
            public string userId { get; set; }
            public int? deckId { get; set; }
        }

        /**
         * Define response type for Card creation
         */
        public class CardCreationResponse
        {
            public int id { get; set; }
            public DateTime dateCreated { get; set; }
        }

        /**
         * Define response type for Card update
         */
        public class CardUpdateResponse
        {
            public int id { get; set; }
            public DateTime dateUpdated { get; set; }
        }

        // GET: /Cards
        /// <summary>
        /// Retrieves all Card objects, Card objects related to a user by Id or by Email, or Card
        /// objects associated with a specific Deck
        /// </summary>
        /// <remarks>If no parameter is specified, returns all card objects.
        /// If userId or userEmail is specified, returns the cards related to the given user. If
        /// both parameters are specified, userId takes precedence.
        /// </remarks>
        /// <param name="userId">The ASP.NET Id of the related user. Overrides <c>userEmail</c> and <c>deckId</c> if present</param>
        /// <param name="userEmail">The email address of the related user. Overrides <c>deckId</c> if present</param>
        /// <param name="deckId">The ID of the related deck</param>"
        /// <response code="200">Returns the queried Card objects</response>
        /// <response code="404">Object not found with the provided userId, userEmail, or deckId</response>
        [HttpGet]
        [Produces("application/json", "text/plain")]
        [ProducesResponseType(typeof(IQueryable<CardInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CardInfo>>> GetCards(string userId, string userEmail, int? deckId)
        {
            if (userId is not null)
            {
                EchoUser user = await _userManager.FindByIdAsync(userId);

                if (user is null) return NotFound("provided userId not found");

                var queryuserid = from c in _context.Cards
                                  where c.UserId == userId
                                  select new CardInfo
                                  {
                                      id = c.CardID,
                                      ftext = c.FrontText,
                                      btext = c.BackText,
                                      faud = c.FrontAudio,
                                      baud = c.BackAudio,
                                      flang = c.FrontLang.ToString(),
                                      blang = c.BackLang.ToString(),
                                      deckId = c.DeckID,
                                      score = c.Score,
                                      ownerId = c.UserId,
                                      date_created = c.DateCreated,
                                      date_updated = c.DateUpdated,
                                      date_touched = c.DateTouched
                                  };
                return Ok(await queryuserid.ToListAsync());
            }

            if (userEmail is not null)
            {
                EchoUser user = await _userManager.FindByEmailAsync(userEmail);

                if (user is null) return NotFound("provided userId not found");

                var queryemail = from c in _context.Cards
                                 where c.UserId == user.Id
                                 select new CardInfo
                                 {
                                     id = c.CardID,
                                     ftext = c.FrontText,
                                     btext = c.BackText,
                                     faud = c.FrontAudio,
                                     baud = c.BackAudio,
                                     flang = c.FrontLang.ToString(),
                                     blang = c.BackLang.ToString(),
                                     deckId = c.DeckID,
                                     score = c.Score,
                                     ownerId = c.UserId,
                                     date_created = c.DateCreated,
                                     date_updated = c.DateUpdated,
                                     date_touched = c.DateTouched
                                 };
                return Ok(await queryemail.ToListAsync());
            }

            if (deckId is not null)
            {
                var querydeck = from c in _context.Cards
                                where c.DeckID == deckId
                                select new CardInfo
                                {
                                    id = c.CardID,
                                    ftext = c.FrontText,
                                    btext = c.BackText,
                                    faud = c.FrontAudio,
                                    baud = c.BackAudio,
                                    flang = c.FrontLang.ToString(),
                                    blang = c.BackLang.ToString(),
                                    deckId = c.DeckID,
                                    score = c.Score,
                                    ownerId = c.UserId,
                                    date_created = c.DateCreated,
                                    date_updated = c.DateUpdated,
                                    date_touched = c.DateTouched
                                };
                return Ok(await querydeck.ToListAsync());
            }

            var queryall = from c in _context.Cards
                           select new CardInfo
                           {
                               id = c.CardID,
                               ftext = c.FrontText,
                               btext = c.BackText,
                               faud = c.FrontAudio,
                               baud = c.BackAudio,
                               flang = c.FrontLang.ToString(),
                               blang = c.BackLang.ToString(),
                               deckId = c.DeckID,
                               score = c.Score,
                               ownerId = c.UserId,
                               date_created = c.DateCreated,
                               date_updated = c.DateUpdated,
                               date_touched = c.DateTouched
                           };
            return Ok(await queryall.ToListAsync());
        }

        // GET: /Cards/{id}
        /// <summary>
        /// Retrieves one Card specified by Id
        /// </summary>
        /// <remarks>
        /// <param name="id">Card ID</param>
        /// </remarks>
        /// <response code="200">Returns the queried Card object</response>
        /// <response code="404">Object not found with the cardId</response>
        [HttpGet("{id}")]
        [Produces("application/json", "text/plain")]
        [ProducesResponseType(typeof(CardInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CardInfo>> GetCard(int id)
        {
            var query = from c in _context.Cards
                        where c.CardID == id
                        select new CardInfo
                        {
                            id = c.CardID,
                            ftext = c.FrontText,
                            btext = c.BackText,
                            faud = c.FrontAudio,
                            baud = c.BackAudio,
                            flang = c.FrontLang.ToString(),
                            blang = c.BackLang.ToString(),
                            deckId = c.DeckID,
                            score = c.Score,
                            ownerId = c.UserId,
                            date_created = c.DateCreated,
                            date_updated = c.DateUpdated,
                            date_touched = c.DateTouched
                        };

            if (!query.Any())
            {
                return NotFound("CardId " + id + " was not found");
            }

            return Ok(await query.FirstAsync());
        }

        /*
 * "touches" given card by id
 */
        /*
        // PATCH: /Cards/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("Touch={id}&{score}")]
        public async Task<IActionResult> TouchCard(int id, int score)
        {
            var cardQuery = from c in _context.Cards
                            where c.CardID == id
                            select c;
            // Card doesn't exist
            if (!cardQuery.Any())
            {
                return BadRequest("Card " + id + " does not exist");
            }
            // Update the card
            else
            {
                Card card = cardQuery.First();

                // Update score and last touched
                card.Score = score;
                card.DateTouched = DateTime.Now;

                // Mark the card as modified
                _context.Entry(card).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok();
            }
        }
        */

        // Post: /Cards/{id}
        /// <summary>
        /// Edits an existing Card
        /// </summary>
        /// <remarks></remarks>
        /// <param name="id">ID of the Card to edit</param>
        /// <param name="cardInfo">
        /// Optional: frontText, backText, frontLang, backLang, userId, deckId
        /// </param>
        /// <response code="200">Returns the Id and DateUpdated of the edited Card</response>
        /// <response code="400">Invalid input or input type</response>
        /// <response code="404">Object at the cardId provided was not found</response>
        [HttpPost("{id}")]
        [Produces("application/json", "text/plain")]
        [ProducesResponseType(typeof(CardUpdateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostCardEdit(int id, PostCardInfo cardInfo)
        {
            Card card;
            var cardQuery = from c in _context.Cards
                            where c.CardID == id
                            select c;
            if ((card = cardQuery.First()) is null) return NotFound("Card id " + id + " not found");

            //-------Update according to incoming info
            if (cardInfo.userId is not null)
            {
                EchoUser user = await _userManager.FindByIdAsync(cardInfo.userId);
                if (user is null) return BadRequest("User " + cardInfo.userId + " not found");

                card.UserId = user.Id;
            }
            if (cardInfo.frontText is not null)
            {
                card.FrontText = cardInfo.frontText;
            }
            if (cardInfo.backText is not null)
            {
                card.BackText = cardInfo.backText;
            }
            if (cardInfo.frontLang is not null)
            {
                switch (cardInfo.frontLang.ToLower())
                {
                    case "english":
                        card.FrontLang = Language.English;
                        break;
                    case "spanish":
                        card.FrontLang = Language.Spanish;
                        break;
                    case "japanese":
                        card.FrontLang = Language.Japanese;
                        break;
                    case "german":
                        card.FrontLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
                }
            }
            if (cardInfo.backLang is not null)
            {
                switch (cardInfo.backLang.ToLower())
                {
                    case "english":
                        card.BackLang = Language.English;
                        break;
                    case "spanish":
                        card.BackLang = Language.Spanish;
                        break;
                    case "japanese":
                        card.BackLang = Language.Japanese;
                        break;
                    case "german":
                        card.BackLang = Language.German;
                        break;
                    default:
                        return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
                }
            }
            // if the card is changing decks, we need to change both decks' updated date
            int? olddeckid = null;
            if (cardInfo.deckId is not null)
            {
                olddeckid = card.DeckID;
                card.DeckID = (int)cardInfo.deckId;
            }
            // Dates
            card.DateUpdated = DateTime.Now;

            // if we changed the text or language of the front or back of the card, make a call to Polly
            if (cardInfo.frontLang is not null || cardInfo.frontText is not null)
            {
                card.FrontAudio = AmazonPolly.createTextToSpeechAudio(card.FrontText, card.FrontLang);
            }
            if (cardInfo.backLang is not null || cardInfo.backText is not null)
            {
                card.BackAudio = AmazonPolly.createTextToSpeechAudio(card.BackText, card.BackLang);
            }

            _context.Cards.Update(card);

            // Get the deck(s) whose DateUpdated needs to be changed
            // card.DeckID is either the same deck the card started with or the new DeckID
            var deckquery = from d in _context.Decks
                            where d.DeckID == card.DeckID || d.DeckID == olddeckid
                            select d;

            foreach (var deck in deckquery)
            {
                deck.DateUpdated = card.DateUpdated;
                _context.Decks.Update(deck);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500);
            }

            return Ok(new CardUpdateResponse
            {
                id = card.CardID,
                dateUpdated = card.DateUpdated
            });
        }

        // POST: /Cards
        /// <summary>
        /// Creates a Card for a specific user
        /// </summary>
        /// <param name="cardInfo">
        /// Required: frontText, backText, frontLang, backLang, userId, deckId
        /// </param>
        /// <remarks></remarks>
        /// <response code="201">Returns the id and creation date of the created Card</response>
        /// <response code="400">Invalid input or input type</response>
        /// <response code="404">Object at userId or deckId provided was not found</response>
        [HttpPost]
        [Produces("application/json", "text/plain")]
        [ProducesResponseType(typeof(CardCreationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostCardCreate(PostCardInfo cardInfo)
        {
            if (cardInfo.frontText is null)
            {
                return BadRequest("frontText is required");
            }
            if (cardInfo.backText is null)
            {
                return BadRequest("backText is required");
            }
            if (cardInfo.frontLang is null)
            {
                return BadRequest("frontLang is required");
            }
            if (cardInfo.backLang is null)
            {
                return BadRequest("backLang is required");
            }
            if (cardInfo.userId is null)
            {
                return BadRequest("userId is required");
            }
            if (cardInfo.deckId is null)
            {
                return BadRequest("deckId is required");
            }

            // Create a card and assign it all of the basic provided data
            Card card = new Card();

            EchoUser user = await _userManager.FindByIdAsync(cardInfo.userId);
            if (user is null) return BadRequest("User " + cardInfo.userId + " not found");
            card.UserId = user.Id;

            card.FrontText = cardInfo.frontText;
            card.BackText = cardInfo.backText;
            card.DeckID = (int)cardInfo.deckId;

            switch (cardInfo.frontLang.ToLower())
            {
                case "english":
                    card.FrontLang = Language.English;
                    break;
                case "spanish":
                    card.FrontLang = Language.Spanish;
                    break;
                case "japanese":
                    card.FrontLang = Language.Japanese;
                    break;
                case "german":
                    card.FrontLang = Language.German;
                    break;
                default:
                    return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
            }
            switch (cardInfo.backLang.ToLower())
            {
                case "english":
                    card.BackLang = Language.English;
                    break;
                case "spanish":
                    card.BackLang = Language.Spanish;
                    break;
                case "japanese":
                    card.BackLang = Language.Japanese;
                    break;
                case "german":
                    card.BackLang = Language.German;
                    break;
                default:
                    return BadRequest("Current supported languages are English, Spanish, Japanese, and German");
            }

            // Assign it dates and a score of 0
            card.DateCreated = DateTime.Now;
            card.DateTouched = DateTime.Now;
            card.DateUpdated = DateTime.Now;
            card.Score = 0;
            card.DeckPosition = "";

            // Make Polly calls
            card.FrontAudio = AmazonPolly.createTextToSpeechAudio(card.FrontText, card.FrontLang);
            card.BackAudio = AmazonPolly.createTextToSpeechAudio(card.BackText, card.BackLang);

            // Get the related deck and update its DateUpdated
            var deckQuery = from d in _context.Decks
                            where d.DeckID == cardInfo.deckId
                            select d;
            if (!deckQuery.Any())
            {
                return NotFound("Deck ID " + cardInfo.deckId + " not found");
            }
            Deck deck = deckQuery.First();
            card.Deck = deck;
            deck.DateUpdated = card.DateUpdated;
            _context.Decks.Update(deck);

            // Ready to add
            _context.Cards.Add(card);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500);
            }

            return CreatedAtAction("PostCardCreate", new CardCreationResponse
            {
                id = card.CardID,
                dateCreated = card.DateCreated
            });
        }

        // DELETE: /Cards/Delete/{id}
        /// <summary>
        /// Deletes one specific card
        /// </summary>
        /// <param name="id">The card's ID</param>
        /// <response code="204"></response>
        /// <response code="404">Object at cardId was not found</response>
        [HttpPost("Delete/{id}")]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                return NotFound();
            }

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /**
         * Deletes all cards associated with one user
         */
        /*
        // DELETE: api/Cards/5
        [HttpDelete("/Cards/DeleteUserCards={userId}")]
        public async Task<IActionResult> DeleteUserCards(string userId)
        {
            var query = from c in _context.Cards
                        where c.UserId == userId
                        select c;

            List<Card> userCards = await query.ToListAsync();
            foreach(Card card in userCards)
            {
                _context.Cards.Remove(card);
            }
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        /**
 * Deletes all cards associated with one user
 */
        /*
        // DELETE: api/Cards/5
        [HttpDelete("/Cards/DeleteUserCardsByEmail={userEmail}")]
        public async Task<IActionResult> DeleteUserCardsByEmail(string userEmail)
        {
            EchoUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null)
            {
                return BadRequest("User " + userEmail + " not found");
            }
            else
            {
                var query = from c in _context.Cards
                            where c.UserId == user.Id
                            select c;

                List<Card> userCards = await query.ToListAsync();
                foreach (Card card in userCards)
                {
                    _context.Cards.Remove(card);
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
        */

        /**
         * Deletes all cards associated with a deck
         */
        /*
        [HttpDelete("/Cards/DeleteDeckCards={deckId}")]
        public async Task<IActionResult> DeleteDeckCards(int deckId)
        {
            // Grab the deck. Only possible for one or zero results since ids are unique.
            var deckQuery = from d in _context.Decks.Include(d => d.Cards)
                        where d.DeckID == deckId
                        select d;
            if (deckQuery.Count() == 0)
            {
                return BadRequest("Deck id " + deckId + " does not exist");
            }

            Deck deck = deckQuery.First();

            foreach (Card card in deck.Cards)
            {
                _context.Cards.Remove(card);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
        */

        private bool CardExists(int id)
        {
            return _context.Cards.Any(e => e.CardID == id);
        }
    }
}
