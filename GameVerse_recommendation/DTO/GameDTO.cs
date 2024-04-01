using System.ComponentModel.DataAnnotations;
using GameVerse_recommendation.Models;

namespace GameVerse_recommendation.DTO
{
    public class GameDTO()
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Genre { get; set; } = "";
        public bool Multiplayer { get; set; } = false;
        public string GameStudio { get; set; } = "";
        public string Publisher { get; set;} = "";
        public short Rating { get; set; } = 0;
        public string ImageURL { get; set; } = "";
        public bool IsLikes { get; set; } = false;
    }
}
