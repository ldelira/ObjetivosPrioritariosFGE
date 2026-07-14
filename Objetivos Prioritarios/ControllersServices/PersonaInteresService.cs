using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class PersonaInteresService : BaseService
    {
        public List<tb_Persona> GetPersonasInteresList()
        {
            return dbFiliMuni.tb_Persona
                .AsNoTracking()
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();
        }

        public tb_Persona GetPersonaInteresById(int idPersona)
        {
            return dbFiliMuni.tb_Persona
                .Include(x => x.tb_Fotografia)
                .FirstOrDefault(x => x.idPersona == idPersona);
        }

        public BasicOperationResponse SavePersonaInteres(
            int idPersona,
            string nombre,
            string paterno,
            string materno,
            int? edadAproximada,
            DateTime? fechaNacimientoExacta,
            string estatura,
            string sexo,
            string observaciones,
            int usuarioRegistro)
        {
            try
            {
                nombre = string.IsNullOrWhiteSpace(nombre) ? " " : nombre.Trim();
                paterno = string.IsNullOrWhiteSpace(paterno) ? " " : paterno.Trim();
                materno = string.IsNullOrWhiteSpace(materno) ? " " : materno.Trim();

                DateTime? fechaNacimiento = null;

                if (fechaNacimientoExacta.HasValue)
                {
                    fechaNacimiento = fechaNacimientoExacta.Value;
                }
                else if (edadAproximada.HasValue && edadAproximada.Value > 0)
                {
                    int anioNacimiento = DateTime.Now.Year - edadAproximada.Value;
                    fechaNacimiento = new DateTime(anioNacimiento, 1, 1);
                }

                if (idPersona == 0)
                {
                    var nueva = new tb_Persona
                    {
                        Nombre = nombre,
                        Paterno = paterno,
                        Materno = materno,
                        FechaNacimiento = fechaNacimiento,
                        Estatura = string.IsNullOrWhiteSpace(estatura) ? null : estatura.Trim(),
                        Sexo = string.IsNullOrWhiteSpace(sexo) ? null : (sexo.Trim()=="M"?"1":"0"),
                        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim(),
                        FechaRegistro = DateTime.Now,
                        UsuarioRegistro = usuarioRegistro
                    };

                    dbFiliMuni.tb_Persona.Add(nueva);
                    dbFiliMuni.SaveChanges();

                    return new BasicOperationResponse
                    {
                        IsSuccess = true,
                        Message = "Persona de interés registrada correctamente.",
                        Id = nueva.idPersona
                    };
                }

                var persona = dbFiliMuni.tb_Persona.FirstOrDefault(x => x.idPersona == idPersona);

                if (persona == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la persona de interés."
                    };
                }

                persona.Nombre = nombre;
                persona.Paterno = paterno;
                persona.Materno = materno;
                persona.FechaNacimiento = fechaNacimiento;
                persona.Estatura = string.IsNullOrWhiteSpace(estatura) ? null : estatura.Trim();
                persona.Sexo = string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim();
                persona.Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();

                dbFiliMuni.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Persona de interés actualizada correctamente.",
                    Id = persona.idPersona
                };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la persona de interés: " + ex.Message
                };
            }
        }

        public string GetNombreMostrar(tb_Persona persona)
        {
            if (persona == null)
                return "Persona sin identificar";

            string nombreCompleto = string.Join(" ", new[]
            {
                persona.Nombre,
                persona.Paterno,
                persona.Materno
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return "Persona sin identificar";

            return nombreCompleto;
        }

        public int? GetEdadAproximada(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue)
                return null;

            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Value.Year;

            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
                edad--;

            if (edad < 0 || edad > 130)
                return null;

            return edad;
        }
    }
}