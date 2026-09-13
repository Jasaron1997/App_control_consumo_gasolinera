using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GasolinaApi.Data.Configurations;

public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("TB_LOG_AUDITORIA");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("ID");
        builder.Property(e => e.UsuarioId).HasColumnName("USUARIO_ID");
        builder.Property(e => e.Fecha).HasColumnName("FECHA").IsRequired();
        builder.Property(e => e.TipoAccion).HasColumnName("TIPO_ACCION").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Entidad).HasColumnName("ENTIDAD").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IdRegistro).HasColumnName("ID_REGISTRO").HasMaxLength(50);
        builder.Property(e => e.Parametros).HasColumnName("PARAMETROS");
        builder.Property(e => e.Controlador).HasColumnName("CONTROLADOR").HasMaxLength(100);
        builder.Property(e => e.AccionMetodo).HasColumnName("ACCION_METODO").HasMaxLength(100);
        builder.Property(e => e.VistaOrigen).HasColumnName("VISTA_ORIGEN").HasMaxLength(100);
        builder.Property(e => e.Exitoso).HasColumnName("EXITOSO").IsRequired();
        builder.Property(e => e.MensajeError).HasColumnName("MENSAJE_ERROR").HasMaxLength(500);

        builder.HasOne(e => e.Usuario).WithMany()
            .HasForeignKey(e => e.UsuarioId).HasConstraintName("FK_LOGAUD_USUARIO");

        builder.HasIndex(e => e.Fecha).HasDatabaseName("IX_LOGAUD_FECHA");
        builder.HasIndex(e => new { e.UsuarioId, e.Fecha }).HasDatabaseName("IX_LOGAUD_USUARIO");
        builder.HasIndex(e => new { e.Entidad, e.IdRegistro }).HasDatabaseName("IX_LOGAUD_ENTIDAD");
    }
}
