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
[Route("api/vehiculos")]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculoService _vehiculoService;

    public VehiculosController(IVehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    [HttpGet]
    public async Task<ActionResult<RespuestaApi<List<VehiculoResponse>>>> ObtenerMisVehiculos()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var vehiculos = await _vehiculoService.ObtenerDelUsuarioAsync(usuarioId);
        return Ok(RespuestaApi<List<VehiculoResponse>>.Ok(vehiculos));
    }

    [HttpPost]
    public async Task<ActionResult<RespuestaApi<VehiculoResponse>>> Crear(VehiculoRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var vehiculo = await _vehiculoService.CrearAsync(request, usuarioId);
        return Ok(RespuestaApi<VehiculoResponse>.Ok(vehiculo));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RespuestaApi<VehiculoResponse>>> Actualizar(int id, VehiculoRequest request)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var vehiculo = await _vehiculoService.ActualizarAsync(id, request, usuarioId);
        return Ok(RespuestaApi<VehiculoResponse>.Ok(vehiculo));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<RespuestaApi<object>>> Eliminar(int id)
    {
        var usuarioId = User.ObtenerUsuarioId();
        await _vehiculoService.EliminarAsync(id, usuarioId);
        return Ok(RespuestaApi<object>.Ok(new { }, "Vehículo eliminado correctamente."));
    }
}
