namespace VH.Web.Helpers
{
    public static class FormatoHelper
    {
        /// <summary>
        /// Formato de cantidad: entero si no tiene decimales, 2 decimales si tiene.
        /// 10.0000 → "10"    |    10.5000 → "10.50"    |    10.2500 → "10.25"
        /// </summary>
        public static string FormatoCantidad(this decimal valor)
        {
            return valor == Math.Truncate(valor)
                ? valor.ToString("N0")
                : valor.FormatoCantidad();
        }

        /// <summary>
        /// Formato de moneda: siempre 2 decimales con símbolo.
        /// 1500 → "$1,500.00"
        /// </summary>
        public static string FormatoMoneda(this decimal valor)
        {
            return valor.ToString("C2");
        }

        /// <summary>
        /// Formato de cantidad nullable.
        /// </summary>
        public static string FormatoCantidad(this decimal? valor)
        {
            return valor.HasValue ? valor.Value.FormatoCantidad() : "0";
        }

        /// <summary>
        /// Formato de moneda nullable.
        /// </summary>
        public static string FormatoMoneda(this decimal? valor)
        {
            return valor.HasValue ? valor.Value.FormatoMoneda() : "$0.00";
        }
    }
}