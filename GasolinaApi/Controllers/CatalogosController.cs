using GasolinaApi.Auth;
using GasolinaApi.DTOs;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Filters;
using GasolinaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GasolinaApi.Controllers;

[ApiController]
[Authorize]
[Route("api/catalogos")]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CatalogosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [SinAuditoria]
    [HttpGet("tipos-vehiculo")]
    public async Task<ActionResult<RespuestaApi<List<CatalogoResponse>>>> ObtenerTiposVehiculo()
    {
        var datos = await _catalogoService.ObtenerTiposVehiculoAsync();
        return Ok(RespuestaApi<List<CatalogoResponse>>.Ok(datos));
    }

    [SinAuditoria]
    [HttpGet("tipos-combustible")]
    public async Task<ActionResult<RespuestaApi<List<CatalogoResponse>>>> ObtenerTiposCombustible()
    {
        var datos = await _catalogoService.ObtenerTiposCombustibleAsync();
        return Ok(RespuestaApi<List<CatalogoResponse>>.Ok(datos));
    }

    [SinAuditoria]
    [HttpGet("departamentos")]
    public async Task<ActionResult<RespuestaApi<List<CatalogoResponse>>>> ObtenerDepartamentos()
    {
        var datos = await _catalogoService.ObtenerDepartamentosAsync();
        return Ok(RespuestaApi<List<CatalogoResponse>>.Ok(datos));
    }

    [SinAuditoria]
    [HttpGet("departamentos/{id:int}/municipios")]
    public async Task<ActionResult<RespuestaApi<List<MunicipioResponse>>>> ObtenerMunicipios(int id)
    {
        var datos = await _catalogoService.ObtenerMunicipiosPorDepartamentoAsync(id);
        return Ok(RespuestaApi<List<MunicipioResponse>>.Ok(datos));
    }

    [SinAuditoria]
    [HttpGet("estaciones-servicio")]
    public async Task<ActionResult<RespuestaApi<List<EstacionServicioResponse>>>> BuscarEstacionesServicio([FromQuery] string? buscar)
    {
        var datos = await _catalogoService.BuscarEstacionesServicioAsync(buscar);
        return Ok(RespuestaApi<List<EstacionServicioResponse>>.Ok(datos));
    }

    [HttpPost("estaciones-servicio")]
    public async Task<ActionResult<RespuestaApi<EstacionServicioResponse>>> CrearEstacionServicio(EstacionServicioRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var estacion = await _catalogoService.CrearEstacionServicioAsync(request, usuarioId);
        return Ok(RespuestaApi<EstacionServicioResponse>.Ok(estacion));
    }
}
