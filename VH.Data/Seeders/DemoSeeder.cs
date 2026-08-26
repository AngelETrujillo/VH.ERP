using Microsoft.EntityFrameworkCore;
using VH.Services.Entities;

namespace VH.Data.Seeders
{
    /// <summary>
    /// Datos de demostración para presentaciones y capturas de pantalla.
    ///
    /// Reglas que sigue este seeder:
    ///
    /// 1. **No inventa clientes.** Todos los nombres —constructora, obras,
    ///    empleados, proveedores— son ficticios. Nada aquí puede confundirse
    ///    con la operación real de un cliente.
    /// 2. **Fechas relativas a hoy.** El dashboard filtra por mes actual; con
    ///    fechas fijas la demo abre en ceros a los treinta días y la captura
    ///    de la presentación deja de coincidir con lo que ve el prospecto.
    /// 3. **Determinista.** Sin Random: la misma demo, siempre. Un número raro
    ///    se puede reproducir para entenderlo.
    /// 4. **No se ejecuta solo.** Solo corre si la variable de entorno
    ///    VHERP_SEED_DEMO es "true". Nunca en la instalación de un cliente.
    /// </summary>
    public static class DemoSeeder
    {
        public static async Task SeedAsync(VHERPContext db)
        {
            // Candado: sin la variable de entorno, este seeder no existe.
            if (Environment.GetEnvironmentVariable("VHERP_SEED_DEMO") != "true") return;

            // Los registros heredados de la instalacion anterior traen nombres
            // de personas y de empresas reales. Se renombran a genericos antes
            // de cualquier otra cosa: esta base se usa para capturas y demos,
            // y ahi no puede aparecer el dato de nadie.
            await NormalizarHeredadosAsync(db);

            // Si ya hay entregas de este mes, la demo ya está armada.
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (await db.EntregasEPP.AnyAsync(e => e.FechaEntrega >= inicioMes)) return;

            var hoy = DateTime.Today;

            // ── Unidades de medida ────────────────────────────────────────
            var pieza = await ObtenerOCrear(db.UnidadesMedida,
                u => u.Nombre == "Pieza",
                () => new UnidadMedida { Nombre = "Pieza", Abreviatura = "Pza", Descripcion = "Unidad individual" });
            var par = await ObtenerOCrear(db.UnidadesMedida,
                u => u.Nombre == "Par",
                () => new UnidadMedida { Nombre = "Par", Abreviatura = "Par", Descripcion = "Dos piezas" });
            await db.SaveChangesAsync();

            // ── Obras ─────────────────────────────────────────────────────
            var torre = await ObtenerOCrear(db.Proyectos,
                p => p.Nombre == "Torre Cumbres Residencial",
                () => new Proyecto
                {
                    Nombre = "Torre Cumbres Residencial",
                    TipoObra = "Edificación vertical",
                    FechaInicio = hoy.AddMonths(-8),
                    FechaFinEstimada = hoy.AddMonths(10),
                    PresupuestoTotal = 48_500_000m,
                    PresupuestoEPPMensual = 42_000m,
                    EmpleadosEsperados = 65,
                    Activo = true
                });
            var vial = await ObtenerOCrear(db.Proyectos,
                p => p.Nombre == "Ampliación Vial Carretera Nacional",
                () => new Proyecto
                {
                    Nombre = "Ampliación Vial Carretera Nacional",
                    TipoObra = "Infraestructura",
                    FechaInicio = hoy.AddMonths(-4),
                    FechaFinEstimada = hoy.AddMonths(7),
                    PresupuestoTotal = 22_300_000m,
                    PresupuestoEPPMensual = 18_000m,
                    EmpleadosEsperados = 30,
                    Activo = true
                });
            await db.SaveChangesAsync();

            // ── Almacenes ─────────────────────────────────────────────────
            var bodegaTorre = await ObtenerOCrear(db.Almacenes,
                a => a.Nombre == "Bodega Central Cumbres",
                () => new Almacen
                {
                    Nombre = "Bodega Central Cumbres",
                    Descripcion = "Almacén principal de la obra",
                    Domicilio = "Av. Paseo de los Leones s/n, Monterrey, N.L.",
                    TipoUbicacion = "Bodega fija",
                    IdProyecto = torre.IdProyecto,
                    Activo = true
                });
            var bodegaVial = await ObtenerOCrear(db.Almacenes,
                a => a.Nombre == "Almacén Móvil Carretera",
                () => new Almacen
                {
                    Nombre = "Almacén Móvil Carretera",
                    Descripcion = "Contenedor en campo, km 14",
                    Domicilio = "Carretera Nacional km 14, Santiago, N.L.",
                    TipoUbicacion = "Contenedor en campo",
                    IdProyecto = vial.IdProyecto,
                    Activo = true
                });
            await db.SaveChangesAsync();

            // ── Proveedores ───────────────────────────────────────────────
            var provSeg = await ObtenerOCrear(db.Proveedores,
                p => p.Nombre == "Seguridad Industrial del Norte",
                () => new Proveedor
                {
                    Nombre = "Seguridad Industrial del Norte",
                    RFC = "SIN180504QX3",
                    Contacto = "Mariana Cepeda",
                    Telefono = "81 8340 2211",
                    Activo = true
                });
            var provEquipo = await ObtenerOCrear(db.Proveedores,
                p => p.Nombre == "Equipo y Protección Regia",
                () => new Proveedor
                {
                    Nombre = "Equipo y Protección Regia",
                    RFC = "EPR150922KL7",
                    Contacto = "Rubén Ortega",
                    Telefono = "81 8112 7788",
                    Activo = true
                });
            await db.SaveChangesAsync();

            // ── Puestos ───────────────────────────────────────────────────
            var puestos = new (string Nombre, string Desc, NivelRiesgoEPP Riesgo)[]
            {
                ("Albañil",            "Mampostería y acabados",              NivelRiesgoEPP.Medio),
                ("Soldador",           "Estructura metálica y armado",        NivelRiesgoEPP.Alto),
                ("Operador de grúa",   "Maniobras de izaje",                  NivelRiesgoEPP.Alto),
                ("Ayudante general",   "Apoyo en obra",                       NivelRiesgoEPP.Medio),
                ("Supervisor de obra", "Coordinación y control de avance",    NivelRiesgoEPP.Bajo),
            };
            var puestosCat = new Dictionary<string, Puesto>();
            foreach (var (nombre, desc, riesgo) in puestos)
            {
                puestosCat[nombre] = await ObtenerOCrear(db.Puestos,
                    p => p.Nombre == nombre,
                    () => new Puesto { Nombre = nombre, Descripcion = desc, NivelRiesgoEPP = riesgo, Activo = true });
            }
            await db.SaveChangesAsync();

            // ── Empleados ─────────────────────────────────────────────────
            // Nombres ficticios. Ningún dato de persona real.
            var plantilla = new (string Nom, string ApP, string ApM, string Puesto, int Obra, string Nomina)[]
            {
                ("Ricardo",  "Salazar",  "Mendoza",  "Soldador",           1, "EMP-1041"),
                ("Fernando", "Guerrero", "Ibarra",   "Albañil",            1, "EMP-1042"),
                ("Joel",     "Ramírez",  "Treviño",  "Operador de grúa",   1, "EMP-1043"),
                ("Alonso",   "Cavazos",  "Puente",   "Ayudante general",   1, "EMP-1044"),
                ("Beatriz",  "Nájera",   "Solís",    "Supervisor de obra", 1, "EMP-1045"),
                ("Gerardo",  "Montoya",  "Lozano",   "Albañil",            2, "EMP-2011"),
                ("Iván",     "Delgado",  "Cortés",   "Soldador",           2, "EMP-2012"),
                ("Paulina",  "Escobedo", "Villareal","Supervisor de obra", 2, "EMP-2013"),
            };
            var empleados = new List<Empleado>();
            foreach (var (nom, apP, apM, pst, obra, nomina) in plantilla)
            {
                var e = await ObtenerOCrear(db.Empleados,
                    x => x.NumeroNomina == nomina,
                    () => new Empleado
                    {
                        Nombre = nom,
                        ApellidoPaterno = apP,
                        ApellidoMaterno = apM,
                        NumeroNomina = nomina,
                        Puesto = pst,
                        IdPuesto = puestosCat[pst].IdPuesto,
                        FechaIngreso = hoy.AddMonths(-(6 + (nomina.Length % 5))),
                        IdProyecto = obra == 1 ? torre.IdProyecto : vial.IdProyecto,
                        Activo = true
                    });
                empleados.Add(e);
            }
            await db.SaveChangesAsync();

            // ── Materiales de EPP ─────────────────────────────────────────
            var catalogo = new (string Nombre, string Desc, decimal Costo, int? Vida, bool Desechable, CategoriaRiesgoMaterial Cat, int Unidad)[]
            {
                ("Casco de seguridad clase E",  "Dieléctrico, ala completa",         389m, 730, false, CategoriaRiesgoMaterial.Alto,  1),
                ("Botas dieléctricas casquillo","Suela antiderrapante",             1290m, 365, false, CategoriaRiesgoMaterial.Alto,  2),
                ("Guantes de carnaza",          "Reforzados, uso rudo",               85m,  30, true,  CategoriaRiesgoMaterial.Medio, 2),
                ("Lentes de seguridad claros",  "Antiempañante",                      98m,  90, true,  CategoriaRiesgoMaterial.Medio, 1),
                ("Careta para soldar",          "Fotosensible, tono variable",      1850m, 730, false, CategoriaRiesgoMaterial.Alto,  1),
                ("Arnés de cuerpo completo",    "Cuatro argollas, con línea de vida",2450m,1095, false, CategoriaRiesgoMaterial.Alto,  1),
                ("Chaleco reflejante",          "Alta visibilidad",                  120m, 180, false, CategoriaRiesgoMaterial.Bajo,  1),
                ("Tapones auditivos",           "Espuma, desechables",                18m,   7, true,  CategoriaRiesgoMaterial.Bajo,  2),
            };
            var materiales = new List<MaterialEPP>();
            foreach (var (nombre, desc, costo, vida, desech, cat, uni) in catalogo)
            {
                var m = await ObtenerOCrear(db.MaterialesEPP,
                    x => x.Nombre == nombre,
                    () => new MaterialEPP
                    {
                        Nombre = nombre,
                        Descripcion = desc,
                        IdUnidadMedida = uni == 1 ? pieza.IdUnidadMedida : par.IdUnidadMedida,
                        CostoUnitarioEstimado = costo,
                        VidaUtilDiasDefault = vida,
                        EsDesechable = desech,
                        CategoriaRiesgo = cat,
                        Activo = true
                    });
                materiales.Add(m);
            }
            await db.SaveChangesAsync();

            // ── Compras ───────────────────────────────────────────────────
            // Dos meses de abasto, para que las entregas tengan de dónde salir.
            var compras = new List<CompraEPP>();
            for (int i = 0; i < materiales.Count; i++)
            {
                var m = materiales[i];
                var esTorre = i % 3 != 2;
                var compra = new CompraEPP
                {
                    IdMaterial = m.IdMaterial,
                    IdProveedor = (i % 2 == 0 ? provSeg : provEquipo).IdProveedor,
                    IdAlmacen = (esTorre ? bodegaTorre : bodegaVial).IdAlmacen,
                    FechaCompra = hoy.AddDays(-(45 - i * 3)),
                    CantidadComprada = 30 + i * 5,
                    CantidadDisponible = 12 + i * 3,
                    PrecioUnitario = m.CostoUnitarioEstimado,
                    NumeroDocumento = $"FAC-{2600 + i}",
                    Observaciones = "Abasto programado"
                };
                db.ComprasEPP.Add(compra);
                compras.Add(compra);
            }
            await db.SaveChangesAsync();

            // ── Inventarios ───────────────────────────────────────────────
            for (int i = 0; i < materiales.Count; i++)
            {
                var m = materiales[i];
                var alm = i % 3 != 2 ? bodegaTorre : bodegaVial;
                if (await db.Inventarios.AnyAsync(x => x.IdMaterial == m.IdMaterial && x.IdAlmacen == alm.IdAlmacen)) continue;
                // Dos materiales quedan bajo mínimo a propósito: un almacén donde
                // todo está en verde no enseña para qué sirve el control.
                var bajo = i == 2 || i == 7;
                db.Inventarios.Add(new Inventario
                {
                    IdAlmacen = alm.IdAlmacen,
                    IdMaterial = m.IdMaterial,
                    Existencia = bajo ? 3 + i : 24 + i * 4,
                    StockMinimo = 10 + i,
                    StockMaximo = 80 + i * 5,
                    UbicacionPasillo = $"Pasillo {(char)('A' + i % 4)}-{i + 1:00}",
                    FechaUltimoMovimiento = hoy.AddDays(-(i + 1))
                });
            }
            await db.SaveChangesAsync();

            // ── Entregas ──────────────────────────────────────────────────
            // Reparto determinista sobre este mes y el anterior, para que el
            // KPI de variación mensual tenga contra qué compararse.
            var guion = new (int Emp, int Compra, int DiasAtras, decimal Cant, string Talla)[]
            {
                (0, 4,  2, 1, "-"),   (0, 2,  3, 2, "G"),   (1, 0,  4, 1, "M"),
                (2, 6,  5, 1, "G"),   (3, 3,  6, 3, "-"),   (4, 6,  8, 1, "M"),
                (5, 1,  9, 1, "27"),  (6, 4, 11, 1, "-"),   (7, 3, 12, 2, "-"),
                (1, 7, 13, 4, "-"),   (0, 5, 15, 1, "-"),   (2, 2, 16, 2, "G"),
                (3, 0, 18, 1, "L"),   (5, 3, 20, 2, "-"),   (6, 1, 22, 1, "28"),
                (4, 7, 24, 3, "-"),   (7, 2, 26, 2, "M"),   (1, 6, 28, 1, "M"),
                (2, 0, 34, 1, "G"),   (3, 5, 38, 1, "-"),   (0, 3, 41, 2, "-"),
                (6, 2, 45, 2, "G"),   (5, 0, 48, 1, "M"),   (4, 1, 52, 1, "25"),
            };
            foreach (var (emp, comp, dias, cant, talla) in guion)
            {
                db.EntregasEPP.Add(new EntregaEPP
                {
                    IdEmpleado = empleados[emp].IdEmpleado,
                    IdCompra = compras[comp].IdCompra,
                    FechaEntrega = hoy.AddDays(-dias),
                    CantidadEntregada = cant,
                    TallaEntregada = talla,
                    Observaciones = ""
                });
            }
            await db.SaveChangesAsync();

            // ── Alertas de consumo ────────────────────────────────────────
            // Una de cada severidad, para que la gráfica por tipo tenga forma.
            var alertas = new (int Emp, int Mat, TipoAlerta Tipo, SeveridadAlerta Sev, string Desc, string Esp, string Real, decimal Desv, decimal Costo, int Dias)[]
            {
                (0, 2, TipoAlerta.ExcesoFrecuencia,     SeveridadAlerta.Critica,
                 "Solicitó guantes de carnaza 5 veces en 30 días; la vida útil del material es de 30 días.",
                 "1 par al mes", "5 pares", 400m, 340m, 1),
                (3, 3, TipoAlerta.SolicitudPrematura,   SeveridadAlerta.Alta,
                 "Reemplazo de lentes a los 18 días de entregados. Vida útil estimada: 90 días.",
                 "90 días", "18 días", 80m, 98m, 3),
                (6, 7, TipoAlerta.ExcesoCantidad,       SeveridadAlerta.Media,
                 "Retiró 4 paquetes de tapones auditivos en una sola entrega.",
                 "1 paquete", "4 paquetes", 300m, 54m, 5),
                (1, 0, TipoAlerta.PatronAnomalo,        SeveridadAlerta.Media,
                 "Tres reposiciones de casco en el trimestre, todas en viernes.",
                 "1 por año", "3 en 90 días", 200m, 1167m, 8),
                (4, 5, TipoAlerta.DesviacionPresupuestal, SeveridadAlerta.Alta,
                 "El consumo de EPP de la obra va 18% arriba del presupuesto mensual.",
                 "$42,000.00", "$49,560.00", 18m, 7560m, 2),
                (2, 4, TipoAlerta.TopConsumidor,        SeveridadAlerta.Baja,
                 "Encabeza el consumo de la obra este mes.",
                 "Promedio $1,850.00", "$4,210.00", 127m, 4210m, 6),
            };
            foreach (var (emp, mat, tipo, sev, desc, esp, real, desv, costo, dias) in alertas)
            {
                db.AlertasConsumo.Add(new AlertaConsumo
                {
                    IdEmpleado = empleados[emp].IdEmpleado,
                    IdMaterial = materiales[mat].IdMaterial,
                    IdProyecto = empleados[emp].IdProyecto,
                    TipoAlerta = tipo,
                    Severidad = sev,
                    Descripcion = desc,
                    ValorEsperado = esp,
                    ValorReal = real,
                    Desviacion = desv,
                    CostoEstimado = costo,
                    FechaGeneracion = hoy.AddDays(-dias),
                    EstadoAlerta = EstadoAlerta.Pendiente
                });
            }
            await db.SaveChangesAsync();

            Console.WriteLine("[DemoSeeder] Datos de demostración cargados.");
        }


