using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using OpenXmlPackaging = DocumentFormat.OpenXml.Packaging;

namespace Objetivos_Prioritarios.Utils
{

    

    public static class PowerPointGeneratorNoImagePartType
    {
        /// <summary>
        /// Genera la presentación a partir de plantilla. No usa ImagePartType en tiempo de compilación.
        /// </summary>
        public static void GenerarPresentacion(List<Detenido> detenidos, string plantillaPath, string salidaPath, string defaultImg, string fondo, bool eliminarPlantillaOriginal = true)
        {
            if (!File.Exists(plantillaPath))
                throw new FileNotFoundException("No se encontró la plantilla", plantillaPath);

            File.Copy(plantillaPath, salidaPath, true);

            using (var ppt = OpenXmlPackaging.PresentationDocument.Open(salidaPath, true))
            {
                var presentationPart = ppt.PresentationPart ?? throw new Exception("No PresentationPart");

                var slideTemplate = presentationPart.SlideParts.FirstOrDefault()
                    ?? throw new Exception("La plantilla no contiene diapositivas.");

                // Obtener SlideId de la plantilla para eliminar luego (opcional)
                string plantillaRelId = presentationPart.GetIdOfPart(slideTemplate);
                P.SlideId plantillaSlideId = presentationPart.Presentation.SlideIdList?
                    .ChildElements
                    .OfType<P.SlideId>()
                    .FirstOrDefault(s => s.RelationshipId == plantillaRelId);

                foreach (var detenido in detenidos)
                {
                    var newSlide = CloneSlidePart(presentationPart, slideTemplate);

                    // Reemplazo de textos
                    ReplaceText(newSlide, "[C1]", detenido.Nombre);
                    ReplaceText(newSlide, "[C2]", detenido.Cartel);
                    ReplaceText(newSlide, "[C3]", detenido.Ocupacion);


                    ReplaceText(newSlide, "[C4]", ListToBulletedText(detenido.Delitos));
                    ReplaceText(newSlide, "[C5]", ListToBulletedText(detenido.Carpetas));
                    ReplaceText(newSlide, "[C6]", ListToBulletedText(detenido.Ordenes));

                    ReplaceText(newSlide, "[C7]", detenido.Estatus);
                    ReplaceText(newSlide, "[C8]", detenido.DescripcionEstatus);

                    // Listas como viñetas de texto
                    //ReplaceText(newSlide, "[C9]", detenido.Asunto);
                    ReplaceText(newSlide, "[C9]", ListToBulletedText(detenido.Asunto));

                    // Reemplazar foto principal (placeholder en alt text: {{foto}})

                    ReplaceImageSafeNoDelete2(newSlide, "[FONDO]", fondo);

                    if (!string.IsNullOrEmpty(detenido.Foto) )
                    {
                        //ReplaceImageSafeNoDelete(newSlide, "[FP]", detenido.Foto);
                       
                        ReplaceImageSafeNoDelete2(newSlide, "[FP]", detenido.Foto, null,true);

                    }

                    // Victimas
                    for (int i = 0; i < detenido.Victimas.Count; i++)
                    {
                        int idx = i + 1;
                        string tagFoto = $"[FV{idx}]";
                        string tagNombre = $"[V{idx}]";

                        ReplaceText(newSlide, tagNombre, detenido.Victimas[i].Nombre ?? "");

                        if (!string.IsNullOrEmpty(detenido.Victimas[i].Foto) && File.Exists(detenido.Victimas[i].Foto))
                        {
                            //ReplaceImageByPlaceholder_NoEnum(newSlide, tagFoto, detenido.Victimas[i].Foto);
                            DebugSlidePartRelations(newSlide);
                            //ReplaceImageSafe(newSlide, tagFoto, detenido.Victimas[i].Foto);
                            ReplaceImageSafeNoDelete(newSlide, tagFoto, detenido.Victimas[i].Foto, defaultImg);

                        }
                    }

                    AddSlideToPresentation(presentationPart, newSlide);
                }

                if (eliminarPlantillaOriginal && plantillaSlideId != null)
                {
                    plantillaSlideId.Remove();
                }

                presentationPart.Presentation.Save();
            }
        }



