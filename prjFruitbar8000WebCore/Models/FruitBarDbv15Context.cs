using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace prjFruitbar8000WebCore.Models;

public partial class FruitBarDbv15Context : DbContext
{
    public FruitBarDbv15Context()
    {
    }

    public FruitBarDbv15Context(DbContextOptions<FruitBarDbv15Context> options)
        : base(options)
    {
    }

    public virtual DbSet<TAlbum> TAlbums { get; set; }

    public virtual DbSet<TAlbumArtist> TAlbumArtists { get; set; }

    public virtual DbSet<TArtist> TArtists { get; set; }

    public virtual DbSet<TArtistsSong> TArtistsSongs { get; set; }

    public virtual DbSet<TGenre> TGenres { get; set; }

    public virtual DbSet<TSong> TSongs { get; set; }

    public virtual DbSet<TSongGenre> TSongGenres { get; set; }

    public virtual DbSet<TSongsAlbum> TSongsAlbums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=FruitBarDBv15;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TAlbum>(entity =>
        {
            entity.HasKey(e => e.FAlbumId);

            entity.ToTable("tAlbums", "Fruitbar");

            entity.Property(e => e.FAlbumId).HasColumnName("fAlbumId");
            entity.Property(e => e.FAlbumName)
                .HasMaxLength(200)
                .HasColumnName("fAlbumName");
            entity.Property(e => e.FAlbumType)
                .HasMaxLength(50)
                .HasDefaultValue("Album", "DF_tAlbums_fAlbumType")
                .HasColumnName("fAlbumType");
            entity.Property(e => e.FCoverPic).HasColumnName("fCoverPic");
            entity.Property(e => e.FIsDeleted).HasColumnName("fIsDeleted");
            entity.Property(e => e.FReleaseDate).HasColumnName("fReleaseDate");
        });

