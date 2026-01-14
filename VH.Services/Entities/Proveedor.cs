using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(13)]
        public string RFC { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Contacto { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Navigation Property
        public virtual ICollection<EntregaEPP> EntregasEPP { get; set; } = new List<EntregaEPP>();
    }
}