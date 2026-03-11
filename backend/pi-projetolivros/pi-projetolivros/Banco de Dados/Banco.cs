using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;

namespace pi_projetolivros_banco;

// dotnet ef dbcontext scaffold Name=ConnectionStrings:ConexaoRender Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c Banco -f
public partial class Banco : DbContext
{
    public Banco()
    {
    }

    public Banco(DbContextOptions<Banco> options)
        : base(options)
    {
    }

    public virtual DbSet<Livro> Livros { get; set; }

    public virtual DbSet<SituacaoLivro> SituacaoLivros { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:ConexaoSupabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_stat_statements");

        modelBuilder.Entity<Livro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("livros_pkey");

            entity.ToTable("livros");

            entity.HasIndex(e => e.Isbn, "livros_isbn_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Autor)
                .HasMaxLength(255)
                .HasColumnName("autor");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("isbn");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<SituacaoLivro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("situacao_livros_pkey");

            entity.ToTable("situacao_livros");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DataAtualizacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("data_atualizacao");
            entity.Property(e => e.LivroId).HasColumnName("livro_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Livro).WithMany(p => p.SituacaoLivros)
                .HasForeignKey(d => d.LivroId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("situacao_livros_livro_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.SituacaoLivros)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("situacao_livros_usuario_id_fkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Email, "usuarios_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .HasColumnName("senha");
            entity.Property(e => e.Idade)
                .HasColumnName("idade");

            entity.Property(e => e.Cidade)
                .HasMaxLength(100)
                .HasColumnName("cidade");

            entity.Property(e => e.Descricao)
                .HasColumnName("descricao");

            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(255)
                .HasColumnName("foto_perfil");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