        modelBuilder.Entity<TAlbumArtist>(entity =>
        {
            entity.HasKey(e => e.FId);

            entity.ToTable("tAlbumArtist", "Fruitbar");

            entity.HasIndex(e => new { e.FAlbumId, e.FArtistId }, "UQ_tAlbumArtist_fAlbumId_fArtistId").IsUnique();

            entity.Property(e => e.FId).HasColumnName("fId");
            entity.Property(e => e.FAlbumId).HasColumnName("fAlbumId");
            entity.Property(e => e.FArtistId).HasColumnName("fArtistId");
            entity.Property(e => e.FCreditRoles)
                .HasMaxLength(200)
                .HasColumnName("fCreditRoles");

            entity.HasOne(d => d.FAlbum).WithMany(p => p.TAlbumArtists)
                .HasForeignKey(d => d.FAlbumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tAlbumArtist_tAlbums");

            entity.HasOne(d => d.FArtist).WithMany(p => p.TAlbumArtists)
                .HasForeignKey(d => d.FArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tAlbumArtist_tArtists");
        });

        modelBuilder.Entity<TArtist>(entity =>
        {
            entity.HasKey(e => e.FArtistId);

            entity.ToTable("tArtists", "Fruitbar");

            entity.Property(e => e.FArtistId).HasColumnName("fArtistId");
            entity.Property(e => e.FArtistName)
                .HasMaxLength(200)
                .HasColumnName("fArtistName");
            entity.Property(e => e.FArtistType)
                .HasMaxLength(50)
                .HasColumnName("fArtistType");
            entity.Property(e => e.FIsDeleted).HasColumnName("fIsDeleted");
        });

        modelBuilder.Entity<TArtistsSong>(entity =>
        {
            entity.HasKey(e => e.FId);

            entity.ToTable("tArtistsSongs", "Fruitbar");

            entity.HasIndex(e => new { e.FSongId, e.FArtistId }, "UQ_tArtistsSongs_fSongId_fArtistId").IsUnique();

            entity.Property(e => e.FId).HasColumnName("fId");
            entity.Property(e => e.FArtistId).HasColumnName("fArtistId");
            entity.Property(e => e.FCreditRoles)
                .HasMaxLength(200)
                .HasColumnName("fCreditRoles");
            entity.Property(e => e.FSongId).HasColumnName("fSongId");

            entity.HasOne(d => d.FArtist).WithMany(p => p.TArtistsSongs)
                .HasForeignKey(d => d.FArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tArtistsSongs_tArtists");

            entity.HasOne(d => d.FSong).WithMany(p => p.TArtistsSongs)
                .HasForeignKey(d => d.FSongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tArtistsSongs_tSongs");
        });

        modelBuilder.Entity<TGenre>(entity =>
        {
            entity.HasKey(e => e.FGenreId);

            entity.ToTable("tGenre", "Fruitbar");

            entity.HasIndex(e => e.FGenreName, "UQ_tGenre_fGenreName").IsUnique();

            entity.Property(e => e.FGenreId).HasColumnName("fGenreId");
            entity.Property(e => e.FGenreName)
                .HasMaxLength(50)
                .HasColumnName("fGenreName");
            entity.Property(e => e.FIsDeleted).HasColumnName("fIsDeleted");
        });

        modelBuilder.Entity<TSong>(entity =>
        {
            entity.HasKey(e => e.FSongId);

            entity.ToTable("tSongs", "Fruitbar");

            entity.Property(e => e.FSongId).HasColumnName("fSongId");
            entity.Property(e => e.FDuration)
                .HasComment("Unit: second")
                .HasColumnName("fDuration");
            entity.Property(e => e.FIsDeleted).HasColumnName("fIsDeleted");
            entity.Property(e => e.FLyrics).HasColumnName("fLyrics");
            entity.Property(e => e.FSongName)
                .HasMaxLength(200)
                .HasColumnName("fSongName");
        });

        modelBuilder.Entity<TSongGenre>(entity =>
        {
            entity.HasKey(e => e.FId);

            entity.ToTable("tSongGenres", "Fruitbar");

            entity.HasIndex(e => new { e.FSongId, e.FGenreId }, "UQ_tSongGenres_fSongId_fGenreId").IsUnique();

            entity.Property(e => e.FId).HasColumnName("fId");
            entity.Property(e => e.FGenreId).HasColumnName("fGenreId");
            entity.Property(e => e.FSongId).HasColumnName("fSongId");

            entity.HasOne(d => d.FGenre).WithMany(p => p.TSongGenres)
                .HasForeignKey(d => d.FGenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tSongGenres_tGenre");

            entity.HasOne(d => d.FSong).WithMany(p => p.TSongGenres)
                .HasForeignKey(d => d.FSongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tSongGenres_tSongs");
        });

        modelBuilder.Entity<TSongsAlbum>(entity =>
        {
            entity.HasKey(e => e.FId);

            entity.ToTable("tSongsAlbums", "Fruitbar");

            entity.HasIndex(e => new { e.FAlbumId, e.FSongId }, "UQ_tSongsAlbums_fAlbumId_fSongId").IsUnique();

            entity.HasIndex(e => new { e.FAlbumId, e.FTrackNumber }, "UQ_tSongsAlbums_fAlbumId_fTrackNumber").IsUnique();

            entity.Property(e => e.FId).HasColumnName("fId");
            entity.Property(e => e.FAlbumId).HasColumnName("fAlbumId");
            entity.Property(e => e.FSongId).HasColumnName("fSongId");
            entity.Property(e => e.FTrackNumber).HasColumnName("fTrackNumber");

            entity.HasOne(d => d.FAlbum).WithMany(p => p.TSongsAlbums)
                .HasForeignKey(d => d.FAlbumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tSongsAlbums_tAlbums");

            entity.HasOne(d => d.FSong).WithMany(p => p.TSongsAlbums)
                .HasForeignKey(d => d.FSongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tSongsAlbums_tSongs");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
