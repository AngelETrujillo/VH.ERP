using System.Globalization;

namespace VH.Web.Helpers
{
    public static class FormatoHelper
    {
        /// <summary>
        /// Formato de cantidad: entero si no tiene decimales, 2 decimales si tiene.
        /// 10.0000 → "10"    |    10.5000 → "10.50"    |    10.2500 → "10.25"
        ///
        /// Las cantidades de almacén casi siempre son piezas enteras; mostrar
        /// "10.0000 cascos" no dice nada que "10 cascos" no diga mejor. Los
        /// decimales sólo aparecen cuando de verdad los hay, como en un consumible
        /// que se mide por litros.
        /// </summary>
        public static string FormatoCantidad(this decimal valor)
        {
            // Ojo: aquí había una llamada recursiva a sí misma en lugar de dar
            // formato. Cualquier cantidad con decimales tumbaba el proceso entero
            // con StackOverflow, que no se puede capturar.
            return valor == Math.Truncate(valor)
                ? valor.ToString("N0")
                : valor.ToString("N2");
        }

        /// <summary>
        /// Valor para un &lt;input type="number"&gt;.
        ///
        /// Un campo numérico de HTML sólo acepta punto decimal y ningún separador
        /// de miles: con "1,500" el navegador lo toma como vacío y el usuario
        /// pierde el dato sin que nada se lo avise. Por eso aquí no se usa el
        /// formato de presentación, que sí agrupa.
        /// </summary>
        public static string FormatoInput(this decimal valor)
        {
            return valor.ToString("0.####", CultureInfo.InvariantCulture);
        }

        /// <summary>Precio para un &lt;input type="number"&gt;: dos decimales, sin agrupar.</summary>
        public static string FormatoInputPrecio(this decimal valor)
        {
            return valor.ToString("0.00", CultureInfo.InvariantCulture);
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