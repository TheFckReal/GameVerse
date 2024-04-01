using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameVerse_recommendation.Models;

public partial class VideogameStoreContext : IdentityDbContext<Player>
{
    public VideogameStoreContext()
    {
    }

    public VideogameStoreContext(DbContextOptions<VideogameStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameStudio> GameStudios { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("games_pkey");

            entity.ToTable("games");

            entity.HasIndex(e => e.Name, "games_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameStudioId).HasColumnName("game_studio_id");
            entity.Property(e => e.Genre)
                .HasColumnType("character varying")
                .HasColumnName("genre");
            entity.Property(e => e.ImageUrl)
                .HasColumnType("character varying")
                .HasColumnName("image_url");
            entity.Property(e => e.IsMultiplayer).HasColumnName("is_multiplayer");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.GameStudio).WithMany(p => p.Games)
                .HasForeignKey(d => d.GameStudioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("games_game_studio_id_fkey");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Games)
                .HasForeignKey(d => d.PublisherId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("games_publisher_id_fkey");
        });

        modelBuilder.Entity<GameStudio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_studio_pkey");

            entity.ToTable("game_studio");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("players_pkey");

            entity.ToTable("players");

            entity.Property(e => e.Id).HasColumnName("id"); ;

            entity.HasMany(d => d.IdGames).WithMany(p => p.IdPlayers)
                .UsingEntity<Dictionary<string, object>>(
                    "GamesPlayer",
                    r => r.HasOne<Game>().WithMany()
                        .HasForeignKey("IdGame")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ref_games"),
                    l => l.HasOne<Player>().WithMany()
                        .HasForeignKey("IdPlayer")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ref_player"),
                    j =>
                    {
                        j.HasKey("IdPlayer", "IdGame").HasName("ids_primary");
                        j.ToTable("games_players");
                        j.IndexerProperty<int>("IdPlayer").HasColumnName("id_player");
                        j.IndexerProperty<int>("IdGame").HasColumnName("id_game");
                    });
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("publisher_pkey");

            entity.ToTable("publisher");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
