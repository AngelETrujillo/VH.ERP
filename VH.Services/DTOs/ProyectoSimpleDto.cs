using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VH.Services.DTOs
{
    public record ProyectoSimpleResponseDto(
        int IdProyecto,
        string Nombre
    );
}
