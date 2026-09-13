using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class TipoCombustibleConfiguration : EntidadBaseConfiguration<TipoCombustible>
{
    public override void Configure(EntityTypeBuilder<TipoCombustible> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_TIPO_COMBUSTIBLE");

        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(50).IsRequired();

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_TIPOCOMB_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_TIPOCOMB_USR_MOD");
    }
}
