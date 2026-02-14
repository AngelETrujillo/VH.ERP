//using System.ComponentModel.DataAnnotations;
//using VH.Services.Entities;

//namespace VH.Services.DTOs.Analytics
//{
//    public class PuestoRequestDto
//    {
//        [Required(ErrorMessage = "El nombre es obligatorio")]
//        [MaxLength(100)]
//        public string Nombre { get; set; } = string.Empty;

//        [MaxLength(500)]
//        public string? Descripcion { get; set; }

//        public NivelRiesgoEPP NivelRiesgoEPP { get; set; } = NivelRiesgoEPP.Medio;

//        public bool Activo { get; set; } = true;
//    }

//    public class PuestoResponseDto
//    {
//        public int IdPuesto { get; set; }
//        public string Nombre { get; set; } = string.Empty;
//        public string Descripcion { get; set; } = string.Empty;
//        public NivelRiesgoEPP NivelRiesgoEPP { get; set; }
//        public string NivelRiesgoTexto { get; set; } = string.Empty;
//        public bool Activo { get; set; }
//        public int TotalEmpleados { get; set; }
//    }
//}