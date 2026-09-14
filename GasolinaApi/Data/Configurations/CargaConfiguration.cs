using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class CargaConfiguration : EntidadBaseConfiguration<Carga>
{
    public override void Configure(EntityTypeBuilder<Carga> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_CARGA");

        builder.Property(e => e.VehiculoId).HasColumnName("VEHICULO_ID").IsRequired();
        builder.Property(e => e.TipoCombustibleId).HasColumnName("TIPO_COMBUSTIBLE_ID").IsRequired();
        builder.Property(e => e.EstacionServicioId).HasColumnName("ESTACION_SERVICIO_ID");
        builder.Property(e => e.Fecha).HasColumnName("FECHA").IsRequired();
        builder.Property(e => e.Kilometraje).HasColumnName("KILOMETRAJE").HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(e => e.KilometrosRecorridos).HasColumnName("KILOMETROS_RECORRIDOS").HasColumnType("decimal(10,2)");
        builder.Property(e => e.Galones).HasColumnName("GALONES").HasColumnType("decimal(6,2)").IsRequired();
        builder.Property(e => e.CostoTotal).HasColumnName("COSTO_TOTAL").HasColumnType("decimal(8,2)").IsRequired();

        // Columnas calculadas PERSISTED en BD: nunca se escriben desde el código, solo se leen.
        builder.Property(e => e.PrecioPorGalon).HasColumnName("PRECIO_POR_GALON").HasColumnType("decimal(38,6)")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(e => e.Litros).HasColumnName("LITROS").HasColumnType("decimal(38,10)")
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(e => e.PrecioPorLitro).HasColumnName("PRECIO_POR_LITRO").HasColumnType("decimal(38,6)")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(e => e.Vehiculo).WithMany()
            .HasForeignKey(e => e.VehiculoId).HasConstraintName("FK_CARGA_VEHICULO");
        builder.HasOne(e => e.TipoCombustible).WithMany()
            .HasForeignKey(e => e.TipoCombustibleId).HasConstraintName("FK_CARGA_COMBUSTIBLE");
        builder.HasOne(e => e.EstacionServicio).WithMany()
            .HasForeignKey(e => e.EstacionServicioId).HasConstraintName("FK_CARGA_ESTACION");

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_CARGA_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_CARGA_USR_MOD");

        builder.HasIndex(e => e.VehiculoId).HasDatabaseName("IX_CARGA_VEHICULO");
        builder.HasIndex(e => e.TipoCombustibleId).HasDatabaseName("IX_CARGA_COMBUSTIBLE");
        builder.HasIndex(e => e.EstacionServicioId).HasDatabaseName("IX_CARGA_ESTACION");
        builder.HasIndex(e => e.Fecha).HasDatabaseName("IX_CARGA_FECHA");
        builder.HasIndex(e => e.Id).HasDatabaseName("IX_CARGA_ACTIVAS").HasFilter("[ESTADO] = 1");
    }
}
