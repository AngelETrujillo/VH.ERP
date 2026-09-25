namespace VH.Services.DTOs
{
    /// <summary>
    /// Una página del control de inventarios y los tres conteos del encabezado.
    ///
    /// Como en el kardex, los conteos van aparte porque no se pueden deducir de la
    /// página: "3 materiales sin stock" tiene que ser de todo el inventario, no de
    /// los veinticinco que se alcanzan a ver.
    /// </summary>
    public class InventarioListadoDto
    {
        public ResultadoPaginado<InventarioResponseDto> Renglones { get; set; } = new();

        /// <summary>Existencia en cero: no hay nada que entregar.</summary>
        public int SinStock { get; set; }

        /// <summary>Por debajo del mínimo, pero todavía queda algo.</summary>
        public int StockBajo { get; set; }

        /// <summary>Por encima del máximo definido: dinero parado en el anaquel.</summary>
        public int SobreMaximo { get; set; }

        /// <summary>
        /// Registros de inventario sin mínimo definido. Sin mínimo nadie los vigila,
        /// así que por vacío que esté el anaquel nunca aparecen en la reposición.
        /// </summary>
        public int SinMinimo { get; set; }

        public static InventarioListadoDto Vacio(ConsultaPaginada? consulta = null) => new()
        {
            Renglones = ResultadoPaginado<InventarioResponseDto>.Ninguno(consulta)
        };
    }
}
