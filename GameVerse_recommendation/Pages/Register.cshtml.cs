using GameVerse_recommendation.DTO;
using GameVerse_recommendation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameVerse_recommendation.Pages
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<Player> _userManager;
        private readonly SignInManager<Player> _signInManager;

        public RegisterModel(UserManager<Player> userManager, SignInManager<Player> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public RegistrationUserObject NewUserData { get; set; } = null!;
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Player? player = await _userManager.FindByNameAsync(NewUserData.UserName);

                if (player is null)
                {
                    player = new() { UserName = NewUserData.UserName, Email = NewUserData.Email };
                    IdentityResult registerResult =  await _userManager.CreateAsync(player, NewUserData.Password);

                    if (!registerResult.Succeeded)
                    {
                        foreach (var registerResultError in registerResult.Errors)
                        {
                            ModelState.TryAddModelError(registerResultError.Code, registerResultError.Description);
                        }

                        return Page();
                    }
                }

                var signInResult = await _signInManager.PasswordSignInAsync(player, NewUserData.Password, true, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToPage("Index");
                }
                else
                {
                    return Forbid();
                }
            }

            return Page();
        }
    }
}
