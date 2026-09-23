namespace VH.Services.DTOs
{
    /// <summary>
    /// Lo que pide una pantalla de listado: qué página, de qué tamaño, qué texto
    /// buscar y por dónde ordenar.
    ///
    /// Antes cada listado traía la tabla entera y el navegador escondía las filas
    /// que no coincidían con la búsqueda. Eso funciona con dieciséis materiales y
    /// deja de funcionar con dos años de kardex: el problema no es que se vean de
    /// más, es que se descargan de más.
    /// </summary>
    public class ConsultaPaginada
    {
        /// <summary>Tamaño por omisión. Cabe en una pantalla sin scroll infinito.</summary>
        public const int TamanoPorOmision = 25;

        /// <summary>Tope duro: nadie pide mil renglones de una sentada, ni por URL.</summary>
        public const int TamanoMaximo = 200;

        private int _pagina = 1;
        private int _tamano = TamanoPorOmision;

        /// <summary>Página solicitada, empezando en 1.</summary>
        public int Pagina
        {
            get => _pagina;
            set => _pagina = value < 1 ? 1 : value;
        }

        /// <summary>Renglones por página, acotado para que la URL no pueda tumbar el servidor.</summary>
        public int Tamano
        {
            get => _tamano;
            set => _tamano = value switch
            {
                < 1 => TamanoPorOmision,
                > TamanoMaximo => TamanoMaximo,
                _ => value
            };
        }

        /// <summary>Texto libre. Cada servicio decide sobre qué columnas busca.</summary>
        public string? Buscar { get; set; }

        /// <summary>Columna por la que ordenar. Cada servicio traduce el nombre.</summary>
        public string? Orden { get; set; }

        /// <summary>De mayor a menor cuando es verdadero.</summary>
        public bool Descendente { get; set; }

        /// <summary>Cuántos renglones saltar para llegar a esta página.</summary>
        public int Salto => (Pagina - 1) * Tamano;

        /// <summary>El texto de búsqueda ya recortado, o nulo si venía vacío.</summary>
        public string? TextoLimpio =>
            string.IsNullOrWhiteSpace(Buscar) ? null : Buscar.Trim();

        public bool HayBusqueda => TextoLimpio != null;
    }
}
