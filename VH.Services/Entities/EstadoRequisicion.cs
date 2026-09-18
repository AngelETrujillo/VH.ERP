namespace VH.Services.Entities
{
    /// <summary>
    /// Estado del documento. Se deriva de los renglones mediante
    /// <see cref="RequisicionEPP.CalcularEstado"/>, no se fija a mano.
    /// </summary>
    public enum EstadoRequisicion
    {
        Pendiente = 0,
        Aprobada = 1,
        Rechazada = 2,
        Entregada = 3,
        Cancelada = 4,

        /// <summary>
        /// Parte de los renglones ya se surtió y otra parte sigue esperando.
        /// Antes no existía: la requisición se marcaba Entregada aunque se hubiera
        /// surtido uno de cinco materiales.
        /// </summary>
        Parcial = 5
    }
}
