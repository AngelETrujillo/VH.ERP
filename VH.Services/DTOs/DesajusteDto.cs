namespace VH.Services.DTOs
{
    /// <summary>
    /// Un dato que está guardado en dos lugares y ya no coincide.
    ///
    /// El sistema guarda a propósito algunos totales calculados —lo recibido de una
    /// orden, lo entregado de un renglón— para no tener que sumarlos en cada
    /// consulta. La contrapartida es que pueden separarse de lo que suman sus
    /// partes, y entonces la pantalla enseña un número que ya no es cierto.
    ///
    /// Esta revisión los compara. Cero filas significa que los atajos siguen
    /// diciendo la verdad.
    /// </summary>
    public class DesajusteDto
    {
        /// <summary>Qué se está comparando: "Recibido de la orden", "Lote del proveedor"…</summary>
        public string Concepto { get; set; } = string.Empty;

        /// <summary>El documento concreto, con su folio, para poder ir a verlo.</summary>
        public string Documento { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;

        /// <summary>Lo que dice el campo guardado.</summary>
        public string Guardado { get; set; } = string.Empty;

        /// <summary>Lo que sale de sumar o de consultar la fuente.</summary>
        public string Real { get; set; } = string.Empty;

        /// <summary>
        /// Qué hacer. Un desajuste no siempre es un error de datos: a veces es una
        /// corrección legítima que no se propagó.
        /// </summary>
        public string Sugerencia { get; set; } = string.Empty;
    }
}