        public static void GenerarPresentacion2(List<Detenido> detenidos, string plantillaPath, string salidaPath, string defaultImg, string fondo, bool eliminarPlantillaOriginal = true)
        {
            if (!File.Exists(plantillaPath))
                throw new FileNotFoundException("No se encontró la plantilla", plantillaPath);

            File.Copy(plantillaPath, salidaPath, true);

            using (var ppt = OpenXmlPackaging.PresentationDocument.Open(salidaPath, true))
            {
                var presentationPart = ppt.PresentationPart ?? throw new Exception("No PresentationPart");

                var slideTemplate = presentationPart.SlideParts.FirstOrDefault()
                    ?? throw new Exception("La plantilla no contiene diapositivas.");

                // Obtener SlideId de la plantilla para eliminar luego (opcional)
                string plantillaRelId = presentationPart.GetIdOfPart(slideTemplate);
                P.SlideId plantillaSlideId = presentationPart.Presentation.SlideIdList?
                    .ChildElements
                    .OfType<P.SlideId>()
                    .FirstOrDefault(s => s.RelationshipId == plantillaRelId);

                foreach (var detenido in detenidos)
                {
                    var newSlide = CloneSlidePart(presentationPart, slideTemplate);

                    // Reemplazo de textos
                    ReplaceText(newSlide, "[C1]", detenido.Nombre);
                    ReplaceText(newSlide, "[C2]", detenido.Cartel);
                    ReplaceText(newSlide, "[C3]", detenido.Ocupacion);


                    ReplaceText(newSlide, "[C4]", ListToBulletedText(detenido.Delitos));
                    ReplaceText(newSlide, "[C5]", ListToBulletedText(detenido.Carpetas));
                    ReplaceText(newSlide, "[C6]", ListToBulletedText(detenido.Ordenes));

                    ReplaceText(newSlide, "[C7]", detenido.Estatus);
                    ReplaceText(newSlide, "[C8]", detenido.DescripcionEstatus);

                    // Listas como viñetas de texto
                    //ReplaceText(newSlide, "[C9]", detenido.Asunto);
                    ReplaceText(newSlide, "[C9]", ListToBulletedText(detenido.Asunto));
                    
                    ReplaceText(newSlide, "[C10]", detenido.Alias);
                    ReplaceText(newSlide, "[C11]", detenido.FechaNacimiento);
                    ReplaceText(newSlide, "[C12]", detenido.Edad);


                    // Reemplazar foto principal (placeholder en alt text: {{foto}})

                    ReplaceImageSafeNoDelete2(newSlide, "[FONDO]", fondo);

                    if (!string.IsNullOrEmpty(detenido.Foto))
                    {
                        //ReplaceImageSafeNoDelete(newSlide, "[FP]", detenido.Foto);

                        ReplaceImageSafeNoDelete2(newSlide, "[FP]", detenido.Foto, null, true);

                    }

                    // Victimas
                    for (int i = 0; i < detenido.Victimas.Count; i++)
                    {
                        int idx = i + 1;
                        string tagFoto = $"[FV{idx}]";
                        string tagNombre = $"[V{idx}]";

                        ReplaceText(newSlide, tagNombre, detenido.Victimas[i].Nombre ?? "");

                        if (!string.IsNullOrEmpty(detenido.Victimas[i].Foto) && File.Exists(detenido.Victimas[i].Foto))
                        {
                            //ReplaceImageByPlaceholder_NoEnum(newSlide, tagFoto, detenido.Victimas[i].Foto);
                            DebugSlidePartRelations(newSlide);
                            //ReplaceImageSafe(newSlide, tagFoto, detenido.Victimas[i].Foto);
                            ReplaceImageSafeNoDelete(newSlide, tagFoto, detenido.Victimas[i].Foto, defaultImg);

                        }
                    }

                    AddSlideToPresentation(presentationPart, newSlide);
                }

                if (eliminarPlantillaOriginal && plantillaSlideId != null)
                {
                    plantillaSlideId.Remove();
                }

                presentationPart.Presentation.Save();
            }
        }

        #region Helpers

