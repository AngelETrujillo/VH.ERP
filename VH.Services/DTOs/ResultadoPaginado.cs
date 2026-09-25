namespace VH.Services.DTOs
{
    /// <summary>
    /// Una página de resultados y lo que hace falta para pintar el paginador:
    /// cuántos hay en total, en cuál página vamos y de qué tamaño.
    ///
    /// El total viene de un COUNT aparte sobre el mismo filtro. Cuesta una consulta
    /// más, pero sin él la pantalla no puede decir "25 de 336" ni saber cuántas
    /// páginas dibujar.
    /// </summary>
    public class ResultadoPaginado<T>
    {
        // Propiedades con set y tipos concretos a propósito: este objeto viaja de la
        // API a la Web como JSON, y las de sólo lectura o las declaradas como
        // interfaz no se rehidratan del otro lado.
        public List<T> Renglones { get; set; } = new();

        /// <summary>Cuántos hay en total con el filtro aplicado, no sólo en esta página.</summary>
        public int Total { get; set; }

        public int Pagina { get; set; } = 1;
        public int Tamano { get; set; } = ConsultaPaginada.TamanoPorOmision;

        /// <summary>El texto que se buscó, para poder decir "sin resultados para X".</summary>
        public string? Buscado { get; set; }

        public int TotalPaginas => Total == 0 ? 1 : (int)Math.Ceiling(Total / (double)Tamano);

        public bool HayAnterior => Pagina > 1;
        public bool HaySiguiente => Pagina < TotalPaginas;

        public bool Vacio => Total == 0;

        /// <summary>Número del primer renglón de esta página, para el "mostrando N a M".</summary>
        public int Desde => Total == 0 ? 0 : ((Pagina - 1) * Tamano) + 1;

        /// <summary>Número del último renglón de esta página.</summary>
        public int Hasta => Math.Min(Pagina * Tamano, Total);

        /// <summary>
        /// Verdadero cuando todo cabe en una página: el paginador entonces sobra
        /// y la pantalla puede ahorrárselo.
        /// </summary>
        public bool CabeEnUna => TotalPaginas <= 1;

        /// <summary>
        /// Las páginas a dibujar alrededor de la actual. Con 200 páginas no se
        /// pintan 200 botones: se pinta una ventana y los extremos.
        /// </summary>
        public IReadOnlyList<int> Ventana(int aLosLados = 2)
        {
            var desde = Math.Max(1, Pagina - aLosLados);
            var hasta = Math.Min(TotalPaginas, Pagina + aLosLados);

            // Si la ventana queda corta por estar en un extremo, se estira del otro
            // lado para que siempre se ofrezca el mismo número de saltos.
            var ancho = (aLosLados * 2) + 1;
            if (hasta - desde + 1 < ancho)
            {
                if (desde == 1) hasta = Math.Min(TotalPaginas, desde + ancho - 1);
                else if (hasta == TotalPaginas) desde = Math.Max(1, hasta - ancho + 1);
            }

            var paginas = new List<int>();
            for (var p = desde; p <= hasta; p++) paginas.Add(p);
            return paginas;
        }

        /// <summary>Vacío, para los catch de los controladores.</summary>
        public static ResultadoPaginado<T> Ninguno(ConsultaPaginada? consulta = null) => new()
        {
            Renglones = new List<T>(),
            Total = 0,
            Pagina = consulta?.Pagina ?? 1,
            Tamano = consulta?.Tamano ?? ConsultaPaginada.TamanoPorOmision,
            Buscado = consulta?.TextoLimpio
        };

        /// <summary>
        /// Convierte los renglones a otro tipo conservando los datos del paginador.
        /// Lo usan los servicios que consultan entidades y devuelven DTOs.
        /// </summary>
        public ResultadoPaginado<TDestino> Convertir<TDestino>(Func<T, TDestino> mapear) => new()
        {
            Renglones = Renglones.Select(mapear).ToList(),
            Total = Total,
            Pagina = Pagina,
            Tamano = Tamano,
            Buscado = Buscado
        };

        /// <summary>Igual que el anterior, cuando el mapeo se hace en bloque (AutoMapper).</summary>
        public ResultadoPaginado<TDestino> ConLos<TDestino>(IEnumerable<TDestino> renglones) => new()
        {
            Renglones = renglones.ToList(),
            Total = Total,
            Pagina = Pagina,
            Tamano = Tamano,
            Buscado = Buscado
        };
    }
}
