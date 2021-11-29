// BoardGamesReservationsController.cs
// Author: František Nečas

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using KachnaOnline.App.Extensions;
using KachnaOnline.Business.Constants;
using KachnaOnline.Business.Exceptions;
using KachnaOnline.Business.Exceptions.BoardGames;
using KachnaOnline.Business.Facades;
using KachnaOnline.Dto.BoardGames;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KachnaOnline.App.Controllers
{
    [ApiController]
    [Route("boardGames/reservations")]
    [Authorize]
    public class BoardGamesReservationsController : ControllerBase
    {
        private readonly BoardGamesFacade _facade;

        public BoardGamesReservationsController(BoardGamesFacade facade)
        {
            _facade = facade;
        }

        /// <summary>
        /// Returns a list of the authenticated user's reservations.
        /// </summary>
        /// <param name="state">If present, only reservations of this overall state will be returned.</param>
        /// <response code="200">The list of reservations.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<List<ReservationDto>>> GetReservations(ReservationState? state)
        {
            var userId = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
            return await _facade.GetUserReservations(userId, state);
        }

        /// <summary>
        /// Returns a list of all reservations in the system.
        /// </summary>
        /// <param name="state">If present, only reservations of this overall state will be returned.</param>
        /// <response code="200">The list of all reservations.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("all")]
        public async Task<ActionResult<List<ManagerReservationDto>>> GetAllReservations(ReservationState? state)
        {
            return await _facade.GetAllReservations(state, null);
        }

        /// <summary>
        /// Returns a list of all reservations in the system assigned to the specified user.
        /// </summary>
        /// <param name="userId">ID of the user to search in reservation assignments.</param>
        /// <param name="state">If present, only reservations of this overall state will be returned.</param>
        /// <response code="200">The list of all reservations assigned to the user.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("all/assignedTo/{userId}")]
        public async Task<ActionResult<List<ManagerReservationDto>>> GetAssignedReservations(int userId,
            ReservationState? state)
        {
            return await _facade.GetAllReservations(state, userId);
        }

        /// <summary>
        /// Returns a list of all reservations in the system assigned to the authenticated user.
        /// </summary>
        /// <param name="state">If present, only reservations of this overall state will be returned.</param>
        /// <response code="200">The list of all reservations assigned to the authenticated user.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("all/assignedTo/me")]
        public async Task<ActionResult<List<ManagerReservationDto>>> GetAssignedReservations(
            ReservationState? state)
        {
            var userId = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
            return await _facade.GetAllReservations(state, userId);
        }

        /// <summary>
        /// Returns a reservation with the given ID.
        /// </summary>
        /// <remarks>
        /// A `ManagerReservationDto` is returned if the user is an authorized board games manager.
        /// </remarks>
        /// <param name="id">ID of the reservation to return.</param>
        /// <response code="200">The reservation.</response>
        /// <response code="403">The user is not a board games manager and the reservation belongs to another user.</response>
        /// <response code="404">No such reservation exists.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            try
            {
                return await _facade.GetReservation(this.User, id);
            }
            catch (NotABoardGamesManagerException)
            {
                return this.ForbiddenProblem("The reservation belongs to another user.");
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
        }

        /// <summary>
        /// Returns an item of a reservation with the given ID.
        /// </summary>
        /// <param name="id">ID of the reservation which the item is in.</param>
        /// <param name="itemId">ID of the item to return.</param>
        /// <response code="200">The reservation item.</response>
        /// <response code="403">The user is not a board games manager and the reservation belongs to another user.</response>
        /// <response code="404">No such reservation item exists.</response>
        [HttpGet("{id}/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservationItemDto>> GetReservationItem(int id, int itemId)
        {
            try
            {
                return await _facade.GetReservationItem(this.User, id, itemId);
            }
            catch (NotABoardGamesManagerException)
            {
                return this.ForbiddenProblem("The reservation belongs to another user.");
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
        }

        /// <summary>
        /// Creates a new reservation.
        /// </summary>
        /// <param name="creationDto">A model describing the new reservation.</param>
        /// <response code="201">The created reservation.</response>
        /// <response code="404">When a requested game does not exist.</response>
        /// <response code="409">All of the given board games could not be reserved (e.g. are not available).
        /// Returns the first conflicting board game ID.</response>
        [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status409Conflict)]
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationDto creationDto)
        {
            try
            {
                var user = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
                var reservation = await _facade.CreateNewReservation(user, creationDto);
                return this.CreatedAtAction(nameof(this.GetReservation), new { id = reservation.Id }, reservation);
            }
            catch (BoardGameNotFoundException)
            {
                return this.NotFoundProblem("The specified board game does not exist.");
            }
            catch (GameUnavailableException e)
            {
                return this.Conflict(e.UnavailableBoardGameId);
            }
        }

        /// <summary>
        /// Updates the public note of a reservation with the given ID.
        /// </summary>
        /// <param name="id">ID of the reservation to update.</param>
        /// <param name="note">The new user note.</param>
        /// <response code="204">The reservation was updated.</response>
        /// <response code="403">The reservation belongs to another user.</response>
        /// <response code="404">No such reservation exists.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}/note")]
        public async Task<IActionResult> UpdateReservationNote(int id,
            [StringLength(1024)][FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]
            string note)
        {
            try
            {
                var userId = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
                await _facade.UpdateReservationNote(id, userId, note);
                return this.NoContent();
            }
            catch (ReservationAccessDeniedException)
            {
                return this.ForbiddenProblem("The reservation belongs to another user.");
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
        }

        /// <summary>
        /// Updates the internal note of a reservation with the given ID.
        /// </summary>
        /// <param name="id">ID of the reservation to update.</param>
        /// <param name="note">The new internal note.</param>
        /// <response code="204">The reservation was updated.</response>
        /// <response code="404">No such reservation exists.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}/noteInternal")]
        public async Task<IActionResult> UpdateReservationNoteInternal(int id,
            [StringLength(1024)][FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string note)
        {
            try
            {
                await _facade.UpdateReservationNoteInternal(id, note);
                return this.NoContent();
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
        }

        /// <summary>
        /// Creates a new reservation for a user.
        /// </summary>
        /// <param name="userId">ID of the user to create the reservation for.</param>
        /// <param name="creationDto">A model describing the reservation to create.</param>
        /// <returns>The created <see cref="ManagerReservationDto"/> if the creation succeeded.</returns>
        /// <response code="201">The created reservation.</response>
        /// <response code="404">When a requested game or user to create for does not exist.</response>
        /// <response code="409">All of the given board games could not be reserved (e.g. are not available).
        /// Returns the list of conflicting board game IDs.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(typeof(ManagerReservationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status409Conflict)]
        [HttpPost("madeFor/{userId}")]
        public async Task<ActionResult<ManagerReservationDto>> ManagerCreateReservation(int userId,
            ManagerCreateReservationDto creationDto)
        {
            try
            {
                var creatingUser = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
                var reservation = await _facade.ManagerCreateNewReservation(creatingUser, userId, creationDto);
                return this.CreatedAtAction(nameof(this.GetReservation), new { id = reservation.Id }, reservation);
            }
            catch (BoardGameNotFoundException)
            {
                return this.NotFoundProblem("The specified board game does not exist.");
            }
            catch (UserNotFoundException)
            {
                return this.NotFoundProblem("The specified user does not exist.");
            }
            catch (GameUnavailableException e)
            {
                return this.Conflict(e.UnavailableBoardGameId);
            }
        }

        /// <summary>
        /// Adds extra board games to a reservation with the given ID.
        /// </summary>
        /// <param name="id">ID of the reservation to update.</param>
        /// <param name="newItems">IDs of games to be added to the reservation. If a game is to be added multiple times,
        /// it must be included in the array that many times.</param>
        /// <response code="204">The reservation was updated.</response>
        /// <response code="404">No such reservation exists or a requested game does not exist.</response>
        /// <response code="409">All of the given board games could not be reserved (e.g. are not available).
        /// Returns the list of conflicting board game IDs.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(int), StatusCodes.Status409Conflict)]
        [HttpPost("{id}/items")]
        public async Task<IActionResult> UpdateReservationItems(int id, [FromBody][MinLength(1)] int[] newItems)
        {
            try
            {
                var user = int.Parse(this.User.FindFirstValue(IdentityConstants.IdClaim));
                await _facade.AddReservationItems(id, user, newItems);
                return this.NoContent();
            }
            catch (BoardGameNotFoundException)
            {
                return this.NotFoundProblem("The specified board game does not exist.");
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
            catch (GameUnavailableException e)
            {
                return this.Conflict(e.UnavailableBoardGameId);
            }
        }

        /// <summary>
        /// Retrieves history of a reservation item.
        /// </summary>
        /// <param name="id">ID of the reservation.</param>
        /// <param name="itemId">ID of the item in the reservation.</param>
        /// <response code="200">The item history, sorted chronologically.</response>
        /// <response code="404">No such item or reservation exists.</response>
        [Authorize(Roles = AuthConstants.BoardGamesManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/{itemId}/events")]
        public async Task<ActionResult<List<ReservationItemEventDto>>> GetItemHistory(int id, int itemId)
        {
            try
            {
                return await _facade.GetItemHistory(id, itemId);
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
        }

        /// <summary>
        /// Modifies the state of a reservation item.
        /// </summary>
        /// <param name="id">ID of the reservation.</param>
        /// <param name="itemId">ID of the item in the reservation.</param>
        /// <param name="type">Type of the modification.</param>
        /// <response code="204">Item state updated.</response>
        /// <response code="403">This type of event requires board games manager permissions or the requesting user
        /// must be the owner of this reservation.</response>
        /// <response code="404">No such item or reservation exists.</response>
        /// <response code="409">Such modification is not possible.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("{id}/{itemId}/events")]
        public async Task<IActionResult> ModifyItemState(int id, int itemId, ReservationEventType type)
        {
            try
            {
                await _facade.ModifyItemState(this.User, id, itemId, type);
                return this.NoContent();
            }
            catch (ReservationAccessDeniedException)
            {
                return this.ForbiddenProblem("The reservation belongs to another user.");
            }
            catch (NotABoardGamesManagerException)
            {
                // Shouldn't happen
                return this.ForbiddenProblem();
            }
            catch (ReservationNotFoundException)
            {
                return this.NotFoundProblem("The specified reservation does not exist.");
            }
            catch (InvalidTransitionException)
            {
                return this.ConflictProblem("The requested reservation change is not possible in its current state.",
                    "Invalid transition");
            }
        }
    }
}
