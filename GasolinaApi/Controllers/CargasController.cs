using GasolinaApi.Auth;
using GasolinaApi.DTOs;
using GasolinaApi.DTOs.Requests;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GasolinaApi.Controllers;

[ApiController]
[Authorize]
[Route("api/cargas")]
public class CargasController : ControllerBase
{
    private readonly ICargaService _cargaService;

    public CargasController(ICargaService cargaService)
    {
        _cargaService = cargaService;
    }

    [HttpGet]
    public async Task<ActionResult<RespuestaApi<List<CargaResponse>>>> Obtener(
        [FromQuery] int? vehiculoId, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var cargas = await _cargaService.ObtenerAsync(usuarioId, vehiculoId, desde, hasta);
        return Ok(RespuestaApi<List<CargaResponse>>.Ok(cargas));
    }

    [HttpPost]
    public async Task<ActionResult<RespuestaApi<CargaResponse>>> Crear(CrearCargaRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var carga = await _cargaService.CrearAsync(request, usuarioId);
        return Ok(RespuestaApi<CargaResponse>.Ok(carga));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RespuestaApi<CargaResponse>>> Actualizar(int id, CrearCargaRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var carga = await _cargaService.ActualizarAsync(id, request, usuarioId);
        return Ok(RespuestaApi<CargaResponse>.Ok(carga));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RespuestaApi<object>>> Eliminar(int id)
    {
        var usuarioId = User.ObtenerUsuarioId();
        await _cargaService.EliminarAsync(id, usuarioId);
        return Ok(RespuestaApi<object>.Ok(new { }, "Carga eliminada correctamente."));
    }
}
