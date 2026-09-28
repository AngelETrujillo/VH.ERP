using VH.Services.DTOs;
using VH.Services.Interfaces;
using VH.Services.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VH.Services.Services
{
    public class ConceptoPartidaService : IConceptoPartidaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConceptoPartidaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultadoPaginado<ConceptoPartida>> GetPaginadoAsync(
            ConsultaPaginada consulta, int idProyecto)
        {
            var texto = consulta.TextoLimpio;

            return await _unitOfWork.ConceptosPartidas.GetPaginadoAsync(
                consulta,
                filtro: cp => cp.IdProyecto == idProyecto &&
                              (texto == null ||
                               cp.Descripcion.Contains(texto) ||
                               (cp.UnidadMedida != null && cp.UnidadMedida.Nombre.Contains(texto))),
                orden: q => q.OrderBy(cp => cp.Descripcion),
                includeProperties: "UnidadMedida");
        }

        public async Task<IEnumerable<ConceptoPartida>> GetPartidasByProyectoAsync(int idProyecto)
        {
            // Incluir UnidadMedida para poder mostrar su información
            return await _unitOfWork.ConceptosPartidas.FindAsync(
                filter: cp => cp.IdProyecto == idProyecto,
                includeProperties: "UnidadMedida"
            );
        }

        public async Task<ConceptoPartida?> GetPartidaByIdAsync(int idPartida)
        {
            // Incluir UnidadMedida para mapeo completo
            return await _unitOfWork.ConceptosPartidas.GetByIdAsync(
                idPartida,
                includeProperties: "UnidadMedida"
            );
        }

        public async Task<ConceptoPartida?> CreatePartidaAsync(int idProyecto, ConceptoPartida nuevaPartida)
        {
            // 1. Verificar si el proyecto existe
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(idProyecto);
            if (proyecto == null)
            {
                return null; // El proyecto padre no existe
            }

            // 2. Verificar si la UnidadMedida existe
            var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(nuevaPartida.IdUnidadMedida);
            if (unidadMedida == null)
            {
                return null; // La unidad de medida no existe
            }

            // 3. Asignar la FK y agregar la entidad
            nuevaPartida.IdProyecto = idProyecto;
            await _unitOfWork.ConceptosPartidas.AddAsync(nuevaPartida);
            await _unitOfWork.CompleteAsync(); // Guardar cambios en la DB

            return nuevaPartida;
        }

        public async Task<bool> UpdatePartidaAsync(ConceptoPartida partidaActualizada)
        {
            var partidaExistente = await _unitOfWork.ConceptosPartidas.GetByIdAsync(partidaActualizada.IdPartida);

            if (partidaExistente == null)
            {
                return false;
            }

            // Verificar si la UnidadMedida existe (si se está cambiando)
            if (partidaActualizada.IdUnidadMedida != partidaExistente.IdUnidadMedida)
            {
                var unidadMedida = await _unitOfWork.UnidadesMedida.GetByIdAsync(partidaActualizada.IdUnidadMedida);
                if (unidadMedida == null)
                {
                    return false; // La unidad de medida no existe
                }
            }

            // Mapear los campos permitidos para la actualización
            partidaExistente.Descripcion = partidaActualizada.Descripcion;
            partidaExistente.IdUnidadMedida = partidaActualizada.IdUnidadMedida;
            partidaExistente.CantidadEstimada = partidaActualizada.CantidadEstimada;
            partidaExistente.PrecioUnitarioEstimado = partidaActualizada.PrecioUnitarioEstimado;
            // NOTA: No se debe permitir cambiar IdProyecto aquí

            _unitOfWork.ConceptosPartidas.Update(partidaExistente);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeletePartidaAsync(int idPartida)
        {
            var partida = await _unitOfWork.ConceptosPartidas.GetByIdAsync(idPartida);
            if (partida == null)
            {
                return false;
            }

            _unitOfWork.ConceptosPartidas.Remove(partida);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /*
         * Aquí vivía RecalcularPresupuestoTotalAsync, y hacía dos cosas mal a la
         * vez. Sumaba CantidadEstimada -cantidades- y guardaba el resultado en
         * Proyecto.PresupuestoTotal, que las pantallas muestran como dinero: 340
         * piezas más 120 kilos daban un "presupuesto" de $460.00. Y ese campo lo
         * captura el usuario al dar de alta la obra, así que la primera partida
         * que alguien creara se llevaba por delante el importe contratado sin
         * avisar.
         *
         * Son dos números distintos y ninguno se deriva del otro: lo contratado
         * se captura, y lo repartido en partidas se calcula al consultarlo, en
         * GetResumenCostosAsync. Guardar el segundo sólo servía para que se
         * desfasara.
         */

        public async Task<Dictionary<int, (decimal Costo, int Salidas)>> GetConsumoPorPartidaAsync(
            IEnumerable<int> idsPartida)
        {
            var ids = idsPartida.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, (decimal, int)>();

            // El costo de cada salida es el del lote del que salió, no el costo
            // estimado del material: es lo que de verdad se pagó por esa pieza.
            var salidas = await _unitOfWork.EntregasEPP.FindAsync(
                filter: e => e.IdConceptoPartida != null && ids.Contains(e.IdConceptoPartida.Value),
                includeProperties: "CompraDetalle");

            return salidas
                .GroupBy(e => e.IdConceptoPartida!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Costo: g.Sum(e => e.CantidadEntregada *
                                          (e.CompraDetalle != null ? e.CompraDetalle.PrecioUnitario : 0m)),
                        Salidas: g.Count()));
        }

        public async Task<ResumenCostosProyectoDto?> GetResumenCostosAsync(int idProyecto)
        {
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(idProyecto);
            if (proyecto == null) return null;

            var partidas = (await _unitOfWork.ConceptosPartidas.FindAsync(
                cp => cp.IdProyecto == idProyecto)).ToList();

            var consumos = await GetConsumoPorPartidaAsync(partidas.Select(p => p.IdPartida));

            // El consumo cargado a la obra sin partida no aparece en el avance de
            // ninguna, pero es gasto de la obra igual y tiene que verse.
            var sueltas = await _unitOfWork.EntregasEPP.FindAsync(
                filter: e => e.IdProyectoDestino == idProyecto && e.IdConceptoPartida == null,
                includeProperties: "CompraDetalle");

            var resumen = new ResumenCostosProyectoDto
            {
                IdProyecto = idProyecto,
                NombreProyecto = proyecto.Nombre,
                PresupuestoObra = proyecto.PresupuestoTotal,
                PresupuestadoEnPartidas = partidas.Sum(p => p.CostoTotalEstimado ?? 0m),
                PartidasSinPresupuesto = partidas.Count(p => !p.PrecioUnitarioEstimado.HasValue),
                ConsumidoEnPartidas = consumos.Values.Sum(v => v.Costo),
                ConsumidoSinPartida = sueltas.Sum(e =>
                    e.CantidadEntregada * (e.CompraDetalle != null ? e.CompraDetalle.PrecioUnitario : 0m))
            };

            resumen.PartidasSobregiradas = partidas.Count(p =>
                p.CostoTotalEstimado.HasValue &&
                consumos.TryGetValue(p.IdPartida, out var c) &&
                c.Costo > p.CostoTotalEstimado.Value);

            return resumen;
        }
    }
}