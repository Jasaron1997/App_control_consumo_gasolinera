using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class UsuarioConfiguration : EntidadBaseConfiguration<Usuario>
{
    public override void Configure(EntityTypeBuilder<Usuario> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_USUARIO");

        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(150).IsRequired();
        builder.Property(e => e.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(255).IsRequired();
        builder.Property(e => e.TokenVersion).HasColumnName("TOKEN_VERSION").IsRequired();

        builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_USUARIO_EMAIL");
    }
}
