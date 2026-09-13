using GasolinaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GasolinaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoVehiculo> TiposVehiculo => Set<TipoVehiculo>();
    public DbSet<TipoCombustible> TiposCombustible => Set<TipoCombustible>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<EstacionServicio> EstacionesServicio => Set<EstacionServicio>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Carga> Cargas => Set<Carga>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
