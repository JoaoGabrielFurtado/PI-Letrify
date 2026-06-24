using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.Usuario;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Models.Chat;
using System;
using System.Collections.Generic;

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
    public virtual DbSet<Avaliaco> Avaliacoes { get; set; }
    public virtual DbSet<Favorito> Favoritos { get; set; }
    public virtual DbSet<Seguidor> Seguidores { get; set; }
    public DbSet<MensagemChat> MensagensChat { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<Grupo> Grupos { get; set; }
    public DbSet<UsuarioGrupo> UsuarioGrupos { get; set; }
    public DbSet<SolicitacaoGrupo> SolicitacoesGrupo { get; set; }
    public DbSet<PostGrupo> PostsGrupo { get; set; }
    public DbSet<MensagemGrupo> MensagensGrupo { get; set; }
    public DbSet<CurtidaChat> CurtidasChat { get; set; }
    public DbSet<Conversa> Conversas { get; set; }
    public DbSet<MensagemDireta> MensagensDiretas { get; set; }
    public DbSet<MetaLeitura> MetasLeitura { get; set; }
    public DbSet<CheckInLeitura> CheckInsLeitura { get; set; }
    public DbSet<StreakLeitura> StreaksLeitura { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Azure");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.Temas).HasColumnName("temas");
        });

        modelBuilder.Entity<SituacaoLivro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("situacao_livros_pkey");

            entity.ToTable("situacao_livros");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.DataAtualizacao)
                .HasDefaultValueSql("GETDATE()") 
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
            entity.Property(e => e.Premium)
                .HasColumnName("premium")
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<Avaliaco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_avaliacoes"); // ou o nome da sua PK no banco

            entity.ToTable("avaliacoes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.LivroId).HasColumnName("livro_id");
            entity.Property(e => e.Nota).HasColumnName("nota");
            entity.Property(e => e.Resenha).HasColumnName("resenha");
            entity.Property(e => e.DataAvaliacao)
                .HasColumnType("datetime")
                .HasColumnName("data_avaliacao");

            // Relação com Livro
            entity.HasOne(d => d.Livro)
                .WithMany() // Se você criou a lista no Livro.cs, pode colocar p => p.Avaliacoes aqui
                .HasForeignKey(d => d.LivroId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Livro_Avaliacao");

            // Relação com Usuario
            entity.HasOne(d => d.Usuario)
                .WithMany() // Se você criou a lista no Usuario.cs, pode colocar p => p.Avaliacoes aqui
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Usuario_Avaliacao");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_favoritos");

            entity.ToTable("favoritos");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.LivroId).HasColumnName("livro_id");
            entity.Property(e => e.DataFavoritado)
                .HasColumnType("datetime")
                .HasColumnName("data_favoritado");

            // Relação com Livro
            entity.HasOne(d => d.Livro)
                .WithMany()
                .HasForeignKey(d => d.LivroId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Fav_Livro");

            // Relação com Usuario
            entity.HasOne(d => d.Usuario)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Fav_Usuario");
        });

        modelBuilder.Entity<Seguidor>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.SeguidorUsuario)
                  .WithMany()
                  .HasForeignKey(s => s.SeguidorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.SeguidoUsuario)
                  .WithMany()
                  .HasForeignKey(s => s.SeguidoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MensagemChat>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Conteudo)
                  .IsRequired()
                  .HasMaxLength(750); 

            entity.Property(m => m.ResenhaIsbn)
                  .HasMaxLength(20);

            entity.Property(m => m.ResenhaTituloLivro)
                  .HasMaxLength(300);

            entity.Property(m => m.ResenhaAutorLivro)
                  .HasMaxLength(300);

            entity.Property(m => m.ResenhaCapaUrl)
                  .HasMaxLength(500);

            entity.Property(m => m.ResenhaNotaLivro);

            entity.HasOne(m => m.Usuario)
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.MensagemPai)
                  .WithMany(m => m.Respostas)
                  .HasForeignKey(m => m.MensagemPaiId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Grupo)
                  .WithMany()
                  .HasForeignKey(m => m.GrupoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.HasKey(n => n.Id);

            entity.ToTable("Notificacoes");

            entity.Property(n => n.Tipo)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(n => n.Conteudo)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(n => n.Lida)
                  .HasDefaultValue(false);

            entity.Property(n => n.DataCriacao)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(n => n.Usuario)
                  .WithMany()
                  .HasForeignKey(n => n.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Notificacoes_Usuarios");
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.ToTable("Grupos");

            entity.Property(g => g.Nome).IsRequired().HasMaxLength(100);
            entity.Property(g => g.Descricao).HasMaxLength(500);
            entity.Property(g => g.Status).IsRequired().HasMaxLength(10);
            entity.Property(g => g.FotoCapa).HasMaxLength(255);
            entity.Property(g => g.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(g => g.Lider)
                  .WithMany()
                  .HasForeignKey(g => g.LiderId)
                  .OnDelete(DeleteBehavior.NoAction)
                  .HasConstraintName("FK_Grupos_Lider");
        });

        modelBuilder.Entity<UsuarioGrupo>(entity =>
        {
            entity.HasKey(ug => ug.Id);
            entity.ToTable("UsuarioGrupo");

            entity.HasIndex(ug => new { ug.UsuarioId, ug.GrupoId }).IsUnique();

            entity.Property(ug => ug.Role).IsRequired().HasMaxLength(15);
            entity.Property(ug => ug.DataEntrada).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(ug => ug.Usuario)
                  .WithMany()
                  .HasForeignKey(ug => ug.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ug => ug.Grupo)
                  .WithMany(g => g.Membros)
                  .HasForeignKey(ug => ug.GrupoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitacaoGrupo>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.ToTable("SolicitacaoGrupo");

            entity.HasIndex(s => new { s.UsuarioId, s.GrupoId }).IsUnique();

            entity.Property(s => s.Status).IsRequired().HasMaxLength(10);
            entity.Property(s => s.DataSolicitacao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Grupo)
                  .WithMany()
                  .HasForeignKey(s => s.GrupoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PostGrupo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.ToTable("PostGrupo");

            entity.Property(p => p.Conteudo).IsRequired().HasMaxLength(500);
            entity.Property(p => p.DataPostagem).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(p => p.Grupo)
                  .WithMany()
                  .HasForeignKey(p => p.GrupoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Usuario)
                  .WithMany()
                  .HasForeignKey(p => p.UsuarioId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.PostPai)
                  .WithMany(p => p.Respostas)
                  .HasForeignKey(p => p.PostPaiId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MensagemGrupo>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.ToTable("MensagemGrupo");

            entity.Property(m => m.Conteudo).IsRequired().HasMaxLength(300);
            entity.Property(m => m.DataPostagem).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(m => m.Grupo)
                  .WithMany()
                  .HasForeignKey(m => m.GrupoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Usuario)
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CurtidaChat>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("CurtidaChat");

            entity.HasIndex(c => new { c.UsuarioId, c.MensagemId }).IsUnique();

            entity.Property(c => c.DataCurtida).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(c => c.Usuario)
                  .WithMany()
                  .HasForeignKey(c => c.UsuarioId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(c => c.Mensagem)
                  .WithMany(m => m.Curtidas)
                  .HasForeignKey(c => c.MensagemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversa>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("Conversas");
            entity.HasIndex(c => new { c.Usuario1Id, c.Usuario2Id }).IsUnique();
            entity.Property(c => c.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(c => c.Usuario1)
                  .WithMany()
                  .HasForeignKey(c => c.Usuario1Id)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(c => c.Usuario2)
                  .WithMany()
                  .HasForeignKey(c => c.Usuario2Id)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MensagemDireta>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.ToTable("MensagensDiretas");
            entity.Property(m => m.Conteudo).IsRequired().HasMaxLength(1000);
            entity.Property(m => m.DataEnvio).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(m => m.Conversa)
                  .WithMany(c => c.Mensagens)
                  .HasForeignKey(m => m.ConversaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Remetente)
                  .WithMany()
                  .HasForeignKey(m => m.RemetenteId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MetaLeitura>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.ToTable("MetasLeitura");
            entity.Property(m => m.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(m => m.Periodicidade).IsRequired().HasMaxLength(10);
            entity.Property(m => m.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(m => m.Usuario)
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CheckInLeitura>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("CheckInsLeitura");
            entity.HasIndex(c => new { c.MetaId, c.Data }).IsUnique();
            entity.Property(c => c.DataCriacao).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(c => c.Data).HasColumnType("date");

            entity.HasOne(c => c.Meta)
                  .WithMany(m => m.CheckIns)
                  .HasForeignKey(c => c.MetaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Usuario)
                  .WithMany()
                  .HasForeignKey(c => c.UsuarioId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<StreakLeitura>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.ToTable("StreaksLeitura");
            entity.HasIndex(s => s.UsuarioId).IsUnique();
            entity.Property(s => s.UltimoCheckIn).HasColumnType("date");

            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
