using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class DepartamentoConfiguration : EntidadBaseConfiguration<Departamento>
{
    public override void Configure(EntityTypeBuilder<Departamento> builder)
    {
        base.Configure(builder);

        builder.ToTable("TB_DEPARTAMENTO");

        builder.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(50).IsRequired();

        builder.HasMany(e => e.Municipios).WithOne(m => m.Departamento).HasForeignKey(m => m.DepartamentoId);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioCreacion).HasConstraintName("FK_DEPTO_USR_CREA");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioModificacion).HasConstraintName("FK_DEPTO_USR_MOD");
    }
}
