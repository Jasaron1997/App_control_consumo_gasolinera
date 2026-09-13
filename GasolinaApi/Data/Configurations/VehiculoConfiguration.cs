using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class VehiculoConfiguration : EntidadBaseConfiguration<Vehiculo>
{
    public override void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_VEHICULO");

        builder.Property(e => e.TipoVehiculoId).HasColumnName("TIPO_VEHICULO_ID").IsRequired();
        builder.Property(e => e.TipoCombustibleId).HasColumnName("TIPO_COMBUSTIBLE_ID").IsRequired();
        builder.Property(e => e.UsuarioPropietarioId).HasColumnName("USUARIO_PROPIETARIO_ID").IsRequired();
        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Placa).HasColumnName("PLACA").HasMaxLength(20);
        builder.Property(e => e.TanqueCapacidadGalones).HasColumnName("TANQUE_CAPACIDAD_GALONES").HasColumnType("decimal(5,2)");

        builder.HasOne(e => e.TipoVehiculo).WithMany()
            .HasForeignKey(e => e.TipoVehiculoId).HasConstraintName("FK_VEHICULO_TIPO");
        builder.HasOne(e => e.TipoCombustible).WithMany()
            .HasForeignKey(e => e.TipoCombustibleId).HasConstraintName("FK_VEHICULO_COMBUSTIBLE");
        builder.HasOne(e => e.UsuarioPropietario).WithMany()
            .HasForeignKey(e => e.UsuarioPropietarioId).HasConstraintName("FK_VEHICULO_PROPIETARIO");

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_VEHICULO_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_VEHICULO_USR_MOD");

        builder.HasIndex(e => e.TipoVehiculoId).HasDatabaseName("IX_VEHICULO_TIPO");
        builder.HasIndex(e => e.TipoCombustibleId).HasDatabaseName("IX_VEHICULO_COMBUSTIBLE");
        builder.HasIndex(e => e.UsuarioPropietarioId).HasDatabaseName("IX_VEHICULO_PROPIETARIO");

        builder.HasIndex(e => e.Placa)
            .IsUnique()
            .HasDatabaseName("UQ_VEHICULO_PLACA_ACTIVA")
            .HasFilter("[ESTADO] = 1 AND [PLACA] IS NOT NULL");
    }
}
