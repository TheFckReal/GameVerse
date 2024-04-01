using System;
using System.Collections.Generic;

namespace GameVerse_recommendation.Models;

public partial class Game
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Genre { get; set; } = null!;

    public bool IsMultiplayer { get; set; }

    public int GameStudioId { get; set; }

    public int PublisherId { get; set; }

    public short? Rating { get; set; }

    public string? ImageUrl { get; set; }

    public virtual GameStudio GameStudio { get; set; } = null!;

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual ICollection<Player> IdPlayers { get; set; } = new List<Player>();
}
