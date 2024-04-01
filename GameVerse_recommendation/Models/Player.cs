using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace GameVerse_recommendation.Models;

public partial class Player : IdentityUser
{
    public virtual ICollection<Game> IdGames { get; set; } = new List<Game>();
}
