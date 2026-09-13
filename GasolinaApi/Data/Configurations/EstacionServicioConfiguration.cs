using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class EstacionServicioConfiguration : EntidadBaseConfiguration<EstacionServicio>
{
    public override void Configure(EntityTypeBuilder<EstacionServicio> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_ESTACION_SERVICIO");

        builder.Property(e => e.MunicipioId).HasColumnName("MUNICIPIO_ID");
        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Marca).HasColumnName("MARCA").HasMaxLength(50);
        builder.Property(e => e.Latitud).HasColumnName("LATITUD").HasColumnType("decimal(9,6)");
        builder.Property(e => e.Longitud).HasColumnName("LONGITUD").HasColumnType("decimal(9,6)");
        builder.Property(e => e.Referencia).HasColumnName("REFERENCIA").HasMaxLength(255);

        builder.HasOne(e => e.Municipio).WithMany()
            .HasForeignKey(e => e.MunicipioId).HasConstraintName("FK_ESTACION_MUNICIPIO");

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_ESTACION_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_ESTACION_USR_MOD");

        builder.HasIndex(e => e.MunicipioId).HasDatabaseName("IX_ESTACION_MUNICIPIO");
    }
}
