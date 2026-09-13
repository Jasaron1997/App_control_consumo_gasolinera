using System.ComponentModel.DataAnnotations;
using GasolinaApi.Auth;
using GasolinaApi.DTOs;
using GasolinaApi.DTOs.Responses;
using GasolinaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GasolinaApi.Controllers;

[ApiController]
[Authorize]
[Route("api/estadisticas")]
public class EstadisticasController : ControllerBase
{
    private readonly IEstadisticaService _estadisticaService;

    public EstadisticasController(IEstadisticaService estadisticaService)
    {
        _estadisticaService = estadisticaService;
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<RespuestaApi<ResumenEstadisticasResponse>>> ObtenerResumen([FromQuery] int? vehiculoId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resumen = await _estadisticaService.ObtenerResumenAsync(usuarioId, vehiculoId);
        return Ok(RespuestaApi<ResumenEstadisticasResponse>.Ok(resumen));
    }

    [HttpGet("historico")]
    public async Task<ActionResult<RespuestaApi<List<HistoricoPuntoResponse>>>> ObtenerHistorico(
        [FromQuery] int? vehiculoId,
        [FromQuery] [Range(1, 120, ErrorMessage = "meses debe estar entre 1 y 120.")] int meses = 6)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var historico = await _estadisticaService.ObtenerHistoricoAsync(usuarioId, vehiculoId, meses);
        return Ok(RespuestaApi<List<HistoricoPuntoResponse>>.Ok(historico));
    }
}
