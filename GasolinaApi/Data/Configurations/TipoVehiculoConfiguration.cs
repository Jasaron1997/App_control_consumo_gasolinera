using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class TipoVehiculoConfiguration : EntidadBaseConfiguration<TipoVehiculo>
{
    public override void Configure(EntityTypeBuilder<TipoVehiculo> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_TIPO_VEHICULO");

        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(50).IsRequired();

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_TIPOVEH_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_TIPOVEH_USR_MOD");
    }
}
