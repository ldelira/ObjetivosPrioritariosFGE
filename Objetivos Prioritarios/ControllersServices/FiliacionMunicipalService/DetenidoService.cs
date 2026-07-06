using Antlr.Runtime.Misc;
using FiliacionMunicipal.Models;
using FiliacionMunicipal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Web;
using System.Xml.Linq;
using Web_SIPAEIC.ControllerServices;

namespace FiliacionMunicipal.ControllerServices
{
    public class DetenidoService : BaseService
    {
        public void Guardar(DetenidoVM model, HttpFileCollectionBase fotos, HttpPostedFileBase pdf, string[] tiposFoto)
        {
            string rutaBase = ConfigurationManager.AppSettings["RutaArchivos"];
            int idUsuario = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]?.ToString()),Id_Persona, edad=0;
            using (var transaction = db.Database.BeginTransaction())
            {
                //var archivosFotos = new List<(HttpPostedFileBase file, string ruta)>();
                var archivosFotos = new List<Tuple<HttpPostedFileBase, string>>();
                //(HttpPostedFileBase file, string ruta) archivoPdf = (null, null);
                // Para el PDF:
                Tuple<HttpPostedFileBase, string> archivoPdf = null;

                // Para agregar datos usarías:
                archivosFotos.Add(new Tuple<HttpPostedFileBase, string>(null, null));

                // Para leer los datos usarías .Item1 y .Item2:
                var elArchivo = archivosFotos[0].Item1;
                var laRuta = archivosFotos[0].Item2;
                try
                {
                    // 1. PERSONA
                    tb_Persona persona;
                    if (model.idPersona.HasValue && model.idPersona > 0)
                    {
                        // EDITAR
                        persona = db.tb_Persona.Find(model.idPersona);

                        if (persona == null)
                            throw new Exception("Persona no encontrada");

                        persona.Nombre = model.Nombre;
                        persona.Paterno = model.Paterno;
                        persona.Materno = model.Materno;
                        persona.FechaNacimiento = model.FechaNacimiento;
                        persona.Estatura = model.Estatura.ToString();
                        persona.idEscolaridad = model.idEscolaridad;
                        persona.idOcupacion = model.idOcupacion;
                        persona.Telefono = model.Telefono;
                        persona.Sexo = model.Sexo;
                        persona.CURP = model.CURP;
                        persona.Observaciones = model.Observaciones;

                        persona.FechaActualizacion = DateTime.Now;
                        persona.UsuarioModificacion = idUsuario;
                    }
                    else
                    {
                        //NUEVO
                        persona = new tb_Persona
                        {
                            Nombre = model.Nombre,
                            Paterno = model.Paterno,
                            Materno = model.Materno,
                            FechaNacimiento = model.FechaNacimiento,
                            Estatura = model.Estatura.ToString(),
                            idEscolaridad = model.idEscolaridad,
                            idOcupacion = model.idOcupacion,
                            Telefono = model.Telefono,
                            Sexo = model.Sexo,
                            CURP = model.CURP,
                            Observaciones = model.Observaciones,
                            FechaRegistro = DateTime.Now,
                            UsuarioRegistro = idUsuario
                        };

                        db.tb_Persona.Add(persona);
                    }
                    
                    db.SaveChanges();
                    if (model.idPersona.HasValue && model.idPersona > 0)
                        Id_Persona = model.idPersona ?? 0;
                    else
                        Id_Persona = persona.idPersona;

                    // 1.1. ALIAS VALIDAR Y SEPARAR
                    if (!string.IsNullOrWhiteSpace(model.Alias))
                    {
                        var listaAlias = model.Alias
                            .Split(',')
                            .Select(a => a.Trim())              
                            .Where(a => !string.IsNullOrEmpty(a)) 
                            .Distinct()                         
                            .ToList();

                        //  alias existentes en BD
                        var aliasExistentes = db.tb_PersonaAlias
                            .Where(x => x.idPersona == Id_Persona)
                            .Select(x => x.Alias.ToLower())
                            .ToList();

                        // Filtrar los nuevos
                        var aliasParaGuardar = listaAlias
                            .Where(a => !aliasExistentes.Contains(a))
                            .ToList();

                        foreach (var alias in aliasParaGuardar)
                        {
                            var entidadAlias = new tb_PersonaAlias
                            {
                                idPersona = Id_Persona,
                                Alias = alias,
                                FechaRegistro = DateTime.Now,
                                UsuarioRegistro = idUsuario,
                                Activo=true
                            };

                            db.tb_PersonaAlias.Add(entidadAlias);
                        }

                        db.SaveChanges();
                    }

                    // 2. DOMICILIO
                    if (model.Nuevo == "si")
                    {
                        int ne, ni;
                        int? numExt = int.TryParse(model.NumExt, out ne) ? ne : (int?)null;
                        int? numInt = int.TryParse(model.NumInt, out ni) ? ni : (int?)null;

                        var existe = db.tb_Domicilio
                            .FirstOrDefault(x =>
                                x.idPersona == Id_Persona &&
                                x.Cve_Calle == model.Cve_Calle &&
                                x.NumExt == numExt
                            );

                        if (existe == null)
                        {
                            // Desactivar anteriores
                            var anteriores = db.tb_Domicilio
                                .Where(x => x.idPersona == Id_Persona && x.Tipo == 1)
                                .ToList();

                            foreach (var d in anteriores)
                            {
                                d.Tipo = 0;
                            }

                            // Nuevo domicilio
                            var nuevo = new tb_Domicilio
                            {
                                idPersona = Id_Persona,
                                Cve_Calle = model.Cve_Calle,
                                NumExt = numExt,
                                NumInt = numInt,
                                FechaRegistro = DateTime.Now,
                                UsuarioRegistro = idUsuario,
                                Tipo = 1
                            };

                            db.tb_Domicilio.Add(nuevo);
                        }
                        else
                        {
                            var anteriores = db.tb_Domicilio
                               .Where(x => x.idPersona == Id_Persona)
                               .ToList();
                            // Desactivar anteriores
                            foreach (var d in anteriores)
                            {
                                d.Tipo = (d.idDomicilio == existe.idDomicilio) ? 1 : 0;
                            }
                        }
                            db.SaveChanges();
                    }

                    // 3. DETENCIÓN
                    if (model.Edad != "")
                    {
                        // Filtra solo los números y los une en un nuevo string
                        string soloNumero = new string(model.Edad.Where(char.IsDigit).ToArray());

                        // Si necesitas guardarlo como un entero (int) para tu base de datos:
                        edad = int.Parse(soloNumero);
                    }
                        var detencion = new tb_Detencion
                        {
                            idPersona = Id_Persona,
                            idMunicipio = Convert.ToInt32(model.idMunicipioDetencion),
                            FechaDetencion = (DateTime)model.FechaDetencion,
                            FechaLiberacion = (DateTime)model.FechaLiberacion,
                            idFaltaAdministrativa = Convert.ToInt32(model.FaltaAdministrativa),
                            idAutoridadRemitente = Convert.ToInt32(model.idAutoridadRemitente),
                            Estatus = 1,
                            Observaciones = model.ObservacionesDetencion,
                            FechaRegistro = DateTime.Now,
                            UsuarioRegistro = idUsuario,
                            Edad = edad
                        };

                    db.tb_Detencion.Add(detencion);

                    db.SaveChanges();

                    // 4. PDF
                    if (pdf != null && pdf.ContentLength > 0)
                    {

                        var ficha = new tb_FichaDecadactilar
                        {
                            idPersona = Id_Persona,
                            FechaRegistro = DateTime.Now,
                            UsuarioRegistro = idUsuario,
                            Activo=true
                        };

                        db.tb_FichaDecadactilar.Add(ficha);

                        db.SaveChanges();

                        if (ficha != null)
                        {
                            // 🔥 LEER ARCHIVO A BYTES
                            byte[] bytes;
                            using (var binaryReader = new BinaryReader(pdf.InputStream))
                            {
                                bytes = binaryReader.ReadBytes(pdf.ContentLength);
                            }

                            // 🔥 CONVERTIR A BASE64
                            string base64 = Convert.ToBase64String(bytes);

                            // 🔥 OPCIONAL: guardar extensión
                            string extension = Path.GetExtension(pdf.FileName);

                            // 🔥 GUARDAR EN BD
                            ficha.ArchivoPDF = base64;
                            ficha.RutaArchivo = extension; // opcional (o tipo MIME)

                            db.SaveChanges(); 
                        }
                    }

                    // 5. FOTOS
                    for (int i = 0; i < fotos.Count-1; i++)
                    {
                        var file = fotos[i];

                        if (file != null && file.ContentLength > 0)
                        {
                            int tipo = 0;

                            if (tiposFoto != null && tiposFoto.Length > i)
                            {
                                tipo = Convert.ToInt32(tiposFoto[i]);
                            }

                            var fotografia = new tb_Fotografia
                            {
                                idPersona = Id_Persona,
                                idTipoFoto = tipo,
                                FechaRegistro = DateTime.Now,
                                UsuarioRegistro = idUsuario
                            };

                            db.tb_Fotografia.Add(fotografia);

                            db.SaveChanges();

                            if (fotografia != null)
                            {
                                // 🔥 LEER ARCHIVO A BYTES
                                byte[] bytes;
                                using (var binaryReader = new BinaryReader(file.InputStream))
                                {
                                    bytes = binaryReader.ReadBytes(file.ContentLength);
                                }
                                // 🔥 CONVERTIR A BASE64
                                string base64 = Convert.ToBase64String(bytes);

                                // 🔥 OPCIONAL: guardar extensión
                                string extension = Path.GetExtension(file.FileName);

                                // 🔥 GUARDAR EN BD
                                fotografia.ArchivoB64 = base64;
                                fotografia.RutaArchivo = extension; // opcional (o tipo MIME)

                                db.SaveChanges(); 
                            }
                        }
                    }

                    // 6. ALERTA
                    if (model.IdsAlertasEncontrados.Count > 0)
                    {
                        foreach (int id in model.IdsAlertasEncontrados)
                        {
                            tb_Alerta Alertas = db.tb_Alerta.Find(id);
                            Alertas.idPersona = Id_Persona;
                            db.SaveChanges();
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<BuscarDetenidoVM> Buscar(string nombre)
        {
            return db.Database.SqlQuery<BuscarDetenidoVM>(
                "EXEC sp_BuscarDetenido @Nombre",
                new SqlParameter("@Nombre", nombre)
            ).ToList();
        }
        public List<int> VerificarFGEA(string nombre, string paterno, string materno)
        {
            try
            {
                var idAlertas = db.Database.SqlQuery<int>(
                   "EXEC sp_BusquedaAlertasFGEA @Nombre, @Paterno, @Materno",
                   new SqlParameter("@Nombre", nombre ?? ""),
                   new SqlParameter("@Paterno", paterno ?? ""),
                   new SqlParameter("@Materno", materno ?? "")
               ).ToList();

                return idAlertas;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

