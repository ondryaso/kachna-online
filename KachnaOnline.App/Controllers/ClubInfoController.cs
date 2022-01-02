// ClubInfoController.cs
// Author: Ondřej Ondryáš

using System.Collections.Generic;
using System.Threading.Tasks;
using KachnaOnline.App.Extensions;
using KachnaOnline.Business.Facades;
using KachnaOnline.Dto.ClubInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KachnaOnline.App.Controllers
{
    [ApiController]
    [Route("club")]
    [AllowAnonymous]
    [EnableCors(Startup.LocalCorsPolicy)]
    public class ClubInfoController : ControllerBase
    {
        private readonly ClubInfoFacade _facade;

        public ClubInfoController(ClubInfoFacade facade)
        {
            _facade = facade;
        }

        /// <summary>
        /// Returns the current offer of refreshments (including beers on tap) that can be bought in the club
        /// when it's open.
        /// </summary>
        /// <response code="200">The current offer of refreshments and beer.</response>
        [HttpGet("offer")]
        public async Task<ActionResult<OfferDto>> GetCurrentOffer()
        {
            var offer = await _facade.GetCurrentOffer();
            if (offer is null)
                return this.GeneralProblem("Cannot fetch current offer from KIS.");

            return offer;
        }

        /// <summary>
        /// Returns the top 10 places in today's prestige leaderboard.
        /// </summary>
        /// <response code="200">Today's leaderboard in ascending order.</response>
        [HttpGet("leaderboard/today")]
        public async Task<ActionResult<List<LeaderboardItemDto>>> GetTodayLeaderboard()
        {
            var leaderboard = await _facade.GetTodayLeaderboard();
            if (leaderboard is null)
                return this.GeneralProblem("Cannot fetch current leaderboard from KIS.");

            return leaderboard;
        }

        /// <summary>
        /// Returns the top 10 places in the current semester's prestige leaderboard.
        /// </summary>
        /// <remarks>
        /// A 'semester' means one of three periods: 1 Sep to 31 Jan (winter semester), 1 Feb to 31 May (summer
        /// semester) or 1 June to 31 Aug (summer holiday).
        /// </remarks>
        /// <response code="200">The current semester's leaderboard in ascending order.</response>
        [HttpGet("leaderboard/semester")]
        public async Task<ActionResult<List<LeaderboardItemDto>>> GetSemesterLeaderboard()
        {
            var leaderboard = await _facade.GetSemesterLeaderboard();
            if (leaderboard is null)
                return this.GeneralProblem("Cannot fetch current leaderboard from KIS.");

            return leaderboard;
        }
    }
}
