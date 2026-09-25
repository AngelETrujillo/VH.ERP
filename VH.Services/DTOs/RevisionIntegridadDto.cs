namespace VH.Services.DTOs
{
    /// <summary>
    /// Lo que enseña la pantalla de revisión: existencia que el kardex no explica,
    /// y totales guardados que ya no coinciden con lo que suman sus partes.
    ///
    /// Son dos preguntas distintas y conviene verlas juntas: la primera dice si el
    /// almacén cuadra, la segunda si los atajos que el sistema guarda calculados
    /// siguen diciendo la verdad.
    /// </summary>
    public class RevisionIntegridadDto
    {
        public List<DescuadreDto> Descuadres { get; set; } = new();
        public List<DesajusteDto> Desajustes { get; set; } = new();

        public bool TodoCuadra => Descuadres.Count == 0 && Desajustes.Count == 0;

        public int Total => Descuadres.Count + Desajustes.Count;
    }
}
