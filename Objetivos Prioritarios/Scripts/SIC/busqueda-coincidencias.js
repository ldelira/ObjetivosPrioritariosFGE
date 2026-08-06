(function ($) {
    'use strict';

    /* ============================================================
       CONFIGURACIÓN GENERAL
       ============================================================ */

    const tiposPermitidos = [
        'image/jpeg',
        'image/jpg',
        'image/png',
        'image/webp'
    ];

    let solicitudBusquedaActual = null;
    let solicitudDetalleActual = null;
    let buscandoCoincidencias = false;


    /* ============================================================
       INICIALIZACIÓN
       ============================================================ */

    $(document).ready(function () {

        inicializarCargaBiometrica();
        inicializarPorcentaje();
        inicializarFormulario();
        inicializarLimpieza();
        inicializarEventosResultados();

        actualizarEstadoBotonBusqueda();
    });


    /* ============================================================
       CARGA DE FOTOGRAFÍA Y HUELLA
       ============================================================ */

    function inicializarCargaBiometrica() {

        $(document)
            .off(
                'change.sicCoincidencias',
                '.js-biometrico-input'
            )
            .on(
                'change.sicCoincidencias',
                '.js-biometrico-input',
                function () {

                    procesarArchivoSeleccionado(this);
                }
            );


        $(document)
            .off(
                'click.sicCoincidencias',
                '.js-seleccionar-biometrico'
            )
            .on(
                'click.sicCoincidencias',
                '.js-seleccionar-biometrico',
                function () {

                    const inputId =
                        $(this).attr('data-input-id');

                    if (!inputId) {
                        return;
                    }

                    $('#' + inputId).trigger('click');
                }
            );


        $(document)
            .off(
                'click.sicCoincidencias',
                '.js-quitar-biometrico'
            )
            .on(
                'click.sicCoincidencias',
                '.js-quitar-biometrico',
                function () {

                    const inputId =
                        $(this).attr('data-input-id');

                    limpiarArchivoBiometrico(inputId);
                }
            );


        $('.sic-dropzone')
            .off('.sicCoincidenciasDrop')
            .on(
                'dragenter.sicCoincidenciasDrop ' +
                'dragover.sicCoincidenciasDrop',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    $(this)
                        .addClass('sic-dropzone-activa');
                }
            )
            .on(
                'dragleave.sicCoincidenciasDrop ' +
                'drop.sicCoincidenciasDrop',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    $(this)
                        .removeClass('sic-dropzone-activa');
                }
            )
            .on(
                'drop.sicCoincidenciasDrop',
                function (evento) {

                    const eventoOriginal =
                        evento.originalEvent;

                    if (
                        !eventoOriginal ||
                        !eventoOriginal.dataTransfer
                    ) {
                        return;
                    }

                    const archivos =
                        eventoOriginal.dataTransfer.files;

                    if (
                        !archivos ||
                        archivos.length === 0
                    ) {
                        return;
                    }

                    const inputId =
                        $(this).attr('data-input-id');

                    const input =
                        document.getElementById(inputId);

                    asignarArchivoAInput(
                        input,
                        archivos[0]
                    );
                }
            );
    }


    function asignarArchivoAInput(
        input,
        archivo
    ) {

        if (!input || !archivo) {
            return;
        }

        try {

            const transferencia =
                new DataTransfer();

            transferencia.items.add(archivo);

            input.files =
                transferencia.files;

            procesarArchivoSeleccionado(input);

        } catch (error) {

            mostrarMensaje(
                'No fue posible cargar el archivo arrastrado. Utilice el botón Seleccionar.',
                'error'
            );
        }
    }


    function procesarArchivoSeleccionado(
        input
    ) {

        if (
            !input ||
            !input.files ||
            input.files.length === 0
        ) {

            limpiarVistaPrevia(input);
            actualizarEstadoBotonBusqueda();

            return;
        }

        const archivo =
            input.files[0];

        if (!archivoEsValido(archivo)) {

            input.value = '';

            limpiarVistaPrevia(input);
            actualizarEstadoBotonBusqueda();

            return;
        }


        const previewId =
            $(input).attr('data-preview');

        const estadoVacioId =
            $(input).attr('data-vacio');

        const quitarId =
            $(input).attr('data-quitar');


        const lector =
            new FileReader();


        lector.onload = function (evento) {

            $('#' + previewId)
                .attr(
                    'src',
                    evento.target.result
                )
                .show();

            $('#' + estadoVacioId)
                .hide();

            $('#' + quitarId)
                .prop(
                    'disabled',
                    false
                );

            $(input)
                .closest('.sic-carga-card')
                .addClass(
                    'sic-carga-card-con-archivo'
                );

            actualizarEstadoBotonBusqueda();
        };


        lector.onerror = function () {

            input.value = '';

            limpiarVistaPrevia(input);

            actualizarEstadoBotonBusqueda();

            mostrarMensaje(
                'No fue posible leer el archivo seleccionado.',
                'error'
            );
        };


        lector.readAsDataURL(archivo);
    }


    function archivoEsValido(
        archivo
    ) {

        if (!archivo) {
            return false;
        }


        const nombreArchivo =
            archivo.name
                ? archivo.name.toLowerCase()
                : '';

        const tipoArchivo =
            archivo.type
                ? archivo.type.toLowerCase()
                : '';


        const extensionValida =
            /\.(jpg|jpeg|png|webp)$/i
                .test(nombreArchivo);

        const mimeValido =
            tipoArchivo === '' ||
            tiposPermitidos.indexOf(
                tipoArchivo
            ) !== -1;


        if (!extensionValida || !mimeValido) {

            mostrarMensaje(
                'Seleccione una imagen JPG, PNG o WEBP.',
                'warning'
            );

            return false;
        }


        const configuracion =
            window.sicCoincidenciasConfig || {};

        const maximoMb =
            configuracion.maximoArchivoMb || 10;

        const maximoBytes =
            maximoMb * 1024 * 1024;


        if (archivo.size > maximoBytes) {

            mostrarMensaje(
                'El archivo supera el límite de ' +
                maximoMb +
                ' MB.',
                'warning'
            );

            return false;
        }


        if (archivo.size <= 0) {

            mostrarMensaje(
                'El archivo seleccionado está vacío.',
                'warning'
            );

            return false;
        }


        return true;
    }


    function limpiarArchivoBiometrico(
        inputId
    ) {

        if (!inputId) {
            return;
        }

        const input =
            document.getElementById(inputId);

        if (!input) {
            return;
        }

        input.value = '';

        limpiarVistaPrevia(input);
        actualizarEstadoBotonBusqueda();
    }


    function limpiarVistaPrevia(
        input
    ) {

        if (!input) {
            return;
        }


        const previewId =
            $(input).attr('data-preview');

        const estadoVacioId =
            $(input).attr('data-vacio');

        const quitarId =
            $(input).attr('data-quitar');


        $('#' + previewId)
            .attr('src', '')
            .hide();

        $('#' + estadoVacioId)
            .show();

        $('#' + quitarId)
            .prop(
                'disabled',
                true
            );

        $(input)
            .closest('.sic-carga-card')
            .removeClass(
                'sic-carga-card-con-archivo'
            );
    }


    /* ============================================================
       CONTROL DEL PORCENTAJE
       ============================================================ */

    function inicializarPorcentaje() {

        $('#PorcentajeMinimo')
            .off('input.sicCoincidencias')
            .on(
                'input.sicCoincidencias',
                function () {

                    const valor =
                        $(this).val() || 70;

                    $('#valorPorcentajeMinimo')
                        .text(valor + '%');
                }
            );
    }


    /* ============================================================
       FORMULARIO DE BÚSQUEDA
       ============================================================ */

    function inicializarFormulario() {

        $('#formBusquedaCoincidencias')
            .off('submit.sicCoincidencias')
            .on(
                'submit.sicCoincidencias',
                function (evento) {

                    evento.preventDefault();

                    if (!validarFormularioBusqueda()) {
                        return;
                    }

                    buscarCoincidencias();
                }
            );
    }


    function validarFormularioBusqueda() {

        const tieneFotografia =
            inputTieneArchivo('Fotografia');

        const tieneHuella =
            inputTieneArchivo('Huella');


        if (!tieneFotografia && !tieneHuella) {

            mostrarMensaje(
                'Seleccione una fotografía, una huella o ambos archivos.',
                'warning'
            );

            return false;
        }


        const edadMinima =
            parseInt(
                $('#EdadMinima').val(),
                10
            );

        const edadMaxima =
            parseInt(
                $('#EdadMaxima').val(),
                10
            );


        if (
            isNaN(edadMinima) ||
            edadMinima < 0 ||
            edadMinima > 120
        ) {

            mostrarMensaje(
                'La edad mínima debe estar entre 0 y 120 años.',
                'warning'
            );

            $('#EdadMinima').focus();

            return false;
        }


        if (
            isNaN(edadMaxima) ||
            edadMaxima < 0 ||
            edadMaxima > 120
        ) {

            mostrarMensaje(
                'La edad máxima debe estar entre 0 y 120 años.',
                'warning'
            );

            $('#EdadMaxima').focus();

            return false;
        }


        if (edadMinima > edadMaxima) {

            mostrarMensaje(
                'La edad mínima no puede ser mayor que la edad máxima.',
                'warning'
            );

            $('#EdadMinima').focus();

            return false;
        }


        const porcentaje =
            parseInt(
                $('#PorcentajeMinimo').val(),
                10
            );


        if (
            isNaN(porcentaje) ||
            porcentaje < 50 ||
            porcentaje > 100
        ) {

            mostrarMensaje(
                'El porcentaje mínimo debe estar entre 50% y 100%.',
                'warning'
            );

            return false;
        }


        return true;
    }


    function buscarCoincidencias() {

        if (buscandoCoincidencias) {
            return;
        }


        const formulario =
            document.getElementById(
                'formBusquedaCoincidencias'
            );

        if (!formulario) {

            mostrarMensaje(
                'No se encontró el formulario de búsqueda.',
                'error'
            );

            return;
        }


        const configuracion =
            window.sicCoincidenciasConfig || {};

        if (!configuracion.urlBuscar) {

            mostrarMensaje(
                'No se configuró la dirección para buscar coincidencias.',
                'error'
            );

            return;
        }


        if (
            solicitudBusquedaActual &&
            solicitudBusquedaActual.readyState !== 4
        ) {
            solicitudBusquedaActual.abort();
        }


        const datos =
            new FormData(formulario);


        buscandoCoincidencias = true;

        solicitudBusquedaActual =
            $.ajax({

                url:
                    configuracion.urlBuscar,

                type:
                    'POST',

                data:
                    datos,

                processData:
                    false,

                contentType:
                    false,

                cache:
                    false,


                beforeSend: function () {

                    mostrarCargandoResultados();

                    $('#btnBuscarCoincidencias')
                        .prop(
                            'disabled',
                            true
                        )
                        .html(
                            '<i class="fa fa-spinner fa-spin"></i> ' +
                            'Buscando...'
                        );
                },


                success: function (html) {

                    $('#contenedorResultadosCoincidencias')
                        .removeClass(
                            'sic-resultados-inicial'
                        )
                        .html(html);

                    sincronizarImagenesConsulta();
                },


                error: function (
                    xhr,
                    estado
                ) {

                    if (estado === 'abort') {
                        return;
                    }

                    let mensaje =
                        xhr.responseText ||
                        'No fue posible realizar la búsqueda.';

                    mensaje =
                        limpiarMensajeErrorServidor(
                            mensaje
                        );

                    mostrarErrorResultados(
                        mensaje
                    );
                },


                complete: function () {

                    buscandoCoincidencias = false;
                    solicitudBusquedaActual = null;

                    $('#btnBuscarCoincidencias')
                        .html(
                            '<i class="fa fa-search"></i> ' +
                            'Buscar coincidencias'
                        );

                    actualizarEstadoBotonBusqueda();
                }
            });
    }


    function mostrarCargandoResultados() {

        $('#contenedorResultadosCoincidencias')
            .removeClass(
                'sic-resultados-inicial'
            )
            .html(
                '<div class="sic-resultados-loader">' +
                '<i class="fa fa-spinner fa-spin"></i>' +
                '<strong>Analizando información biométrica</strong>' +
                '<span>' +
                'Espere mientras se buscan posibles coincidencias.' +
                '</span>' +
                '</div>'
            );
    }


    function mostrarErrorResultados(
        mensaje
    ) {

        $('#contenedorResultadosCoincidencias')
            .removeClass(
                'sic-resultados-inicial'
            )
            .html(
                '<div class="sic-error-resultados">' +
                '<i class="fa fa-exclamation-triangle"></i>' +
                '<strong>No se pudo realizar la búsqueda</strong>' +
                '<span>' +
                escaparHtml(mensaje) +
                '</span>' +
                '</div>'
            );
    }


    /* ============================================================
       EVENTOS DEL LISTADO DE RESULTADOS
       ============================================================ */

    function inicializarEventosResultados() {

        $(document)
            .off(
                'click.sicCoincidencias',
                '.js-ver-coincidencia'
            )
            .on(
                'click.sicCoincidencias',
                '.js-ver-coincidencia',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    const idCoincidencia =
                        parseInt(
                            $(this).attr('data-id'),
                            10
                        );

                    seleccionarCoincidencia(
                        idCoincidencia,
                        $(this)
                            .closest(
                                '.sic-resultado-persona'
                            )
                    );
                }
            );


        $(document)
            .off(
                'click.sicCoincidencias',
                '.sic-resultado-persona'
            )
            .on(
                'click.sicCoincidencias',
                '.sic-resultado-persona',
                function (evento) {

                    if (
                        $(evento.target)
                            .closest(
                                'button, a, input, select, textarea'
                            )
                            .length > 0
                    ) {
                        return;
                    }

                    const idCoincidencia =
                        parseInt(
                            $(this).attr('data-id'),
                            10
                        );

                    seleccionarCoincidencia(
                        idCoincidencia,
                        $(this)
                    );
                }
            );


        $(document)
            .off(
                'click.sicCoincidencias',
                '.js-tab-coincidencias'
            )
            .on(
                'click.sicCoincidencias',
                '.js-tab-coincidencias',
                function () {

                    const tipo =
                        (
                            $(this).attr('data-tipo') ||
                            'TODOS'
                        )
                            .toUpperCase();

                    $('.js-tab-coincidencias')
                        .removeClass('active');

                    $(this)
                        .addClass('active');

                    filtrarResultadosPorTipo(
                        tipo
                    );
                }
            );
    }


    function seleccionarCoincidencia(
        idCoincidencia,
        elementoResultado
    ) {

        if (
            isNaN(idCoincidencia) ||
            idCoincidencia <= 0
        ) {
            return;
        }


        $('.sic-resultado-persona')
            .removeClass('active');

        if (
            elementoResultado &&
            elementoResultado.length > 0
        ) {

            elementoResultado
                .addClass('active');
        }


        cargarDetalleCoincidencia(
            idCoincidencia
        );
    }


    function filtrarResultadosPorTipo(
        tipo
    ) {

        $('.sic-resultado-persona')
            .each(function () {

                const tipoRegistro =
                    (
                        $(this).attr('data-tipo') ||
                        ''
                    )
                        .toUpperCase();

                let mostrar = false;


                if (tipo === 'TODOS') {

                    mostrar = true;

                } else if (tipo === 'FOTO') {

                    mostrar =
                        tipoRegistro === 'FOTO' ||
                        tipoRegistro === 'COMBINADA';

                } else if (tipo === 'HUELLA') {

                    mostrar =
                        tipoRegistro === 'HUELLA' ||
                        tipoRegistro === 'COMBINADA';

                } else if (tipo === 'COMBINADA') {

                    mostrar =
                        tipoRegistro === 'COMBINADA';
                }


                $(this).toggle(mostrar);
            });


        const seleccionadoVisible =
            $('.sic-resultado-persona.active:visible');


        if (seleccionadoVisible.length === 0) {

            const primerResultadoVisible =
                $('.sic-resultado-persona:visible')
                    .first();

            if (primerResultadoVisible.length > 0) {

                const idCoincidencia =
                    parseInt(
                        primerResultadoVisible
                            .attr('data-id'),
                        10
                    );

                seleccionarCoincidencia(
                    idCoincidencia,
                    primerResultadoVisible
                );
            }
        }
    }


    /* ============================================================
       DETALLE DE LA COINCIDENCIA
       ============================================================ */

    function cargarDetalleCoincidencia(
        idCoincidencia
    ) {

        const configuracion =
            window.sicCoincidenciasConfig || {};

        if (!configuracion.urlDetalle) {

            mostrarMensaje(
                'No se configuró la dirección del detalle de coincidencia.',
                'error'
            );

            return;
        }


        if (
            solicitudDetalleActual &&
            solicitudDetalleActual.readyState !== 4
        ) {
            solicitudDetalleActual.abort();
        }


        $('#panelDetalleCoincidencia')
            .html(
                '<div class="sic-detalle-loader">' +
                '<i class="fa fa-spinner fa-spin"></i>' +
                '<span>Cargando detalle...</span>' +
                '</div>'
            );

        const contexto =
            $('#sicResultadosContexto');

        const tieneFotografiaConsulta =
            contexto.attr('data-tiene-fotografia') === 'true';

        const tieneHuellaConsulta =
            contexto.attr('data-tiene-huella') === 'true';


        solicitudDetalleActual =
            $.ajax({

                url:
                    configuracion.urlDetalle,

                type:
                    'GET',

                cache:
                    false,

                data: {
                    idCoincidencia:
                        idCoincidencia,

                    tieneFotografiaConsulta:
                        tieneFotografiaConsulta,

                    tieneHuellaConsulta:
                        tieneHuellaConsulta
                },


                success: function (html) {

                    $('#panelDetalleCoincidencia')
                        .html(html);

                    sincronizarImagenesConsulta();
                },


                error: function (
                    xhr,
                    estado
                ) {

                    if (estado === 'abort') {
                        return;
                    }

                    let mensaje =
                        xhr.responseText ||
                        'No se pudo cargar el detalle.';

                    mensaje =
                        limpiarMensajeErrorServidor(
                            mensaje
                        );

                    $('#panelDetalleCoincidencia')
                        .html(
                            '<div class="sic-detalle-vacio">' +
                            '<i class="fa fa-exclamation-circle"></i>' +
                            '<strong>' +
                            'No se pudo cargar el detalle' +
                            '</strong>' +
                            '<span>' +
                            escaparHtml(mensaje) +
                            '</span>' +
                            '</div>'
                        );
                },


                complete: function () {

                    solicitudDetalleActual = null;
                }
            });
    }


    /* ============================================================
       COMPARACIÓN DE IMÁGENES
       ============================================================ */

    function sincronizarImagenesConsulta() {

        const fotoConsulta =
            $('#previewFotografia').is(':visible')
                ? $('#previewFotografia').attr('src')
                : '';

        const huellaConsulta =
            $('#previewHuella').is(':visible')
                ? $('#previewHuella').attr('src')
                : '';


        $('.js-comparacion-foto')
            .toggle(
                !!fotoConsulta
            );

        $('.js-comparacion-huella')
            .toggle(
                !!huellaConsulta
            );


        $('.js-imagen-consulta-foto')
            .attr(
                'src',
                fotoConsulta || ''
            );

        $('.js-imagen-consulta-huella')
            .attr(
                'src',
                huellaConsulta || ''
            );
    }


    /* ============================================================
       LIMPIAR FORMULARIO
       ============================================================ */

    function inicializarLimpieza() {

        $('#btnLimpiarBusqueda')
            .off('click.sicCoincidencias')
            .on(
                'click.sicCoincidencias',
                function () {

                    limpiarBusquedaCompleta();
                }
            );
    }


    function limpiarBusquedaCompleta() {

        if (
            solicitudBusquedaActual &&
            solicitudBusquedaActual.readyState !== 4
        ) {
            solicitudBusquedaActual.abort();
        }

        if (
            solicitudDetalleActual &&
            solicitudDetalleActual.readyState !== 4
        ) {
            solicitudDetalleActual.abort();
        }


        solicitudBusquedaActual = null;
        solicitudDetalleActual = null;
        buscandoCoincidencias = false;


        limpiarArchivoBiometrico(
            'Fotografia'
        );

        limpiarArchivoBiometrico(
            'Huella'
        );


        $('#Municipio').val('');
        $('#Sexo').val('');
        $('#TipoCoincidencia').val('');

        $('#EdadMinima').val(18);
        $('#EdadMaxima').val(99);

        $('#PorcentajeMinimo').val(70);

        $('#valorPorcentajeMinimo')
            .text('70%');


        restaurarEstadoInicialResultados();

        actualizarEstadoBotonBusqueda();
    }


    function restaurarEstadoInicialResultados() {

        $('#contenedorResultadosCoincidencias')
            .addClass(
                'sic-resultados-inicial'
            )
            .html(
                '<div class="sic-resultados-inicial-icono">' +
                '<i class="fa fa-search"></i>' +
                '</div>' +
                '<strong>' +
                'Realice una búsqueda de coincidencias' +
                '</strong>' +
                '<span>' +
                'Seleccione una fotografía, una huella o ambos archivos para comenzar.' +
                '</span>'
            );
    }


    /* ============================================================
       ESTADO DEL BOTÓN DE BÚSQUEDA
       ============================================================ */

    function actualizarEstadoBotonBusqueda() {

        const tieneFotografia =
            inputTieneArchivo(
                'Fotografia'
            );

        const tieneHuella =
            inputTieneArchivo(
                'Huella'
            );


        $('#btnBuscarCoincidencias')
            .prop(
                'disabled',
                buscandoCoincidencias ||
                (
                    !tieneFotografia &&
                    !tieneHuella
                )
            );
    }


    function inputTieneArchivo(
        inputId
    ) {

        const input =
            document.getElementById(inputId);

        return !!(
            input &&
            input.files &&
            input.files.length > 0
        );
    }


    /* ============================================================
       UTILIDADES
       ============================================================ */

    function mostrarMensaje(
        mensaje,
        tipo
    ) {

        if (
            window.Swal &&
            typeof window.Swal.fire === 'function'
        ) {

            window.Swal.fire({
                icon:
                    tipo || 'info',

                title:
                    mensaje,

                confirmButtonText:
                    'Aceptar'
            });

            return;
        }


        if (
            window.swal &&
            typeof window.swal === 'function'
        ) {

            window.swal({
                title:
                    mensaje,

                icon:
                    tipo || 'info',

                button:
                    'Aceptar'
            });

            return;
        }


        alert(mensaje);
    }


    function escaparHtml(
        valor
    ) {

        return $('<div>')
            .text(
                valor || ''
            )
            .html();
    }


    function limpiarMensajeErrorServidor(
        mensaje
    ) {

        if (!mensaje) {
            return 'Ocurrió un error inesperado.';
        }


        const texto =
            $('<div>')
                .html(mensaje)
                .text()
                .replace(/\s+/g, ' ')
                .trim();


        if (texto.length > 500) {

            return texto.substring(
                0,
                500
            ) + '...';
        }


        return texto;
    }

})(jQuery);