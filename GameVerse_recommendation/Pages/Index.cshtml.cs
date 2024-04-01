using System.Collections.Immutable;
using System.Net;
using System.Security.Principal;
using GameVerse_recommendation.DTO;
using GameVerse_recommendation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace GameVerse_recommendation.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly VideogameStoreContext _context;
        private readonly UserManager<Player> _userManager;
        public IndexModel(VideogameStoreContext context, UserManager<Player> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private readonly ILogger<IndexModel> _logger;
        public bool IsRecommending { get; set; } = false;
        public int PagesCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalGamesOnPage { get; } = 12;

        public IReadOnlyList<GameDTO> Games { get; set; } = new List<GameDTO>();
        public async Task OnGetAsync()
        {

            _ = this.User;
            if (PagesCount == 0)
            {
                int gamesCount = await _context.Games.CountAsync();
                PagesCount = gamesCount / TotalGamesOnPage;
            }

            var player = await _userManager.FindByNameAsync(this.User.Identity!.Name!);
            if (player is null)
            {
                throw new AuthenticationFailureException("User is authenticated, but there is no such User in database");
            }

            var playersGamesSet = player.IdGames.Select(p => p.Id).ToImmutableHashSet();

            Games = await _context.Games.Take(TotalGamesOnPage).Select(x => new GameDTO
            {
                Id = x.Id,
                Name = x.Name,
                GameStudio = x.GameStudio.Name ?? "",
                Genre = x.Genre,
                ImageURL = x.ImageUrl ?? "",
                Multiplayer = x.IsMultiplayer,
                Publisher = x.Publisher.Name ?? "",
                Rating = x.Rating ?? 0,
                IsLikes = playersGamesSet.Contains(x.Id)
            }).ToListAsync();

        }
        public async Task OnGetByIdAsync(int id)
        {

        }

        public void OnPost()
        {
            IsRecommending = true;
        }
    }
}