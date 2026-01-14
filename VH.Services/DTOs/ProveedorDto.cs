using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VH.Services.DTOs
{
    // ProveedorRequestDto.cs
    public record ProveedorRequestDto(
        string Nombre,
        string RFC,
        string Contacto,
        string Telefono,
        bool Activo
    );

    // ProveedorResponseDto.cs
    public record ProveedorResponseDto(
        int IdProveedor,
        string Nombre,
        string RFC,
        string Contacto,
        string Telefono,
        bool Activo
    );
}
