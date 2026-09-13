using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class MunicipioConfiguration : EntidadBaseConfiguration<Municipio>
{
    public override void Configure(EntityTypeBuilder<Municipio> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_MUNICIPIO");

        builder.Property(e => e.DepartamentoId).HasColumnName("DEPARTAMENTO_ID").IsRequired();
        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();

        builder.HasOne(e => e.Departamento).WithMany(d => d.Municipios)
            .HasForeignKey(e => e.DepartamentoId).HasConstraintName("FK_MUNI_DEPARTAMENTO");

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_MUNI_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_MUNI_USR_MOD");

        builder.HasIndex(e => e.DepartamentoId).HasDatabaseName("IX_MUNICIPIO_DEPARTAMENTO");
    }
}
