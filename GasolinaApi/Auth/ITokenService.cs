using GasolinaApi.Models;

namespace GasolinaApi.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
