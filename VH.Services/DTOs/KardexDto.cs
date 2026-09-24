namespace VH.Services.DTOs
{
    /// <summary>
    /// Lo que necesita la pantalla del kardex: una página de movimientos y los tres
    /// totales del encabezado.
    ///
    /// Los totales van aparte a propósito. Antes la vista los deducía de los
    /// renglones que tenía a la mano, lo cual funcionaba mientras se trajeran todos;
    /// al paginar, "existencia actual" pasaría a ser la existencia al final de la
    /// página que uno esté mirando, y las entradas y salidas sólo las de esa página.
    /// Tres números que parecen correctos y no lo son.
    /// </summary>
    public class KardexDto
    {
        public ResultadoPaginado<MovimientoInventarioResponseDto> Movimientos { get; set; } = new();

        /// <summary>
        /// La existencia que hay hoy en ese almacén, tomada del inventario y no del
        /// último renglón visible.
        /// </summary>
        public decimal ExistenciaActual { get; set; }

        /// <summary>Todo lo que entró en el periodo consultado, no sólo en esta página.</summary>
        public decimal Entradas { get; set; }

        /// <summary>Todo lo que salió en el periodo, en positivo.</summary>
        public decimal Salidas { get; set; }

        public string NombreMaterial { get; set; } = string.Empty;
        public string NombreAlmacen { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        /// <summary>
        /// Cuánto de la existencia está apartado para requisiciones. La pantalla lo
        /// necesita para explicar por qué no todo lo que hay se puede mover.
        /// </summary>
        public decimal Apartado { get; set; }

        public decimal Disponible => ExistenciaActual - Apartado;

        public static KardexDto Vacio(ConsultaPaginada? consulta = null) => new()
        {
            Movimientos = ResultadoPaginado<MovimientoInventarioResponseDto>.Ninguno(consulta)
        };
    }
}