        private static string ListToBulletedText(List<string> list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0) sb.Append(list[i]);
                else sb.Append("\n• " + list[i]);
            }
            return sb.ToString();
        }

        private static OpenXmlPackaging.SlidePart CloneSlidePart(OpenXmlPackaging.PresentationPart presentationPart, OpenXmlPackaging.SlidePart source)
        {
            var newSlide = presentationPart.AddNewPart<OpenXmlPackaging.SlidePart>();
            using (var s = source.GetStream(FileMode.Open))
            using (var t = newSlide.GetStream(FileMode.Create))
            {
                s.CopyTo(t);
            }

            foreach (var p in source.Parts)
            {
                try
                {
                    var srcPart = p.OpenXmlPart;
                    newSlide.AddPart(srcPart);
                }
                catch
                {
                    // Ignorar partes que no se copien exactamente
                }
            }

            if (source.SlideLayoutPart != null && newSlide.SlideLayoutPart == null)
            {
                try { newSlide.AddPart(source.SlideLayoutPart); } catch { }
            }

            return newSlide;
        }

        private static void AddSlideToPresentation(OpenXmlPackaging.PresentationPart presentationPart, OpenXmlPackaging.SlidePart slidePart)
        {
            var slideIdList = presentationPart.Presentation.SlideIdList;
            if (slideIdList == null)
            {
                slideIdList = new P.SlideIdList();
                presentationPart.Presentation.Append(slideIdList);
            }

            uint maxId = 256;
            var ids = slideIdList.ChildElements.OfType<P.SlideId>().Select(s => s.Id.Value);
            if (ids.Any()) maxId = ids.Max();

            uint newId = maxId + 1;
            string rId = presentationPart.GetIdOfPart(slidePart);

            var newSlideId = new P.SlideId() { Id = newId, RelationshipId = rId };
            slideIdList.Append(newSlideId);
        }

        private static void ReplaceText(OpenXmlPackaging.SlidePart slidePart, string placeholder, string newValue)
        {
            if (placeholder == "[]")
            {
                var r = 0;
            }

            if (string.IsNullOrEmpty(placeholder)) return;
            newValue = newValue ?? "";

            var texts = slidePart.Slide.Descendants<A.Text>();
            foreach (var t in texts)
            {
                if (!string.IsNullOrEmpty(t.Text) && t.Text.Contains(placeholder))
                    t.Text = t.Text.Replace(placeholder, newValue);
            }

            if (slidePart.SlideLayoutPart != null)
            {
                foreach (var t in slidePart.SlideLayoutPart.SlideLayout.Descendants<A.Text>())
                {
                    if (!string.IsNullOrEmpty(t.Text) && t.Text.Contains(placeholder))
                        t.Text = t.Text.Replace(placeholder, newValue);
                }
            }

            var layout = slidePart.SlideLayoutPart;
            if (layout?.SlideMasterPart != null)
            {
                foreach (var t in layout.SlideMasterPart.SlideMaster.Descendants<A.Text>())
                {
                    if (!string.IsNullOrEmpty(t.Text) && t.Text.Contains(placeholder))
                        t.Text = t.Text.Replace(placeholder, newValue);
                }
            }
        }
        // Reemplazo robusto que trabaja por A.TextBody (formas, celdas de tabla, cuadros de texto)



        /// <summary>
        /// Reemplaza imagen usando reflexión para no depender del enum ImagePartType en tiempo de compilación.
        /// Busca Picture cuyo NonVisualDrawingProperties.Name o Description contenga el placeholder.
        /// </summary>
        /// 

        //private static void ReplaceImage(DocumentFormat.OpenXml.Packaging.SlidePart slidePart, string placeholder, string imagePath)
        //{
        //    if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        //        return;

        //    // Detectar tipo MIME según extensión (compatible con C# 7.3)
        //    string ext = Path.GetExtension(imagePath).ToLowerInvariant();
        //    string contentType = "image/jpeg"; // por defecto
        //    if (ext == ".png") contentType = "image/png";
        //    else if (ext == ".jpg" || ext == ".jpeg") contentType = "image/jpeg";
        //    else if (ext == ".gif") contentType = "image/gif";
        //    else if (ext == ".bmp") contentType = "image/bmp";
        //    else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";

        //    // Buscar todas las imágenes del slide
        //    var pics = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Presentation.Picture>().ToList();

        //    foreach (var pic in pics)
        //    {
        //        var nvPr = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
        //        if (nvPr == null) continue;

        //        // Convertimos StringValue a string con ToString()
        //        string name = nvPr.Name != null ? nvPr.Name.ToString().Trim() : "";
        //        string descr = nvPr.Description != null ? nvPr.Description.ToString().Trim() : "";

        //        if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
        //            string.Equals(descr, placeholder, StringComparison.OrdinalIgnoreCase))
        //        {
        //            var blip = pic.BlipFill != null ? pic.BlipFill.Blip : null;
        //            if (blip == null) continue;

        //            // 1️⃣ Eliminar relación anterior (si existe)
        //            if (blip.Embed != null && !string.IsNullOrEmpty(blip.Embed.Value))
        //            {
        //                var oldPart = slidePart.GetPartById(blip.Embed.Value);
        //                slidePart.DeletePart(oldPart);
        //            }

        //            // 2️⃣ Crear nueva imagen
        //            var imagePart = slidePart.AddNewPart<DocumentFormat.OpenXml.Packaging.ImagePart>(contentType);
        //            using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
        //            {
        //                imagePart.FeedData(stream);
        //            }

        //            // 3️⃣ Asignar el nuevo ID al blip
        //            string relId = slidePart.GetIdOfPart(imagePart);
        //            blip.Embed.Value = relId;

        //            // 4️⃣ Opcional: marcar que se reemplazó
        //            nvPr.Description = placeholder + "_ok";

        //            break;
        //        }
        //    }
        //}

        // Llama a esta función con el slidePart y verás la lista por consola / logs
        private static void DebugSlidePartRelations(OpenXmlPackaging.SlidePart slidePart)
        {
            Console.WriteLine("=== SlidePart Relations ===");
            foreach (var p in slidePart.Parts)
            {
                Console.WriteLine("SlidePart.RelId: {0} - Type: {1}", p.RelationshipId, p.OpenXmlPart.GetType().FullName);
            }

            if (slidePart.SlideLayoutPart != null)
            {
                Console.WriteLine("=== SlideLayoutPart Relations ===");
                foreach (var p in slidePart.SlideLayoutPart.Parts)
                {
                    Console.WriteLine("LayoutPart.RelId: {0} - Type: {1}", p.RelationshipId, p.OpenXmlPart.GetType().FullName);
                }
            }

            var layout = slidePart.SlideLayoutPart;
            if (layout?.SlideMasterPart != null)
            {
                Console.WriteLine("=== SlideMasterPart Relations ===");
                foreach (var p in layout.SlideMasterPart.Parts)
                {
                    Console.WriteLine("MasterPart.RelId: {0} - Type: {1}", p.RelationshipId, p.OpenXmlPart.GetType().FullName);
                }
            }
        }


        //private static void ReplaceImageSafe(OpenXmlPackaging.SlidePart slidePart, string placeholder, string imagePath)
        //{
        //    if (string.IsNullOrEmpty(placeholder) || !File.Exists(imagePath)) return;

        //    // Determinar contentType simple compatible C# 7.3
        //    string ext = Path.GetExtension(imagePath).ToLowerInvariant();
        //    string contentType = "image/jpeg";
        //    if (ext == ".png") contentType = "image/png";
        //    else if (ext == ".jpg" || ext == ".jpeg") contentType = "image/jpeg";
        //    else if (ext == ".gif") contentType = "image/gif";
        //    else if (ext == ".bmp") contentType = "image/bmp";
        //    else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";

        //    // Buscar picture que tenga Name o Description igual al placeholder
        //    P.Picture foundPic = null;
        //    foreach (var pic in slidePart.Slide.Descendants<P.Picture>())
        //    {
        //        var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
        //        if (nv == null) continue;
        //        string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
        //        string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
        //        if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
        //            string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
        //        {
        //            foundPic = pic;
        //            break;
        //        }
        //    }

        //    // Si no está en el slide, buscar en layout (si existe)
        //    if (foundPic == null && slidePart.SlideLayoutPart != null)
        //    {
        //        foreach (var pic in slidePart.SlideLayoutPart.SlideLayout.Descendants<P.Picture>())
        //        {
        //            var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
        //            if (nv == null) continue;
        //            string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
        //            string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
        //            if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
        //                string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
        //            {
        //                foundPic = pic;
        //                break;
        //            }
        //        }
        //    }

        //    // Si aún no lo encontramos, intentar en SlideMaster
        //    var layout = slidePart.SlideLayoutPart;
        //    if (foundPic == null && layout?.SlideMasterPart != null)
        //    {
        //        foreach (var pic in layout.SlideMasterPart.SlideMaster.Descendants<P.Picture>())
        //        {
        //            var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
        //            if (nv == null) continue;
        //            string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
        //            string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
        //            if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
        //                string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
        //            {
        //                foundPic = pic;
        //                break;
        //            }
        //        }
        //    }

        //    if (foundPic == null) return; // no hay placeholder de imagen

        //    var blip = foundPic.BlipFill != null ? foundPic.BlipFill.Blip : null;
        //    if (blip == null) return;

        //    // Si blip tiene Link (vinculada externamente), limpiar para evitar referencias externas
        //    if (blip.Link != null && !string.IsNullOrEmpty(blip.Link.Value))
        //    {
        //        try { blip.Link.Value = null; } catch { }
        //    }

        //    // Intentar eliminar parte antigua SI la relación existe en slidePart.Parts
        //    string embedId = blip.Embed != null ? blip.Embed.Value : null;
        //    if (!string.IsNullOrEmpty(embedId))
        //    {
        //        // Buscar en SlidePart.Parts por RelationshipId
        //        var partPair = slidePart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
        //        if (partPair != null)
        //        {
        //            try { slidePart.DeletePart(partPair.OpenXmlPart); }
        //            catch { /* ignorar fallo en borrado */ }
        //        }
        //        else
        //        {
        //            // No está en slidePart.Parts: intentar buscar en SlideLayoutPart y SlideMasterPart
        //            bool deleted = false;
        //            if (slidePart.SlideLayoutPart != null)
        //            {
        //                var layoutPair = slidePart.SlideLayoutPart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
        //                if (layoutPair != null)
        //                {
        //                    try { slidePart.SlideLayoutPart.DeletePart(layoutPair.OpenXmlPart); deleted = true; }
        //                    catch { deleted = false; }
        //                }
        //            }

        //            if (!deleted && slidePart.SlideLayoutPart?.SlideMasterPart != null)
        //            {
        //                var masterPair = slidePart.SlideLayoutPart.SlideMasterPart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
        //                if (masterPair != null)
        //                {
        //                    try { slidePart.SlideLayoutPart.SlideMasterPart.DeletePart(masterPair.OpenXmlPart); deleted = true; }
        //                    catch { deleted = false; }
        //                }
        //            }

        //            // Si no se encontró la relación en ninguna parte, continuamos (no queremos fallar)
        //        }
        //    }

        //    // Crear nueva ImagePart en el SlidePart (AddNewPart con contentType)
        //    OpenXmlPackaging.ImagePart imagePart = null;
        //    try
        //    {
        //        imagePart = slidePart.AddNewPart<OpenXmlPackaging.ImagePart>(contentType);
        //        using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
        //        {
        //            imagePart.FeedData(fs);
        //        }
        //    }
        //    catch
        //    {
        //        // Si falla la creación, abortamos silenciosamente
        //        return;
        //    }

        //    // Obtener relationship id y asignarlo al blip
        //    try
        //    {
        //        string newRelId = slidePart.GetIdOfPart(imagePart);
        //        if (blip.Embed == null)
        //            blip.Embed = new DocumentFormat.OpenXml.StringValue(newRelId);
        //        else
        //            blip.Embed.Value = newRelId;

        //        // Opcional: marcar en description que se reemplazó
        //        var nvProps = foundPic.NonVisualPictureProperties != null ? foundPic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
        //        if (nvProps != null)
        //        {
        //            nvProps.Description = (placeholder ?? "") + "_ok";
        //        }
        //    }
        //    catch
        //    {
        //        // si falla, no hacemos crash
        //    }
        //}

        private static void ReplaceImageWithFallback(OpenXmlPackaging.SlidePart slidePart, string placeholder, string imagePath, string defaultImagePath = null)
        {
            // Si no existe el placeholder o no hay slidepart, salir
            if (slidePart == null || string.IsNullOrEmpty(placeholder)) return;

            // Buscar la picture que tenga Name/Description == placeholder
            P.Picture foundPic = null;
            foreach (var pic in slidePart.Slide.Descendants<P.Picture>())
            {
                var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
                if (nv == null) continue;
                string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
                string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
                if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
                {
                    foundPic = pic;
                    break;
                }
            }

            // Si no está en el slide, buscar en layout y master también (opcional)
            if (foundPic == null && slidePart.SlideLayoutPart != null)
            {
                foreach (var pic in slidePart.SlideLayoutPart.SlideLayout.Descendants<P.Picture>())
                {
                    var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
                    if (nv == null) continue;
                    string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
                    string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
                    if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
                    {
                        foundPic = pic;
                        break;
                    }
                }
            }
            var layout = slidePart.SlideLayoutPart;
            if (foundPic == null && layout?.SlideMasterPart != null)
            {
                foreach (var pic in layout.SlideMasterPart.SlideMaster.Descendants<P.Picture>())
                {
                    var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
                    if (nv == null) continue;
                    string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
                    string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
                    if (string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase))
                    {
                        foundPic = pic;
                        break;
                    }
                }
            }

            if (foundPic == null) return; // no hay placeholder de imagen

            var blip = foundPic.BlipFill != null ? foundPic.BlipFill.Blip : null;
            if (blip == null) return;

            // Decide qué archivo usar: la imagen real si existe, sino la default si existe, sino no tocar nada.
            string fileToUse = null;
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                fileToUse = imagePath;
            else if (!string.IsNullOrEmpty(defaultImagePath) && File.Exists(defaultImagePath))
                fileToUse = defaultImagePath;
            else
            {
                // No hay imagen y no hay fallback: NO eliminar relación ni tocar el blip.
                // Esto mantiene la imagen "placeholder" que pusiste manualmente en la plantilla.
                return;
            }

            // determinar contentType por extensión simple
            string ext = Path.GetExtension(fileToUse).ToLowerInvariant();
            string contentType = "image/jpeg";
            if (ext == ".png") contentType = "image/png";
            else if (ext == ".gif") contentType = "image/gif";
            else if (ext == ".bmp") contentType = "image/bmp";
            else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";

            // Si blip tiene Link (vínculo externo) limpiar para evitar referencias rotas
            if (blip.Link != null && !string.IsNullOrEmpty(blip.Link.Value))
            {
                try { blip.Link.Value = null; } catch { }
            }

            // Eliminar parte antigua solo si realmente existe como relationship en slide/layout/master
            string embedId = blip.Embed != null ? blip.Embed.Value : null;
            if (!string.IsNullOrEmpty(embedId))
            {
                // Buscar en SlidePart
                var partPair = slidePart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
                if (partPair != null)
                {
                    try { slidePart.DeletePart(partPair.OpenXmlPart); }
                    catch { /* ignorar error de borrado */ }
                }
                else
                {
                    // Intentar layout/master (no siempre recomendable borrar partes del master, pero lo intentamos con cuidado)
                    bool deleted = false;
                    if (slidePart.SlideLayoutPart != null)
                    {
                        var layoutPair = slidePart.SlideLayoutPart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
                        if (layoutPair != null)
                        {
                            try { slidePart.SlideLayoutPart.DeletePart(layoutPair.OpenXmlPart); deleted = true; }
                            catch { deleted = false; }
                        }
                    }
                    if (!deleted && slidePart.SlideLayoutPart?.SlideMasterPart != null)
                    {
                        var masterPair = slidePart.SlideLayoutPart.SlideMasterPart.Parts.FirstOrDefault(pp => string.Equals(pp.RelationshipId, embedId, StringComparison.OrdinalIgnoreCase));
                        if (masterPair != null)
                        {
                            try { slidePart.SlideLayoutPart.SlideMasterPart.DeletePart(masterPair.OpenXmlPart); deleted = true; }
                            catch { deleted = false; }
                        }
                    }
                    // si no se encontró, no hacer nada (no morir)
                }
            }

            // Crear nuevo ImagePart y alimentar datos
            OpenXmlPackaging.ImagePart newImgPart = null;
            try
            {
                newImgPart = slidePart.AddNewPart<OpenXmlPackaging.ImagePart>(contentType);
                using (var fs = new FileStream(fileToUse, FileMode.Open, FileAccess.Read))
                {
                    newImgPart.FeedData(fs);
                }
            }
            catch
            {
                // no se pudo crear la parte: salir sin tocar el blip (para no dejar rId inválido)
                return;
            }

            // Obtener id y asignarlo
            try
            {
                string newRelId = slidePart.GetIdOfPart(newImgPart);
                if (blip.Embed == null)
                    blip.Embed = new DocumentFormat.OpenXml.StringValue(newRelId);
                else
                    blip.Embed.Value = newRelId;

                // Opcional: actualizar description para indicar éxito
                var nvProps = foundPic.NonVisualPictureProperties != null ? foundPic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
                if (nvProps != null)
                {
                    nvProps.Description = (placeholder ?? "") + "_ok";
                }
            }
            catch
            {
                // si algo falla al asignar el id, no hacemos crash
            }
        }




        private static void ReplaceImageSafeNoDelete(OpenXmlPackaging.SlidePart slidePart, string placeholder, string imagePath, string defaultImagePath = null)
        {
            if (slidePart == null || string.IsNullOrEmpty(placeholder)) return;

            // Encontrar picture por Name o Description en Slide, Layout o Master
            P.Picture foundPic = null;
            Func<P.Picture, bool> matches = pic =>
            {
                var nv = pic.NonVisualPictureProperties != null ? pic.NonVisualPictureProperties.NonVisualDrawingProperties : null;
                if (nv == null) return false;
                string name = nv.Name != null ? nv.Name.ToString().Trim() : "";
                string desc = nv.Description != null ? nv.Description.ToString().Trim() : "";
                return string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase);
            };

            foreach (var pic in slidePart.Slide.Descendants<P.Picture>())
                if (matches(pic)) { foundPic = pic; break; }

            if (foundPic == null && slidePart.SlideLayoutPart != null)
            {
                foreach (var pic in slidePart.SlideLayoutPart.SlideLayout.Descendants<P.Picture>())
                    if (matches(pic)) { foundPic = pic; break; }
            }

            var layout = slidePart.SlideLayoutPart;
            if (foundPic == null && layout?.SlideMasterPart != null)
            {
                foreach (var pic in layout.SlideMasterPart.SlideMaster.Descendants<P.Picture>())
                    if (matches(pic)) { foundPic = pic; break; }
            }

            if (foundPic == null) return;

            var blip = foundPic.BlipFill != null ? foundPic.BlipFill.Blip : null;
            if (blip == null) return;

            // Seleccionar el archivo a usar
            string fileToUse = null;
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath)) fileToUse = imagePath;
            else if (!string.IsNullOrEmpty(defaultImagePath) && File.Exists(defaultImagePath)) fileToUse = defaultImagePath;
            else
            {
                // No hay nada que hacer: dejamos el placeholder tal cual (evita error si ya estaba correcto)
                return;
            }

            // contentType básico
            string ext = Path.GetExtension(fileToUse).ToLowerInvariant();
            string contentType = "image/jpeg";
            if (ext == ".png") contentType = "image/png";
            else if (ext == ".gif") contentType = "image/gif";
            else if (ext == ".bmp") contentType = "image/bmp";
            else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";

            // Limpiar Link si existiera (evita vínculos rotos)
            if (blip.Link != null && !string.IsNullOrEmpty(blip.Link.Value))
            {
                try { blip.Link.Value = null; } catch { }
            }

            // Aquí NO borramos la parte anterior. Creamos una nueva imagePart en el slidePart y apuntamos el blip a ella.
            OpenXmlPackaging.ImagePart newImg = null;
            try
            {
                newImg = slidePart.AddNewPart<OpenXmlPackaging.ImagePart>(contentType);
                using (var fs = new FileStream(fileToUse, FileMode.Open, FileAccess.Read))
                    newImg.FeedData(fs);
            }
            catch
            {
                return;
            }

            // Asociar nuevo relId al blip (esto es seguro y no rompe otras referencias)
            try
            {
                string newRelId = slidePart.GetIdOfPart(newImg);
                if (blip.Embed == null) blip.Embed = new DocumentFormat.OpenXml.StringValue(newRelId);
                else blip.Embed.Value = newRelId;
            }
            catch
            {
                // swallow
            }
        }

        private static void ReplaceImageSafeNoDelete2(
    OpenXmlPackaging.SlidePart slidePart,
    string placeholder,
    string imagePathOrBase64,
    string defaultImagePath = null,
    bool isBase64 = false)
        {
            if (slidePart == null || string.IsNullOrEmpty(placeholder)) return;

            // Buscar la imagen en el slide, layout o master
            P.Picture foundPic = null;
            Func<P.Picture, bool> matches = pic =>
            {
                var nv = pic.NonVisualPictureProperties?.NonVisualDrawingProperties;
                if (nv == null) return false;
                string name = nv.Name?.Value?.Trim() ?? "";
                string desc = nv.Description?.Value?.Trim() ?? "";
                return string.Equals(name, placeholder, StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(desc, placeholder, StringComparison.OrdinalIgnoreCase);
            };

            foreach (var pic in slidePart.Slide.Descendants<P.Picture>())
                if (matches(pic)) { foundPic = pic; break; }

            if (foundPic == null && slidePart.SlideLayoutPart != null)
            {
                foreach (var pic in slidePart.SlideLayoutPart.SlideLayout.Descendants<P.Picture>())
                    if (matches(pic)) { foundPic = pic; break; }
            }

            var layout = slidePart.SlideLayoutPart;
            if (foundPic == null && layout?.SlideMasterPart != null)
            {
                foreach (var pic in layout.SlideMasterPart.SlideMaster.Descendants<P.Picture>())
                    if (matches(pic)) { foundPic = pic; break; }
            }

            if (foundPic == null) return;

            var blip = foundPic.BlipFill?.Blip;
            if (blip == null) return;

            // Seleccionar el origen de la imagen
            byte[] imageBytes = null;
            string contentType = "image/jpeg"; // valor por defecto

            try
            {
                if (isBase64 && !string.IsNullOrEmpty(imagePathOrBase64))
                {
                    // Si la cadena tiene encabezado tipo "data:image/png;base64,...", lo limpiamos
                    var base64Data = imagePathOrBase64.Contains(",")
                        ? imagePathOrBase64.Substring(imagePathOrBase64.IndexOf(",") + 1)
                        : imagePathOrBase64;

                    imageBytes = Convert.FromBase64String(base64Data);

                    // Intentar inferir tipo MIME (opcional)
                    if (imagePathOrBase64.Contains("image/png")) contentType = "image/png";
                    else if (imagePathOrBase64.Contains("image/gif")) contentType = "image/gif";
                    else if (imagePathOrBase64.Contains("image/bmp")) contentType = "image/bmp";
                    else if (imagePathOrBase64.Contains("image/tiff")) contentType = "image/tiff";
                }
                else if (!string.IsNullOrEmpty(imagePathOrBase64) && File.Exists(imagePathOrBase64))
                {
                    imageBytes = File.ReadAllBytes(imagePathOrBase64);
                    string ext = Path.GetExtension(imagePathOrBase64).ToLowerInvariant();
                    if (ext == ".png") contentType = "image/png";
                    else if (ext == ".gif") contentType = "image/gif";
                    else if (ext == ".bmp") contentType = "image/bmp";
                    else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";
                }
                else if (!string.IsNullOrEmpty(defaultImagePath) && File.Exists(defaultImagePath))
                {
                    imageBytes = File.ReadAllBytes(defaultImagePath);
                    string ext = Path.GetExtension(defaultImagePath).ToLowerInvariant();
                    if (ext == ".png") contentType = "image/png";
                    else if (ext == ".gif") contentType = "image/gif";
                    else if (ext == ".bmp") contentType = "image/bmp";
                    else if (ext == ".tif" || ext == ".tiff") contentType = "image/tiff";
                }
                else
                {
                    return; // No hay imagen disponible
                }
            }
            catch
            {
                return;
            }

            // Limpiar link si existe
            if (blip.Link != null && !string.IsNullOrEmpty(blip.Link.Value))
            {
                try { blip.Link.Value = null; } catch { }
            }

            // Crear una nueva ImagePart y asignarle la imagen
            OpenXmlPackaging.ImagePart newImg = null;
            try
            {
                newImg = slidePart.AddNewPart<OpenXmlPackaging.ImagePart>(contentType);
                using (var ms = new MemoryStream(imageBytes))
                    newImg.FeedData(ms);
            }
            catch
            {
                return;
            }

            // Asociar el nuevo relId al blip
            try
            {
                string newRelId = slidePart.GetIdOfPart(newImg);
                if (blip.Embed == null)
                    blip.Embed = new DocumentFormat.OpenXml.StringValue(newRelId);
                else
                    blip.Embed.Value = newRelId;
            }
            catch
            {
                // Ignorar error silenciosamente
            }
        }




        private static void ReplaceImageByPlaceholder_NoEnum(OpenXmlPackaging.SlidePart slidePart, string placeholder, string imagePath)
        {
            if (string.IsNullOrEmpty(placeholder) || !File.Exists(imagePath)) return;

            P.Picture foundPic = null;

            foreach (var pic in slidePart.Slide.Descendants<P.Picture>())
            {
                var nv = pic.NonVisualPictureProperties?.NonVisualDrawingProperties;
                if (nv != null)
                {
                    string name = nv.Name ?? "";
                    string desc = nv.Description ?? "";
                    if ((name.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (desc.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        foundPic = pic;
                        break;
                    }
                }
            }

            if (foundPic == null && slidePart.SlideLayoutPart != null)
            {
                foreach (var pic in slidePart.SlideLayoutPart.SlideLayout.Descendants<P.Picture>())
                {
                    var nv = pic.NonVisualPictureProperties?.NonVisualDrawingProperties;
                    if (nv != null)
                    {
                        string name = nv.Name ?? "";
                        string desc = nv.Description ?? "";
                        if ((name.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (desc.IndexOf(placeholder, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            foundPic = pic;
                            break;
                        }
                    }
                }
            }

            if (foundPic == null) return;

            var blip = foundPic.BlipFill?.Blip;
            if (blip == null) return;

            // 1) intentar AddImagePart(string contentType) mediante reflexión
            string contentType = GetMimeTypeFromExtension(imagePath); // e.g. "image/png"
            object imagePartObj = null;

            MethodInfo addImagePartByString = slidePart.GetType().GetMethod("AddImagePart", new Type[] { typeof(string) });
            if (addImagePartByString != null)
            {
                // Invocar AddImagePart(string contentType)
                imagePartObj = addImagePartByString.Invoke(slidePart, new object[] { contentType });
            }
            else
            {
                // 2) intentar AddImagePart(enum ImagePartType) mediante reflexión: construimos el enum por su nombre
                // Buscamos el tipo enum por nombre completo (assembly DocumentFormat.OpenXml)
                Type enumType = Type.GetType("DocumentFormat.OpenXml.Packaging.ImagePartType, DocumentFormat.OpenXml");
                if (enumType != null)
                {
                    // Mapear extensión a nombre de enum (Png, Jpeg, Gif, Bmp, Tiff, Wmf, Emf)
                    string enumName = GetEnumNameForExtension(imagePath);
                    if (!string.IsNullOrEmpty(enumName))
                    {
                        try
                        {
                            object enumVal = Enum.Parse(enumType, enumName, true);
                            MethodInfo addImagePartByEnum = slidePart.GetType().GetMethod("AddImagePart", new Type[] { enumType });
                            if (addImagePartByEnum != null)
                            {
                                imagePartObj = addImagePartByEnum.Invoke(slidePart, new object[] { enumVal });
                            }
                        }
                        catch
                        {
                            // ignore parse/invoke errors y seguir a fallback
                        }
                    }
                }
            }

            // 3) Si no conseguimos imagePartObj, intentar AddNewPart<ImagePart>(string contentType) por reflexión
            if (imagePartObj == null)
            {
                try
                {
                    // Buscar método genérico AddNewPart<T>(string) y crear un ImagePart mediante reflexión
                    var methods = slidePart.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    MethodInfo addNewPartGeneric = methods.FirstOrDefault(m => m.Name == "AddNewPart" && m.IsGenericMethod && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));
                    if (addNewPartGeneric != null)
                    {
                        // Obtener el tipo OpenXmlPackaging.ImagePart desde el ensamblado
                        Type imagePartType = Type.GetType("DocumentFormat.OpenXml.Packaging.ImagePart, DocumentFormat.OpenXml");
                        if (imagePartType != null)
                        {
                            MethodInfo generic = addNewPartGeneric.MakeGenericMethod(imagePartType);
                            imagePartObj = generic.Invoke(slidePart, new object[] { contentType });
                        }
                    }
                }
                catch
                {
                    // si falla, seguimos (no hacemos nada)
                }
            }

            if (imagePartObj == null) return; // no pudimos crear ImagePart

            // FeedData en el imagePartObj
            MethodInfo feedDataMethod = imagePartObj.GetType().GetMethod("FeedData", new Type[] { typeof(Stream) });
            if (feedDataMethod == null) return;

            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                feedDataMethod.Invoke(imagePartObj, new object[] { fs });
            }

            // Obtener relationship id y asignar al blip
            MethodInfo getIdMethod = slidePart.GetType().GetMethod("GetIdOfPart", new Type[] { imagePartObj.GetType() });
            string relId = null;
            if (getIdMethod != null)
            {
                relId = getIdMethod.Invoke(slidePart, new object[] { imagePartObj }) as string;
            }
            else
            {
                // fallback: intentar GetIdOfPart(object) more generic
                var getIdGeneric = slidePart.GetType().GetMethods().FirstOrDefault(m => m.Name == "GetIdOfPart" && m.GetParameters().Length == 1);
                if (getIdGeneric != null)
                {
                    relId = getIdGeneric.Invoke(slidePart, new object[] { imagePartObj }) as string;
                }
            }

            if (string.IsNullOrEmpty(relId)) return;

            if (blip.Embed == null)
                blip.Embed = relId;
            else
                blip.Embed.Value = relId;
        }

        private static string GetMimeTypeFromExtension(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".tif":
                case ".tiff": return "image/tiff";
                case ".wmf": return "image/wmf";
                case ".emf": return "image/emf";
                default: return "image/jpeg";
            }
        }

        // Retorna el nombre del enum (Png, Jpeg, Gif, Bmp, Tiff, Wmf, Emf) para usar con Enum.Parse si es necesario.
        private static string GetEnumNameForExtension(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            switch (ext)
            {
                case ".png": return "Png";
                case ".jpg":
                case ".jpeg": return "Jpeg";
                case ".gif": return "Gif";
                case ".bmp": return "Bmp";
                case ".tif":
                case ".tiff": return "Tiff";
                case ".wmf": return "Wmf";
                case ".emf": return "Emf";
                default: return "Jpeg";
            }
        }

        #endregion
    }
}