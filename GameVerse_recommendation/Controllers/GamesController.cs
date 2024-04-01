using GameVerse_recommendation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameVerse_recommendation.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class GamesController : ControllerBase
    {
        private readonly VideogameStoreContext _context;
        private readonly UserManager<Player> _userManager;
        public GamesController(VideogameStoreContext context, UserManager<Player> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> UpdateLikesGameAsync(int id)
        {
            bool isAdded;
            var game = await (from g in _context.Games where g.Id == id select g).FirstOrDefaultAsync();
            if (game is not null)
            {
                var user = await _userManager.FindByNameAsync(this.User.Identity!.Name!);
                if (user is not null)
                {
                    if (user.IdGames.Contains(game))
                    {
                        user.IdGames.Remove(game);
                        isAdded = false;
                    }
                    else
                    {
                        user.IdGames.Add(game);
                        isAdded = true;
                    }
                    await _context.SaveChangesAsync();
                }
                else
                    return Forbid("No such authorized user was find");
            }
            else
                return BadRequest($"Item with id \"{id}\" was not found");

            return Ok(new {message = isAdded});
        }
    }
}