        /// <summary>
        /// Renombra los registros heredados que traen datos de personas o de
        /// empresas reales. No borra nada: renombrar conserva el historial y
        /// las llaves foraneas intactas.
        /// </summary>
        private static async Task NormalizarHeredadosAsync(VHERPContext db)
        {
            var renombres = new (string Antes, string Despues)[]
            {
                ("Barda Cemex", "Barda Perimetral Poniente"),
            };
            foreach (var (antes, despues) in renombres)
            {
                var proyecto = await db.Proyectos.FirstOrDefaultAsync(x => x.Nombre == antes);
                if (proyecto != null) proyecto.Nombre = despues;
            }

            var almacen = await db.Almacenes.FirstOrDefaultAsync(x => x.Nombre == "Bodega Cemex");
            if (almacen != null) almacen.Nombre = "Bodega Poniente";

            var prov1 = await db.Proveedores.FirstOrDefaultAsync(x => x.Nombre == "Home depot");
            if (prov1 != null) { prov1.Nombre = "Suministros Industriales Bravo"; prov1.Contacto = "Atencion a clientes"; }

            var prov2 = await db.Proveedores.FirstOrDefaultAsync(x => x.Nombre == "PETRA TORRES MARTINEZ");
            if (prov2 != null) { prov2.Nombre = "Comercializadora Aguilar"; prov2.Contacto = "Ventas"; }

            // El empleado 001 es el de pruebas del desarrollo.
            var emp = await db.Empleados.FirstOrDefaultAsync(x => x.NumeroNomina == "001");
            if (emp != null)
            {
                emp.Nombre = "Martin";
                emp.ApellidoPaterno = "Reyes";
                emp.ApellidoMaterno = "Ochoa";
                emp.NumeroNomina = "EMP-1001";
            }

            // Un presupuesto realista para que el KPI de desviacion tenga algo
            // que reportar: una obra pasada de presupuesto es justo lo que el
            // modulo existe para detectar.
            var torre = await db.Proyectos.FirstOrDefaultAsync(x => x.Nombre == "Torre Cumbres Residencial");
            if (torre != null) torre.PresupuestoEPPMensual = 5150m;

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Devuelve la fila que cumple el criterio o la crea. Evita duplicar
        /// catálogos cuando el seeder corre más de una vez.
        /// </summary>
        private static async Task<T> ObtenerOCrear<T>(
            DbSet<T> set,
            System.Linq.Expressions.Expression<Func<T, bool>> criterio,
            Func<T> crear) where T : class
        {
            var existente = await set.FirstOrDefaultAsync(criterio);
            if (existente != null) return existente;
            var nuevo = crear();
            set.Add(nuevo);
            return nuevo;
        }
    }
}
