using System.Collections.Immutable;
using System.Net;
using System.Numerics;
using System.Security.Principal;
using GameVerse_recommendation.DTO;
using GameVerse_recommendation.Models;
using GameVerse_recommendation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Caching.Memory;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;

namespace GameVerse_recommendation.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly VideogameStoreContext _context;
        private readonly UserManager<Player> _userManager;
        private readonly RecommendationsCache _recommendationsCache;
        public IndexModel(VideogameStoreContext context, UserManager<Player> userManager, RecommendationsCache recommendationsCache)
        {
            _context = context;
            _userManager = userManager;
            _recommendationsCache = recommendationsCache;
        }

        public bool IsRecommending { get; set; } = false;
        public int PagesCount { get; set; }
        public int CurrentPage { get; private set; } = 1;
        public int TotalGamesOnPage { get; private set; } = 12;

        public IReadOnlyList<GameDTO> Games { get; set; } = new List<GameDTO>();

        public async Task OnGetAsync(int currentPage)
        {
            int gamesCount = await _context.Games.CountAsync();
            PagesCount = gamesCount / TotalGamesOnPage;
            CurrentPage = currentPage;
            var player = await GetAuthenticatedPlayerAsync();

            var playersGamesSet = player.IdGames.Select(p => p.Id).ToImmutableHashSet();

            Games = await _context.Games.Skip((currentPage - 1) * TotalGamesOnPage).Take(TotalGamesOnPage).Select(x => new GameDTO()
            {
                Id = x.Id,
                Name = x.Name,
                GameStudio = x.GameStudio.Name ?? String.Empty,
                Genre = x.Genre,
                ImageURL = x.ImageUrl ?? String.Empty,
                Multiplayer = x.IsMultiplayer,
                IsLikes = playersGamesSet.Contains(x.Id),
                Publisher = x.Publisher.Name ?? String.Empty,
                Rating = x.Rating ?? 0
            }).ToListAsync();

            if (!IsRecommending)
            {
                if (_recommendationsCache.TryGetValue<List<int>>(player.Id, out List<int>? gameRecommendations))
                {
                    IsRecommending = true;
                }
            }
        }

        public async Task<IActionResult> OnGetShowAsync()
        {
            var player = await GetAuthenticatedPlayerAsync();

            if (_recommendationsCache.TryGetValue<List<int>>(player.Id, out List<int>? gameRecommendations))
            {
                if (gameRecommendations is null)
                    return RedirectToPage("Index");

                Games = await _context.Games.Where(x => gameRecommendations.Contains(x.Id)).Select(x => new GameDTO()
                {
                    Id = x.Id,
                    Name = x.Name,
                    GameStudio = x.GameStudio.Name ?? String.Empty,
                    Genre = x.Genre,
                    ImageURL = x.ImageUrl ?? String.Empty,
                    Multiplayer = x.IsMultiplayer,
                    IsLikes = false,
                    Publisher = x.Publisher.Name ?? String.Empty,
                    Rating = x.Rating ?? 0
                }).ToListAsync();
                TotalGamesOnPage = Games.Count;
                return Page();
            }
            else
            {
                RedirectToPage("Index");
            }

            return NotFound();
        }

        private async Task<Player> GetAuthenticatedPlayerAsync()
        {
            var player = await _userManager.FindByNameAsync(User.Identity!.Name!);
            if (player is null)
            {
                throw new AuthenticationFailureException("User is authenticated, but there is no such User in database");
            }

            return player;
        }
    }
}