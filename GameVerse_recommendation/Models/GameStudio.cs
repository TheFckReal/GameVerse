using System;
using System.Collections.Generic;

namespace GameVerse_recommendation.Models;

public partial class GameStudio
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
