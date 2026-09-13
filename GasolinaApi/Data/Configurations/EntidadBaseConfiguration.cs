using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public abstract class EntidadBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : EntidadBase
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.Id).HasColumnName("ID");
        builder.Property(e => e.UsuarioCreacion).HasColumnName("USUARIO_CREACION");
        builder.Property(e => e.FechaCreacion).HasColumnName("FECHA_CREACION");
        builder.Property(e => e.UsuarioModificacion).HasColumnName("USUARIO_MODIFICACION");
        builder.Property(e => e.FechaModificacion).HasColumnName("FECHA_MODIFICACION");
        builder.Property(e => e.Estado).HasColumnName("ESTADO");
        builder.Property(e => e.FechaEliminacion).HasColumnName("FECHA_ELIMINACION");

        builder.HasIndex(e => e.Estado).HasFilter("[ESTADO] = 1");
    }
}
