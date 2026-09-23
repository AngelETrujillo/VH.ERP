using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Lo que se prestó y no ha vuelto.
    ///
    /// Una herramienta entregada sigue siendo de la empresa: está en manos de
    /// alguien, no consumida. Hasta ahora el sistema no distinguía un par de
    /// guantes de un rotomartillo, así que nadie podía contestar quién tiene qué.
    /// </summary>
    public interface IDevolucionEPPService
    {
        /// <summary>
        /// Material retornable que sigue afuera, por persona. Lo que lleva más
        /// tiempo prestado va primero.
        /// </summary>
        Task<IEnumerable<PrestamoDto>> GetPrestadosAsync(int? idEmpleado = null, int? idAlmacen = null);

        /// <summary>Historial de devoluciones.</summary>
        Task<IEnumerable<DevolucionEPP>> GetDevolucionesAsync(int? idEmpleado = null);

        /// <summary>Una página del historial de devoluciones.</summary>
        Task<ResultadoPaginado<DevolucionEPP>> GetHistorialPaginadoAsync(
            ConsultaPaginada consulta, int? idEmpleado = null);

        /// <summary>
        /// Registra la vuelta de una herramienta.
        ///
        /// Lo que regresa entero o golpeado entra otra vez al almacén y al kardex;
        /// lo perdido no, porque salió del anaquel el día que se entregó y nunca
        /// volvió. En los tres casos queda el renglón que dice qué pasó.
        /// </summary>
        Task<(DevolucionEPP Devolucion, string? Aviso)> RegistrarAsync(
            RegistrarDevolucionRequestDto dto, string userId);
    }
}
