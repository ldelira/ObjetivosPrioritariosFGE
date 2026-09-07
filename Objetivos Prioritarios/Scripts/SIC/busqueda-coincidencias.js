(function ($) {
    'use strict';

    const tiposPermitidos = [
        'image/jpeg',
        'image/jpg',
        'image/png',
        'image/webp'
    ];

    const modoBusquedaPredeterminado =
        'NOMBRE';

    const modoCombinacionPredeterminado =
        'PRIORIZAR';

    const porcentajePredeterminado =
        70;

    let solicitudBusquedaActual =
        null;

    let solicitudDetalleActual =
        null;

    let buscandoCoincidencias =
        false;

    const maximoHuellasConsulta =
        10;


    let huellasSeleccionadas =
        [];


    let criteriosBusquedaAbiertos =
        true;



    $(document).ready(function () {
        inicializarBusquedaTexto();
        inicializarModoCombinacion();
        inicializarCargaBiometrica();
        inicializarCargaHuellasMultiples();
        inicializarCarruselHuellas();
        inicializarPanelCriteriosBusqueda();
        inicializarPorcentaje();
        inicializarFiltrosAvanzados();
        inicializarConteoFiltros();
        inicializarFormulario();
        inicializarLimpieza();
        inicializarEventosResultados();
        inicializarModalDetalleTablet();

        normalizarModoBusquedaInicial();
        normalizarModoCombinacionInicial();
        sincronizarTextoBusquedaAnterior();
        actualizarConteoFiltrosAvanzados();
        abrirFiltrosAvanzadosIniciales();
        actualizarEstadoBotonBusqueda();
        actualizarResumenCriteriosBusqueda();
        inicializarMandamientosDesplegables();
    });



    /* ============================================================
       PANEL COMPACTO DE CRITERIOS
       ============================================================ */

    function inicializarPanelCriteriosBusqueda() {

        $('#btnToggleCriteriosBusqueda')
            .off(
                'click.sicCriterios'
            )
            .on(
                'click.sicCriterios',
                function () {

                    establecerEstadoPanelCriterios(
                        !criteriosBusquedaAbiertos,
                        true
                    );
                }
            );


        $('#formBusquedaCoincidencias')
            .off(
                'input.sicResumenCriterios ' +
                'change.sicResumenCriterios'
            )
            .on(
                'input.sicResumenCriterios ' +
                'change.sicResumenCriterios',
                'input, select',
                function () {

                    actualizarResumenCriteriosBusqueda();
                }
            );
    }


    function establecerEstadoPanelCriterios(
        abierto,
        animar
    ) {
        criteriosBusquedaAbiertos =
            !!abierto;


        const panel =
            $('#panelCriteriosBusqueda');

        const contenido =
            $('#contenidoCriteriosBusqueda');

        const boton =
            $('#btnToggleCriteriosBusqueda');


        panel
            .toggleClass(
                'sic-criterios-abiertos',
                criteriosBusquedaAbiertos
            )
            .toggleClass(
                'sic-criterios-cerrados',
                !criteriosBusquedaAbiertos
            );


        boton
            .attr(
                'aria-expanded',
                criteriosBusquedaAbiertos
                    ? 'true'
                    : 'false'
            );


        boton
            .find(
                '.sic-criterios-toggle-etiqueta'
            )
            .text(
                criteriosBusquedaAbiertos
                    ? 'Ocultar'
                    : 'Mostrar'
            );


        if (animar) {

            contenido
                .stop(
                    true,
                    true
                );


            if (criteriosBusquedaAbiertos) {

                contenido
                    .slideDown(
                        180
                    );
            }
            else {

                contenido
                    .slideUp(
                        180
                    );
            }
        }
        else {

            contenido
                .toggle(
                    criteriosBusquedaAbiertos
                );
        }
    }


    function actualizarResumenCriteriosBusqueda() {

        const partes =
            [];


        if (tieneNombreBusquedaValido()) {
            partes.push(
                'Nombre'
            );
        }


        if (tieneAliasBusquedaValido()) {
            partes.push(
                'Alias'
            );
        }


        if (
            inputTieneArchivo(
                'Fotografia'
            )
        ) {
            partes.push(
                'Fotografía'
            );
        }


        const totalHuellas =
            huellasSeleccionadas.length;


        if (totalHuellas > 0) {

            partes.push(
                totalHuellas +
                (
                    totalHuellas === 1
                        ? ' huella'
                        : ' huellas'
                )
            );
        }


        if (partes.length === 0) {

            partes.push(
                'Sin criterios capturados'
            );
        }


        const modoCombinacion =
            obtenerModoCombinacion() ===
                'ESTRICTO'
                ? 'Exigir todos'
                : 'Priorizar';


        const porcentaje =
            parseInt(
                $('#PorcentajeMinimo')
                    .val(),
                10
            ) ||
            porcentajePredeterminado;


        $('#resumenCriteriosBusqueda')
            .text(
                partes.join(
                    ' + '
                ) +
                ' · ' +
                modoCombinacion +
                ' · ' +
                porcentaje +
                '%'
            );
    }


    /* ============================================================
       CARRUSEL DE HUELLAS
       ============================================================ */

    function inicializarCarruselHuellas() {

        const contenedor =
            $('#contenedorHuellasSeleccionadas');


        if (
            contenedor.length === 0
        ) {
            return;
        }


        if (
            !contenedor
                .parent()
                .hasClass(
                    'sic-huellas-carrusel-shell'
                )
        ) {

            contenedor
                .wrap(
                    '<div class="sic-huellas-carrusel-shell"></div>'
                );


            const shell =
                contenedor
                    .parent();


            shell
                .prepend(
                    '<button type="button" ' +
                    'class="sic-huellas-carrusel-btn sic-huellas-carrusel-prev js-huellas-carrusel-prev" ' +
                    'aria-label="Ver huellas anteriores" title="Anteriores">' +
                    '<i class="fa fa-chevron-left"></i>' +
                    '</button>'
                )
                .append(
                    '<button type="button" ' +
                    'class="sic-huellas-carrusel-btn sic-huellas-carrusel-next js-huellas-carrusel-next" ' +
                    'aria-label="Ver huellas siguientes" title="Siguientes">' +
                    '<i class="fa fa-chevron-right"></i>' +
                    '</button>'
                );
        }


        $(document)
            .off(
                'click.sicCarruselHuellas',
                '.js-huellas-carrusel-prev'
            )
            .on(
                'click.sicCarruselHuellas',
                '.js-huellas-carrusel-prev',
                function () {

                    desplazarCarruselHuellas(
                        -1
                    );
                }
            );


        $(document)
            .off(
                'click.sicCarruselHuellas',
                '.js-huellas-carrusel-next'
            )
            .on(
                'click.sicCarruselHuellas',
                '.js-huellas-carrusel-next',
                function () {

                    desplazarCarruselHuellas(
                        1
                    );
                }
            );


        contenedor
            .off(
                'scroll.sicCarruselHuellas'
            )
            .on(
                'scroll.sicCarruselHuellas',
                function () {

                    actualizarEstadoCarruselHuellas();
                }
            );


        actualizarEstadoCarruselHuellas();
    }


    function desplazarCarruselHuellas(
        direccion
    ) {

        const contenedor =
            document.getElementById(
                'contenedorHuellasSeleccionadas'
            );


        if (!contenedor) {
            return;
        }


        const tarjeta =
            contenedor.querySelector(
                '.sic-huella-consulta-item'
            );


        let paso =
            contenedor.clientWidth *
            0.82;


        if (tarjeta) {

            const estilo =
                window.getComputedStyle(
                    contenedor
                );


            const gap =
                parseFloat(
                    estilo.columnGap ||
                    estilo.gap ||
                    '8'
                ) ||
                8;


            paso =
                Math.max(
                    tarjeta.getBoundingClientRect()
                        .width +
                    gap,
                    paso
                );
        }


        contenedor.scrollBy({
            left:
                direccion *
                paso,

            behavior:
                'smooth'
        });
    }


    function actualizarEstadoCarruselHuellas() {

        const contenedor =
            $('#contenedorHuellasSeleccionadas');

        const shell =
            contenedor
                .closest(
                    '.sic-huellas-carrusel-shell'
                );


        if (
            contenedor.length === 0 ||
            shell.length === 0
        ) {
            return;
        }


        const total =
            huellasSeleccionadas.length;


        shell
            .toggle(
                total > 0
            );


        if (total === 0) {
            return;
        }


        const elemento =
            contenedor.get(
                0
            );


        const maxScroll =
            Math.max(
                0,
                elemento.scrollWidth -
                elemento.clientWidth
            );


        const alInicio =
            elemento.scrollLeft <=
            4;


        const alFinal =
            elemento.scrollLeft >=
            maxScroll -
            4;


        shell
            .find(
                '.js-huellas-carrusel-prev'
            )
            .prop(
                'disabled',
                alInicio
            );


        shell
            .find(
                '.js-huellas-carrusel-next'
            )
            .prop(
                'disabled',
                alFinal ||
                maxScroll <= 4
            );
    }


    /* ============================================================
       BÚSQUEDA TEXTUAL
       ============================================================ */

    function inicializarBusquedaTexto() {
        $(document)
            .off(
                'click.sicBusquedaTexto',
                '.sic-modo-busqueda'
            )
            .on(
                'click.sicBusquedaTexto',
                '.sic-modo-busqueda',
                function () {
                    const modo =
                        normalizarModoBusqueda(
                            $(this).attr('data-modo')
                        );

                    establecerModoBusqueda(
                        modo,
                        true
                    );
                }
            );

        $('#NombreBusqueda, #AliasBusqueda')
            .off(
                'input.sicBusquedaTexto ' +
                'change.sicBusquedaTexto'
            )
            .on(
                'input.sicBusquedaTexto ' +
                'change.sicBusquedaTexto',
                function () {
                    sincronizarTextoBusquedaAnterior();
                    actualizarEstadoBotonBusqueda();
                }
            );

        $('#NombreBusqueda, #AliasBusqueda')
            .off(
                'keydown.sicBusquedaTexto'
            )
            .on(
                'keydown.sicBusquedaTexto',
                function (evento) {
                    if (
                        evento.key === 'Enter' &&
                        !buscandoCoincidencias
                    ) {
                        evento.preventDefault();

                        $('#formBusquedaCoincidencias')
                            .trigger('submit');
                    }
                }
            );

        $(document)
            .off(
                'click.sicLimpiarCampoTexto',
                '.js-limpiar-campo-texto'
            )
            .on(
                'click.sicLimpiarCampoTexto',
                '.js-limpiar-campo-texto',
                function () {
                    const target =
                        $(this).attr('data-target');

                    if (!target) {
                        return;
                    }

                    $('#' + target)
                        .val('')
                        .trigger('focus');

                    sincronizarTextoBusquedaAnterior();
                    actualizarEstadoBotonBusqueda();
                }
            );
    }


    function normalizarModoBusquedaInicial() {
        establecerModoBusqueda(
            normalizarModoBusqueda(
                $('#ModoBusquedaTexto').val()
            ),
            false
        );
    }


    function normalizarModoBusqueda(
        modo
    ) {
        const valor =
            String(
                modo || ''
            )
                .trim()
                .toUpperCase();

        if (
            valor === 'NOMBRE' ||
            valor === 'ALIAS' ||
            valor === 'AMBOS'
        ) {
            return valor;
        }

        return modoBusquedaPredeterminado;
    }


    function establecerModoBusqueda(
        modo,
        enfocarCampo
    ) {
        const modoNormalizado =
            normalizarModoBusqueda(
                modo
            );

        $('#ModoBusquedaTexto')
            .val(
                modoNormalizado
            );

        $('.sic-modo-busqueda')
            .removeClass('active')
            .attr(
                'aria-pressed',
                'false'
            );

        $('.sic-modo-busqueda[data-modo="' +
            modoNormalizado +
            '"]')
            .addClass('active')
            .attr(
                'aria-pressed',
                'true'
            );

        const mostrarNombre =
            modoNormalizado === 'NOMBRE' ||
            modoNormalizado === 'AMBOS';

        const mostrarAlias =
            modoNormalizado === 'ALIAS' ||
            modoNormalizado === 'AMBOS';

        $('#grupoNombreBusqueda')
            .toggle(
                mostrarNombre
            );

        $('#grupoAliasBusqueda')
            .toggle(
                mostrarAlias
            );

        $('#contenedorCamposTextuales')
            .toggleClass(
                'sic-campos-dobles',
                modoNormalizado === 'AMBOS'
            )
            .toggleClass(
                'sic-campos-sencillos',
                modoNormalizado !== 'AMBOS'
            );

        actualizarDescripcionModoBusqueda(
            modoNormalizado
        );

        sincronizarTextoBusquedaAnterior();
        actualizarEstadoBotonBusqueda();

        if (enfocarCampo) {
            window.setTimeout(
                function () {
                    if (modoNormalizado === 'ALIAS') {
                        $('#AliasBusqueda')
                            .trigger('focus');
                    }
                    else {
                        $('#NombreBusqueda')
                            .trigger('focus');
                    }
                },
                0
            );
        }
    }


    function actualizarDescripcionModoBusqueda(
        modo
    ) {
        let descripcion =
            'Se buscarán personas que coincidan con el nombre capturado.';

        if (modo === 'ALIAS') {
            descripcion =
                'Se buscarán personas que coincidan con el alias capturado.';
        }
        else if (modo === 'AMBOS') {
            descripcion =
                'El nombre y el alias se evaluarán como criterios independientes.';
        }

        $('#textoDescripcionModoBusqueda')
            .text(
                descripcion
            );
    }


    function obtenerNombreBusqueda() {
        return $.trim(
            $('#NombreBusqueda').val() || ''
        );
    }


    function obtenerAliasBusqueda() {
        return $.trim(
            $('#AliasBusqueda').val() || ''
        );
    }


    function obtenerNombreBusquedaActivo() {
        const modo =
            normalizarModoBusqueda(
                $('#ModoBusquedaTexto').val()
            );

        if (
            modo !== 'NOMBRE' &&
            modo !== 'AMBOS'
        ) {
            return '';
        }

        return obtenerNombreBusqueda();
    }


    function obtenerAliasBusquedaActivo() {
        const modo =
            normalizarModoBusqueda(
                $('#ModoBusquedaTexto').val()
            );

        if (
            modo !== 'ALIAS' &&
            modo !== 'AMBOS'
        ) {
            return '';
        }

        return obtenerAliasBusqueda();
    }


    function tieneNombreBusquedaValido() {
        return obtenerNombreBusquedaActivo()
            .length >= 2;
    }


    function tieneAliasBusquedaValido() {
        return obtenerAliasBusquedaActivo()
            .length >= 2;
    }


    function tieneTextoBusquedaValido() {
        return (
            tieneNombreBusquedaValido() ||
            tieneAliasBusquedaValido()
        );
    }


    function obtenerTextoBusquedaCompatibilidad() {
        const nombre =
            obtenerNombreBusquedaActivo();

        const alias =
            obtenerAliasBusquedaActivo();

        /*
         * Compatibilidad temporal con el backend anterior.
         * Cuando existan ambos criterios se conserva primero
         * el nombre porque antes solamente se recibía un texto.
         */
        return nombre || alias;
    }


    function sincronizarTextoBusquedaAnterior() {
        $('#TextoBusqueda')
            .val(
                obtenerTextoBusquedaCompatibilidad()
            );
    }


    /* ============================================================
       MODO DE COMBINACIÓN
       ============================================================ */

    function inicializarModoCombinacion() {
        $(document)
            .off(
                'change.sicModoCombinacion',
                '.js-modo-combinacion'
            )
            .on(
                'change.sicModoCombinacion',
                '.js-modo-combinacion',
                function () {
                    establecerModoCombinacion(
                        $(this).val()
                    );
                }
            );
    }


    function normalizarModoCombinacionInicial() {
        establecerModoCombinacion(
            obtenerModoCombinacion()
        );
    }


    function normalizarModoCombinacion(
        modo
    ) {
        const valor =
            String(
                modo || ''
            )
                .trim()
                .toUpperCase();

        if (
            valor === 'PRIORIZAR' ||
            valor === 'ESTRICTO'
        ) {
            return valor;
        }

        return modoCombinacionPredeterminado;
    }


    function obtenerModoCombinacion() {
        return normalizarModoCombinacion(
            $('input[name="ModoCombinacion"]:checked')
                .val()
        );
    }


    function establecerModoCombinacion(
        modo
    ) {
        const modoNormalizado =
            normalizarModoCombinacion(
                modo
            );

        $('input[name="ModoCombinacion"]')
            .prop(
                'checked',
                false
            );

        $('input[name="ModoCombinacion"][value="' +
            modoNormalizado +
            '"]')
            .prop(
                'checked',
                true
            );

        $('.sic-opcion-combinacion')
            .removeClass('active');

        $('input[name="ModoCombinacion"][value="' +
            modoNormalizado +
            '"]')
            .closest(
                '.sic-opcion-combinacion'
            )
            .addClass('active');
    }


    /* ============================================================
       ARCHIVOS BIOMÉTRICOS
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
                    procesarArchivoSeleccionado(
                        this
                    );
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
                function (evento) {
                    evento.preventDefault();
                    evento.stopPropagation();

                    const inputId =
                        $(this).attr(
                            'data-input-id'
                        );

                    if (inputId) {
                        $('#' + inputId)
                            .trigger('click');
                    }
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
                function (evento) {
                    evento.preventDefault();
                    evento.stopPropagation();

                    limpiarArchivoBiometrico(
                        $(this).attr(
                            'data-input-id'
                        )
                    );
                }
            );

        $('.sic-dropzone')
            .off(
                '.sicCoincidenciasDrop'
            )
            .on(
                'dragenter.sicCoincidenciasDrop ' +
                'dragover.sicCoincidenciasDrop',
                function (evento) {
                    evento.preventDefault();
                    evento.stopPropagation();

                    $(this)
                        .addClass(
                            'sic-dropzone-activa'
                        );
                }
            )
            .on(
                'dragleave.sicCoincidenciasDrop ' +
                'drop.sicCoincidenciasDrop',
                function (evento) {
                    evento.preventDefault();
                    evento.stopPropagation();

                    $(this)
                        .removeClass(
                            'sic-dropzone-activa'
                        );
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
                        eventoOriginal
                            .dataTransfer
                            .files;

                    if (
                        !archivos ||
                        archivos.length === 0
                    ) {
                        return;
                    }

                    const inputId =
                        $(this).attr(
                            'data-input-id'
                        );

                    asignarArchivoAInput(
                        document.getElementById(
                            inputId
                        ),
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

            transferencia.items.add(
                archivo
            );

            input.files =
                transferencia.files;

            procesarArchivoSeleccionado(
                input
            );
        }
        catch (error) {
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
            limpiarVistaPrevia(
                input
            );

            actualizarEstadoBotonBusqueda();
            return;
        }

        const archivo =
            input.files[0];

        if (!archivoEsValido(archivo)) {
            input.value = '';

            limpiarVistaPrevia(
                input
            );

            actualizarEstadoBotonBusqueda();
            return;
        }

        const previewId =
            $(input).attr(
                'data-preview'
            );

        const estadoVacioId =
            $(input).attr(
                'data-vacio'
            );

        const quitarId =
            $(input).attr(
                'data-quitar'
            );

        const lector =
            new FileReader();

        lector.onload =
            function (evento) {
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
                    .closest(
                        '.sic-biometrico-tile'
                    )
                    .addClass(
                        'sic-biometrico-con-archivo'
                    );

                actualizarEstadoBotonBusqueda();
            };

        lector.onerror =
            function () {
                input.value = '';

                limpiarVistaPrevia(
                    input
                );

                actualizarEstadoBotonBusqueda();

                mostrarMensaje(
                    'No fue posible leer el archivo seleccionado.',
                    'error'
                );
            };

        lector.readAsDataURL(
            archivo
        );
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
                .test(
                    nombreArchivo
                );

        const mimeValido =
            tipoArchivo === '' ||
            tiposPermitidos.indexOf(
                tipoArchivo
            ) !== -1;

        if (
            !extensionValida ||
            !mimeValido
        ) {
            mostrarMensaje(
                'Seleccione una imagen JPG, PNG o WEBP.',
                'warning'
            );

            return false;
        }

        const configuracion =
            window.sicCoincidenciasConfig || {};

        const maximoMb =
            configuracion.maximoArchivoMb ||
            10;

        const maximoBytes =
            maximoMb *
            1024 *
            1024;

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
            document.getElementById(
                inputId
            );

        if (!input) {
            return;
        }

        input.value = '';

        limpiarVistaPrevia(
            input
        );

        actualizarEstadoBotonBusqueda();
    }


    function limpiarVistaPrevia(
        input
    ) {
        if (!input) {
            return;
        }

        const previewId =
            $(input).attr(
                'data-preview'
            );

        const estadoVacioId =
            $(input).attr(
                'data-vacio'
            );

        const quitarId =
            $(input).attr(
                'data-quitar'
            );

        if (previewId) {
            $('#' + previewId)
                .attr(
                    'src',
                    ''
                )
                .hide();
        }

        if (estadoVacioId) {
            $('#' + estadoVacioId)
                .show();
        }

        if (quitarId) {
            $('#' + quitarId)
                .prop(
                    'disabled',
                    true
                );
        }

        $(input)
            .closest(
                '.sic-biometrico-tile'
            )
            .removeClass(
                'sic-biometrico-con-archivo'
            );
    }


    function inputTieneArchivo(
        inputId
    ) {
        const input =
            document.getElementById(
                inputId
            );

        return !!(
            input &&
            input.files &&
            input.files.length > 0
        );
    }

    /* ================================================================
   HUELLAS MÚLTIPLES
   ================================================================ */

    function inicializarCargaHuellasMultiples() {

        $(document)
            .off(
                'change.sicHuellasMultiples',
                '#Huellas'
            )
            .on(
                'change.sicHuellasMultiples',
                '#Huellas',
                function () {

                    agregarHuellasSeleccionadas(
                        this.files
                    );
                }
            );


        $(document)
            .off(
                'click.sicHuellasMultiples',
                '.js-seleccionar-huellas'
            )
            .on(
                'click.sicHuellasMultiples',
                '.js-seleccionar-huellas',
                function (evento) {

                    evento.preventDefault();

                    evento.stopPropagation();


                    $('#Huellas')
                        .trigger(
                            'click'
                        );
                }
            );


        $(document)
            .off(
                'click.sicHuellasMultiples',
                '.js-quitar-todas-huellas'
            )
            .on(
                'click.sicHuellasMultiples',
                '.js-quitar-todas-huellas',
                function (evento) {

                    evento.preventDefault();

                    evento.stopPropagation();


                    limpiarHuellasSeleccionadas();
                }
            );


        $(document)
            .off(
                'click.sicHuellasMultiples',
                '.js-quitar-huella-consulta'
            )
            .on(
                'click.sicHuellasMultiples',
                '.js-quitar-huella-consulta',
                function (evento) {

                    evento.preventDefault();

                    evento.stopPropagation();


                    const indice =
                        parseInt(
                            $(this).attr(
                                'data-index'
                            ),
                            10
                        );


                    quitarHuellaSeleccionada(
                        indice
                    );
                }
            );


        $('#dropzoneHuellas')
            .off(
                '.sicHuellasDrop'
            )
            .on(
                'click.sicHuellasDrop',
                function () {

                    $('#Huellas')
                        .trigger(
                            'click'
                        );
                }
            )
            .on(
                'dragenter.sicHuellasDrop ' +
                'dragover.sicHuellasDrop',
                function (evento) {

                    evento.preventDefault();

                    evento.stopPropagation();


                    $(this)
                        .addClass(
                            'sic-dropzone-activa'
                        );
                }
            )
            .on(
                'dragleave.sicHuellasDrop ' +
                'drop.sicHuellasDrop',
                function (evento) {

                    evento.preventDefault();

                    evento.stopPropagation();


                    $(this)
                        .removeClass(
                            'sic-dropzone-activa'
                        );
                }
            )
            .on(
                'drop.sicHuellasDrop',
                function (evento) {

                    const original =
                        evento.originalEvent;


                    if (
                        !original ||
                        !original.dataTransfer ||
                        !original.dataTransfer.files
                    ) {
                        return;
                    }


                    agregarHuellasSeleccionadas(
                        original.dataTransfer.files
                    );
                }
            );
    }


    function agregarHuellasSeleccionadas(
        archivos
    ) {
        if (
            !archivos ||
            archivos.length === 0
        ) {
            return;
        }


        let limiteAlcanzado =
            false;


        Array.from(
            archivos
        )
            .forEach(function (archivo) {

                if (
                    huellasSeleccionadas.length >=
                    maximoHuellasConsulta
                ) {
                    limiteAlcanzado =
                        true;

                    return;
                }


                if (
                    !archivoEsValido(
                        archivo
                    )
                ) {
                    return;
                }


                if (
                    huellaYaSeleccionada(
                        archivo
                    )
                ) {
                    return;
                }


                huellasSeleccionadas.push(
                    archivo
                );
            });


        if (limiteAlcanzado) {

            mostrarMensaje(
                'Puede agregar como máximo ' +
                maximoHuellasConsulta +
                ' huellas por búsqueda.',
                'warning'
            );
        }


        sincronizarInputHuellas();

        renderizarHuellasSeleccionadas();

        actualizarEstadoBotonBusqueda();
    }


    function huellaYaSeleccionada(
        archivo
    ) {
        if (!archivo) {
            return false;
        }


        return huellasSeleccionadas
            .some(function (existente) {

                return (
                    existente.name ===
                    archivo.name &&
                    existente.size ===
                    archivo.size &&
                    existente.lastModified ===
                    archivo.lastModified
                );
            });
    }


    function sincronizarInputHuellas() {

        const input =
            document.getElementById(
                'Huellas'
            );


        if (!input) {
            return;
        }


        try {

            const transferencia =
                new DataTransfer();


            huellasSeleccionadas
                .forEach(function (archivo) {

                    transferencia.items.add(
                        archivo
                    );
                });


            input.files =
                transferencia.files;
        }
        catch (error) {

            mostrarMensaje(
                'No fue posible preparar las huellas seleccionadas.',
                'error'
            );
        }
    }


    function quitarHuellaSeleccionada(
        indice
    ) {
        if (
            isNaN(indice) ||
            indice < 0 ||
            indice >= huellasSeleccionadas.length
        ) {
            return;
        }


        huellasSeleccionadas.splice(
            indice,
            1
        );


        sincronizarInputHuellas();

        renderizarHuellasSeleccionadas();

        actualizarEstadoBotonBusqueda();
    }


    function limpiarHuellasSeleccionadas() {

        huellasSeleccionadas =
            [];


        const input =
            document.getElementById(
                'Huellas'
            );


        if (input) {
            input.value =
                '';
        }


        sincronizarInputHuellas();

        renderizarHuellasSeleccionadas();

        actualizarEstadoBotonBusqueda();
    }


    function renderizarHuellasSeleccionadas() {

        const contenedor =
            $('#contenedorHuellasSeleccionadas');


        const total =
            huellasSeleccionadas.length;


        $('#contadorHuellas')
            .text(
                total +
                ' / ' +
                maximoHuellasConsulta
            );


        $('#btnQuitarTodasHuellas')
            .prop(
                'disabled',
                total === 0
            );


        $('#btnAgregarHuellas')
            .prop(
                'disabled',
                total >= maximoHuellasConsulta
            );


        $('#tileHuellas')
            .toggleClass(
                'sic-biometrico-con-archivo',
                total > 0
            );


        contenedor
            .empty();


        if (total === 0) {

            contenedor
                .hide();

            actualizarResumenCriteriosBusqueda();
            actualizarEstadoCarruselHuellas();

            return;
        }


        contenedor
            .show();


        huellasSeleccionadas
            .forEach(function (
                archivo,
                indice
            ) {

                const tarjeta =
                    $('<div>')
                        .addClass(
                            'sic-huella-consulta-item'
                        );


                const imagenWrap =
                    $('<div>')
                        .addClass(
                            'sic-huella-consulta-imagen-wrap'
                        );


                const imagen =
                    $('<img>')
                        .addClass(
                            'sic-huella-consulta-imagen'
                        )
                        .attr(
                            'alt',
                            'Huella ' +
                            (indice + 1)
                        );


                const numero =
                    $('<span>')
                        .addClass(
                            'sic-huella-consulta-numero'
                        )
                        .text(
                            'HUELLA ' +
                            (indice + 1)
                        );


                const botonQuitar =
                    $('<button>')
                        .attr({
                            type:
                                'button',

                            title:
                                'Quitar huella',

                            'data-index':
                                indice
                        })
                        .addClass(
                            'sic-huella-consulta-quitar ' +
                            'js-quitar-huella-consulta'
                        );


                botonQuitar
                    .append(
                        $('<i>')
                            .addClass(
                                'fa fa-times'
                            )
                    );


                const informacion =
                    $('<div>')
                        .addClass(
                            'sic-huella-consulta-info'
                        );


                const nombre =
                    $('<strong>')
                        .text(
                            archivo.name
                        );


                const tamanio =
                    $('<span>')
                        .text(
                            formatearTamanioArchivo(
                                archivo.size
                            )
                        );


                informacion
                    .append(
                        nombre
                    )
                    .append(
                        tamanio
                    );


                imagenWrap
                    .append(
                        imagen
                    )
                    .append(
                        numero
                    )
                    .append(
                        botonQuitar
                    );


                tarjeta
                    .append(
                        imagenWrap
                    )
                    .append(
                        informacion
                    );


                contenedor
                    .append(
                        tarjeta
                    );


                const lector =
                    new FileReader();


                lector.onload =
                    function (evento) {

                        imagen.attr(
                            'src',
                            evento.target.result
                        );
                    };


                lector.readAsDataURL(
                    archivo
                );
            });


        actualizarResumenCriteriosBusqueda();


        window.requestAnimationFrame(
            function () {

                actualizarEstadoCarruselHuellas();
            }
        );
    }


    function formatearTamanioArchivo(
        bytes
    ) {
        if (
            !bytes ||
            bytes <= 0
        ) {
            return '0 KB';
        }


        const kb =
            bytes / 1024;


        if (kb < 1024) {

            return (
                Math.round(
                    kb
                ) +
                ' KB'
            );
        }


        const mb =
            kb / 1024;


        return (
            mb.toFixed(
                1
            ) +
            ' MB'
        );
    }
    /* ============================================================
       PORCENTAJE Y FILTROS
       ============================================================ */

    function inicializarPorcentaje() {
        $('#PorcentajeMinimo')
            .off(
                'input.sicCoincidencias ' +
                'change.sicCoincidencias'
            )
            .on(
                'input.sicCoincidencias ' +
                'change.sicCoincidencias',
                function () {
                    $('#valorPorcentajeMinimo')
                        .text(
                            ($(this).val() || porcentajePredeterminado) +
                            '%'
                        );
                }
            );
    }


    function inicializarFiltrosAvanzados() {
        $('#btnFiltrosAvanzados')
            .off(
                'click.sicFiltrosAvanzados'
            )
            .on(
                'click.sicFiltrosAvanzados',
                function () {
                    const panel =
                        $('#panelFiltrosAvanzados');

                    establecerEstadoFiltrosAvanzados(
                        !panel.is(':visible'),
                        true
                    );
                }
            );
    }


    function establecerEstadoFiltrosAvanzados(
        abrir,
        animar
    ) {
        const panel =
            $('#panelFiltrosAvanzados');

        const boton =
            $('#btnFiltrosAvanzados');

        if (
            panel.length === 0 ||
            boton.length === 0
        ) {
            return;
        }

        panel.stop(
            true,
            true
        );

        if (abrir) {
            animar
                ? panel.slideDown(180)
                : panel.show();

            boton
                .addClass('active')
                .attr(
                    'aria-expanded',
                    'true'
                );
        }
        else {
            animar
                ? panel.slideUp(180)
                : panel.hide();

            boton
                .removeClass('active')
                .attr(
                    'aria-expanded',
                    'false'
                );
        }
    }


    function inicializarConteoFiltros() {
        $('#Municipio, #Sexo, #EdadMinima, #EdadMaxima')
            .off(
                'change.sicConteoFiltros ' +
                'input.sicConteoFiltros'
            )
            .on(
                'change.sicConteoFiltros ' +
                'input.sicConteoFiltros',
                function () {
                    actualizarConteoFiltrosAvanzados();
                }
            );
    }


    function obtenerCantidadFiltrosAvanzados() {
        let total = 0;

        if ($.trim($('#Municipio').val() || '') !== '') {
            total++;
        }

        if ($.trim($('#Sexo').val() || '') !== '') {
            total++;
        }

        if ($.trim($('#EdadMinima').val() || '') !== '') {
            total++;
        }

        if ($.trim($('#EdadMaxima').val() || '') !== '') {
            total++;
        }

        return total;
    }


    function actualizarConteoFiltrosAvanzados() {
        const total =
            obtenerCantidadFiltrosAvanzados();

        const contador =
            $('#contadorFiltrosAvanzados');

        contador.text(
            total
        );

        total > 0
            ? contador.show()
            : contador.hide();
    }


    function abrirFiltrosAvanzadosIniciales() {
        establecerEstadoFiltrosAvanzados(
            obtenerCantidadFiltrosAvanzados() > 0,
            false
        );
    }


    /* ============================================================
       FORMULARIO Y BÚSQUEDA AJAX
       ============================================================ */

    function inicializarFormulario() {
        $('#formBusquedaCoincidencias')
            .off(
                'submit.sicCoincidencias'
            )
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
        const modoBusqueda =
            normalizarModoBusqueda(
                $('#ModoBusquedaTexto').val()
            );

        const nombre =
            obtenerNombreBusquedaActivo();

        const alias =
            obtenerAliasBusquedaActivo();

        const tieneFotografia =
            inputTieneArchivo(
                'Fotografia'
            );

        const tieneHuella =
            inputTieneArchivo(
                'Huellas'
            );

        if (
            nombre.length === 0 &&
            alias.length === 0 &&
            !tieneFotografia &&
            !tieneHuella
        ) {
            mostrarMensaje(
                'Capture un nombre, un alias o agregue una fotografía o huella.',
                'warning'
            );

            enfocarPrimerCampoActivo(
                modoBusqueda
            );

            return false;
        }

        if (
            nombre.length === 1
        ) {
            mostrarMensaje(
                'El nombre debe contener al menos dos caracteres.',
                'warning'
            );

            $('#NombreBusqueda')
                .trigger('focus');

            return false;
        }

        if (
            alias.length === 1
        ) {
            mostrarMensaje(
                'El alias debe contener al menos dos caracteres.',
                'warning'
            );

            $('#AliasBusqueda')
                .trigger('focus');

            return false;
        }

        $('#ModoBusquedaTexto')
            .val(
                modoBusqueda
            );

        establecerModoCombinacion(
            obtenerModoCombinacion()
        );

        sincronizarTextoBusquedaAnterior();

        const edadMinimaTexto =
            $.trim(
                $('#EdadMinima').val() || ''
            );

        const edadMaximaTexto =
            $.trim(
                $('#EdadMaxima').val() || ''
            );

        let edadMinima = null;
        let edadMaxima = null;

        if (edadMinimaTexto !== '') {
            edadMinima =
                parseInt(
                    edadMinimaTexto,
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

                abrirYEnfocarFiltro(
                    'EdadMinima'
                );

                return false;
            }
        }

        if (edadMaximaTexto !== '') {
            edadMaxima =
                parseInt(
                    edadMaximaTexto,
                    10
                );

            if (
                isNaN(edadMaxima) ||
                edadMaxima < 0 ||
                edadMaxima > 120
            ) {
                mostrarMensaje(
                    'La edad máxima debe estar entre 0 y 120 años.',
                    'warning'
                );

                abrirYEnfocarFiltro(
                    'EdadMaxima'
                );

                return false;
            }
        }

        if (
            edadMinima !== null &&
            edadMaxima !== null &&
            edadMinima > edadMaxima
        ) {
            mostrarMensaje(
                'La edad mínima no puede ser mayor que la edad máxima.',
                'warning'
            );

            abrirYEnfocarFiltro(
                'EdadMinima'
            );

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


    function enfocarPrimerCampoActivo(
        modoBusqueda
    ) {
        if (modoBusqueda === 'ALIAS') {
            $('#AliasBusqueda')
                .trigger('focus');
        }
        else {
            $('#NombreBusqueda')
                .trigger('focus');
        }
    }


    function abrirYEnfocarFiltro(
        inputId
    ) {
        establecerEstadoFiltrosAvanzados(
            true,
            true
        );

        window.setTimeout(
            function () {
                $('#' + inputId)
                    .trigger('focus');
            },
            220
        );
    }


    function buscarCoincidencias() {
        if (buscandoCoincidencias) {
            return;
        }

        const formulario =
            document.getElementById(
                'formBusquedaCoincidencias'
            );

        const configuracion =
            window.sicCoincidenciasConfig || {};

        if (!formulario) {
            mostrarMensaje(
                'No se encontró el formulario de búsqueda.',
                'error'
            );

            return;
        }

        if (!configuracion.urlBuscar) {
            mostrarMensaje(
                'No se configuró la dirección para buscar coincidencias.',
                'error'
            );

            return;
        }

        cancelarSolicitudBusqueda();

        const datos =
            new FormData(
                formulario
            );

        const nombreBusqueda =
            obtenerNombreBusquedaActivo();

        const aliasBusqueda =
            obtenerAliasBusquedaActivo();

        datos.set(
            'NombreBusqueda',
            nombreBusqueda
        );

        datos.set(
            'AliasBusqueda',
            aliasBusqueda
        );

        datos.set(
            'TextoBusqueda',
            obtenerTextoBusquedaCompatibilidad()
        );

        datos.set(
            'ModoBusquedaTexto',
            normalizarModoBusqueda(
                $('#ModoBusquedaTexto').val()
            )
        );

        datos.set(
            'ModoCombinacion',
            obtenerModoCombinacion()
        );

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

                beforeSend:
                    function () {
                        actualizarResumenCriteriosBusqueda();

                        $('#sicBusquedaCoincidencias')
                            .addClass(
                                'sic-modo-resultados'
                            );

                        establecerEstadoPanelCriterios(
                            false,
                            true
                        );

                        mostrarCargandoResultados();

                        $('#btnBuscarCoincidencias')
                            .prop(
                                'disabled',
                                true
                            )
                            .html(
                                '<i class="fa fa-spinner fa-spin"></i> ' +
                                '<span>Buscando...</span>'
                            );
                    },
                success:
                    function (html) {
                        $('#contenedorResultadosCoincidencias')
                            .removeClass(
                                'sic-resultados-inicial'
                            )
                            .html(
                                html
                            );

                        sincronizarImagenesConsulta();
                    },

                error:
                    function (
                        xhr,
                        estado
                    ) {
                        if (estado === 'abort') {
                            return;
                        }

                        $('#sicBusquedaCoincidencias')
                            .removeClass(
                                'sic-modo-resultados'
                            );

                        establecerEstadoPanelCriterios(
                            true,
                            true
                        );

                        mostrarErrorResultados(
                            limpiarMensajeErrorServidor(
                                xhr.responseText ||
                                'No fue posible realizar la búsqueda.'
                            )
                        );
                    },

                complete:
                    function () {
                        buscandoCoincidencias = false;
                        solicitudBusquedaActual = null;

                        $('#btnBuscarCoincidencias')
                            .html(
                                '<i class="fa fa-search"></i> ' +
                                '<span>Buscar coincidencias</span>'
                            );

                        actualizarEstadoBotonBusqueda();

                        /*
                         * Esperamos a que finalice completamente
                         * la petición de búsqueda antes de solicitar
                         * el detalle.
                         */
                        window.setTimeout(
                            function () {
                                const primerResultado =
                                    $('.sic-resultado-persona:visible')
                                        .first();

                                if (primerResultado.length === 0) {
                                    return;
                                }

                                seleccionarResultadoDesdeElemento(
                                    primerResultado
                                );
                            },
                            100
                        );
                    }

            });
    }


    function cancelarSolicitudBusqueda() {
        if (
            solicitudBusquedaActual &&
            solicitudBusquedaActual.readyState !== 4
        ) {
            solicitudBusquedaActual.abort();
        }

        solicitudBusquedaActual = null;
    }


    function mostrarCargandoResultados() {
        const tieneNombre =
            tieneNombreBusquedaValido();

        const tieneAlias =
            tieneAliasBusquedaValido();

        const tieneTexto =
            tieneNombre ||
            tieneAlias;

        const tieneFotografia =
            inputTieneArchivo(
                'Fotografia'
            );

        const tieneHuella =
            inputTieneArchivo(
                'Huellas'
            );

        let descripcion =
            'Se están analizando los criterios proporcionados.';

        if (
            tieneTexto &&
            (tieneFotografia || tieneHuella)
        ) {
            descripcion =
                'Se están evaluando los criterios textuales y biométricos proporcionados.';
        }
        else if (
            tieneNombre &&
            tieneAlias
        ) {
            descripcion =
                'Se están comparando el nombre y el alias como criterios independientes.';
        }
        else if (tieneNombre) {
            descripcion =
                'Se está buscando el nombre en las fuentes disponibles.';
        }
        else if (tieneAlias) {
            descripcion =
                'Se está buscando el alias en las fuentes disponibles.';
        }
        else {
            descripcion =
                'Se están comparando los datos biométricos proporcionados.';
        }

        $('#contenedorResultadosCoincidencias')
            .removeClass(
                'sic-resultados-inicial'
            )
            .html(
                '<div class="sic-resultados-loader">' +
                '<i class="fa fa-spinner fa-spin"></i>' +
                '<strong>Analizando posibles coincidencias</strong>' +
                '<span>' +
                escaparHtml(descripcion) +
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
       RESULTADOS Y DETALLE
       ============================================================ */

    function inicializarEventosResultados() {

        $(document)
            .off(
                'click.sicFotoGrande',
                '.js-ampliar-foto'
            )
            .on(
                'click.sicFotoGrande',
                '.js-ampliar-foto',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    const boton =
                        $(this);

                    const foto =
                        boton.attr(
                            'data-foto'
                        ) || '';

                    const nombre =
                        boton.attr(
                            'data-nombre'
                        ) || 'Fotografía';

                    const fuente =
                        boton.attr(
                            'data-fuente'
                        ) || '';


                    if (!foto) {
                        return;
                    }


                    $('#sicFotoGrandeImagen')
                        .attr(
                            'src',
                            foto
                        );


                    $('#sicFotoGrandeNombre')
                        .text(
                            nombre
                        );


                    $('#sicFotoGrandeFuente')
                        .text(
                            fuente
                        );


                    $('#sicModalFotoGrande')
                        .addClass(
                            'abierto'
                        )
                        .attr(
                            'aria-hidden',
                            'false'
                        );


                    $('body')
                        .addClass(
                            'sic-modal-abierto'
                        );
                }
            );
        $(document)
            .off(
                'click.sicCerrarFotoGrande',
                '.js-cerrar-foto-grande'
            )
            .on(
                'click.sicCerrarFotoGrande',
                '.js-cerrar-foto-grande',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    cerrarFotoGrande();
                }
            );

        $(document)
            .off(
                'keydown.sicComparacionFoto'
            )
            .on(
                'keydown.sicComparacionFoto',
                function (evento) {

                    if (
                        evento.key === 'Escape' &&
                        $('#sicModalComparacionFoto')
                            .hasClass('abierto')
                    ) {
                        cerrarComparacionFoto();
                    }
                }
            );

        $(document)
            .off(
                'click.sicComparacionFoto',
                '.js-abrir-comparacion-foto'
            )
            .on(
                'click.sicComparacionFoto',
                '.js-abrir-comparacion-foto',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    const boton =
                        $(this);

                    const fotoConsulta =
                        ($('#previewFotografia').attr('src') || '')
                            .trim();
                    const fotoCandidato =
                        boton.attr(
                            'data-foto-candidato'
                        ) || '';

                    const nombre =
                        boton.attr(
                            'data-nombre'
                        ) || '';

                    const fuente =
                        boton.attr(
                            'data-fuente'
                        ) || '';

                    const porcentaje =
                        boton.attr(
                            'data-porcentaje'
                        ) || '0';


                    if (!fotoConsulta) {
                        mostrarMensaje(
                            'No se encontró la fotografía utilizada en la consulta.',
                            'warning'
                        );

                        return;
                    }


                    $('#sicComparacionFotoConsulta')
                        .attr(
                            'src',
                            fotoConsulta
                        );


                    $('#sicComparacionFotoCandidato')
                        .attr(
                            'src',
                            fotoCandidato
                        );


                    $('#sicComparacionFotoNombre')
                        .text(
                            nombre
                        );


                    $('#sicComparacionFotoFuente')
                        .text(
                            fuente
                        );


                    $('#sicComparacionFotoPorcentaje')
                        .text(
                            porcentaje + '%'
                        );


                    $('#sicModalComparacionFoto')
                        .addClass(
                            'abierto'
                        )
                        .attr(
                            'aria-hidden',
                            'false'
                        );


                    $('body')
                        .addClass(
                            'sic-modal-abierto'
                        );
                }
            );
        $(document)
            .off(
                'click.sicCerrarComparacionFoto',
                '.js-cerrar-comparacion-foto'
            )
            .on(
                'click.sicCerrarComparacionFoto',
                '.js-cerrar-comparacion-foto',
                function (evento) {

                    evento.preventDefault();
                    evento.stopPropagation();

                    cerrarComparacionFoto();
                }
            );
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

                    seleccionarCoincidencia(
                        parseInt(
                            $(this).attr(
                                'data-id'
                            ),
                            10
                        ),
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

                    seleccionarResultadoDesdeElemento(
                        $(this)
                    );
                }
            );

        $(document)
            .off(
                'keydown.sicCoincidencias',
                '.sic-resultado-persona'
            )
            .on(
                'keydown.sicCoincidencias',
                '.sic-resultado-persona',
                function (evento) {
                    if (
                        evento.key !== 'Enter' &&
                        evento.key !== ' '
                    ) {
                        return;
                    }

                    evento.preventDefault();

                    seleccionarResultadoDesdeElemento(
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
                        String(
                            $(this).attr(
                                'data-tipo'
                            ) ||
                            'TODOS'
                        )
                            .trim()
                            .toUpperCase();

                    $('.js-tab-coincidencias')
                        .removeClass('active')
                        .attr(
                            'aria-selected',
                            'false'
                        );

                    $(this)
                        .addClass('active')
                        .attr(
                            'aria-selected',
                            'true'
                        );

                    filtrarResultadosPorTipo(
                        tipo
                    );
                }
            );

        $(document)
            .off(
                'keydown.sicModalesFoto'
            )
            .on(
                'keydown.sicModalesFoto',
                function (evento) {

                    if (evento.key !== 'Escape') {
                        return;
                    }


                    if (
                        $('#sicModalComparacionFoto')
                            .hasClass('abierto')
                    ) {
                        cerrarComparacionFoto();

                        return;
                    }


                    if (
                        $('#sicModalFotoGrande')
                            .hasClass('abierto')
                    ) {
                        cerrarFotoGrande();
                    }
                }
            );

    }

    /* ============================================================
   DETALLE EN MODAL PARA TABLET
   ============================================================ */

    function esVistaTabletDetalle() {
        return window.matchMedia(
            '(max-width: 1100px)'
        ).matches;
    }


    function inicializarModalDetalleTablet() {
        asegurarModalDetalleTablet();

        $(document)
            .off(
                'click.sicModalDetalleTablet',
                '.js-cerrar-detalle-tablet'
            )
            .on(
                'click.sicModalDetalleTablet',
                '.js-cerrar-detalle-tablet',
                function (evento) {
                    evento.preventDefault();
                    evento.stopPropagation();

                    cerrarModalDetalleTablet();
                }
            );

        $(document)
            .off(
                'keydown.sicModalDetalleTablet'
            )
            .on(
                'keydown.sicModalDetalleTablet',
                function (evento) {
                    if (evento.key !== 'Escape') {
                        return;
                    }

                    if (
                        $('#sicModalFotoGrande').hasClass('abierto') ||
                        $('#sicModalComparacionFoto').hasClass('abierto')
                    ) {
                        return;
                    }

                    if (
                        $('#sicModalDetalleCoincidencia')
                            .hasClass('abierto')
                    ) {
                        cerrarModalDetalleTablet();
                    }
                }
            );

        $(window)
            .off(
                'resize.sicModalDetalleTablet'
            )
            .on(
                'resize.sicModalDetalleTablet',
                function () {
                    if (
                        !esVistaTabletDetalle() &&
                        $('#sicModalDetalleCoincidencia')
                            .hasClass('abierto')
                    ) {
                        cerrarModalDetalleTablet();
                    }
                }
            );
    }


    function asegurarModalDetalleTablet() {
        if (
            $('#sicModalDetalleCoincidencia')
                .length > 0
        ) {
            return;
        }

        const modal =
            $('<div>')
                .attr({
                    id:
                        'sicModalDetalleCoincidencia',

                    'aria-hidden':
                        'true'
                })
                .addClass(
                    'sic-modal-detalle-tablet'
                );

        const fondo =
            $('<button>')
                .attr({
                    type:
                        'button',

                    tabindex:
                        '-1',

                    'aria-label':
                        'Cerrar detalle'
                })
                .addClass(
                    'sic-modal-detalle-fondo js-cerrar-detalle-tablet'
                );

        const dialogo =
            $('<section>')
                .attr({
                    role:
                        'dialog',

                    'aria-modal':
                        'true',

                    'aria-labelledby':
                        'sicModalDetalleTitulo'
                })
                .addClass(
                    'sic-modal-detalle-dialogo'
                );

        const encabezado =
            $('<header>')
                .addClass(
                    'sic-modal-detalle-encabezado'
                )
                .html(
                    '<div class="sic-modal-detalle-titulo-wrap">' +
                    '<span class="sic-modal-detalle-icono">' +
                    '<i class="fa fa-id-card-o"></i>' +
                    '</span>' +

                    '<div>' +
                    '<h2 id="sicModalDetalleTitulo">' +
                    'Detalle de coincidencia' +
                    '</h2>' +

                    '<span id="sicModalDetalleSubtitulo">' +
                    'Información institucional del candidato seleccionado' +
                    '</span>' +
                    '</div>' +
                    '</div>' +

                    '<button type="button" ' +
                    'class="sic-modal-detalle-cerrar js-cerrar-detalle-tablet" ' +
                    'aria-label="Cerrar detalle">' +

                    '<i class="fa fa-times"></i>' +
                    '</button>'
                );

        const contenido =
            $('<div>')
                .attr({
                    id:
                        'sicModalDetalleContenido',

                    'aria-live':
                        'polite',

                    'aria-busy':
                        'false'
                })
                .addClass(
                    'sic-modal-detalle-contenido'
                );

        dialogo
            .append(
                encabezado
            )
            .append(
                contenido
            );

        modal
            .append(
                fondo
            )
            .append(
                dialogo
            );

        $('body')
            .append(
                modal
            );
    }


    function abrirModalDetalleTablet(
        elementoResultado
    ) {
        asegurarModalDetalleTablet();

        const nombre =
            elementoResultado &&
                elementoResultado.length > 0
                ? $.trim(
                    elementoResultado
                        .find(
                            '.sic-resultado-nombre'
                        )
                        .first()
                        .text()
                )
                : '';

        $('#sicModalDetalleSubtitulo')
            .text(
                nombre.length > 0
                    ? nombre
                    : 'Información institucional del candidato seleccionado'
            );

        $('#sicModalDetalleCoincidencia')
            .addClass(
                'abierto'
            )
            .attr(
                'aria-hidden',
                'false'
            );

        actualizarBloqueoScrollModales();

        window.setTimeout(
            function () {
                $('#sicModalDetalleCoincidencia')
                    .find(
                        '.sic-modal-detalle-cerrar'
                    )
                    .trigger(
                        'focus'
                    );
            },
            40
        );
    }


    function cerrarModalDetalleTablet() {
        $('#sicModalDetalleCoincidencia')
            .removeClass(
                'abierto'
            )
            .attr(
                'aria-hidden',
                'true'
            );

        actualizarBloqueoScrollModales();
    }


    function actualizarBloqueoScrollModales() {
        const hayModalAbierto =
            $('#sicModalDetalleCoincidencia')
                .hasClass('abierto') ||
            $('#sicModalFotoGrande')
                .hasClass('abierto') ||
            $('#sicModalComparacionFoto')
                .hasClass('abierto');

        $('body')
            .toggleClass(
                'sic-modal-abierto',
                hayModalAbierto
            );
    }


    function cerrarFotoGrande() {

        $('#sicModalFotoGrande')
            .removeClass(
                'abierto'
            )
            .attr(
                'aria-hidden',
                'true'
            );


        $('#sicFotoGrandeImagen')
            .attr(
                'src',
                ''
            );


        $('body')
            .removeClass(
                'sic-modal-abierto'
            );
    }

    function cerrarComparacionFoto() {

        $('#sicModalComparacionFoto')
            .removeClass(
                'abierto'
            )
            .attr(
                'aria-hidden',
                'true'
            );


        $('#sicComparacionFotoConsulta')
            .attr(
                'src',
                ''
            );


        $('#sicComparacionFotoCandidato')
            .attr(
                'src',
                ''
            );


        $('body')
            .removeClass(
                'sic-modal-abierto'
            );
    }

    function seleccionarResultadoDesdeElemento(
        elementoResultado,
        abrirDetalle
    ) {
        if (
            !elementoResultado ||
            elementoResultado.length === 0
        ) {
            return;
        }

        seleccionarCoincidencia(
            parseInt(
                elementoResultado.attr(
                    'data-id'
                ),
                10
            ),
            elementoResultado,
            abrirDetalle
        );
    }

    function seleccionarCoincidencia(
        idCoincidencia,
        elementoResultado,
        abrirDetalle
    ) {
        if (
            isNaN(idCoincidencia) ||
            idCoincidencia <= 0
        ) {
            return;
        }

        $('.sic-resultado-persona')
            .removeClass(
                'active'
            );

        if (
            elementoResultado &&
            elementoResultado.length > 0
        ) {
            elementoResultado
                .addClass(
                    'active'
                );
        }

        if (
            esVistaTabletDetalle()
        ) {
            if (
                abrirDetalle !== false
            ) {
                abrirModalDetalleTablet(
                    elementoResultado
                );

                cargarDetalleCoincidencia(
                    idCoincidencia,
                    true
                );
            }

            return;
        }

        cargarDetalleCoincidencia(
            idCoincidencia,
            false
        );
    }



    function filtrarResultadosPorTipo(
        tipo
    ) {
        $('.sic-resultado-persona')
            .each(function () {
                const registro =
                    $(this);

                const tieneTexto =
                    registro.attr(
                        'data-tiene-texto'
                    ) === 'true';

                const tieneFoto =
                    registro.attr(
                        'data-tiene-foto'
                    ) === 'true';

                const tieneHuella =
                    registro.attr(
                        'data-tiene-huella'
                    ) === 'true';

                const esCombinada =
                    registro.attr(
                        'data-es-combinada'
                    ) === 'true';

                let mostrar = false;

                switch (tipo) {
                    case 'TODOS':
                        mostrar = true;
                        break;

                    case 'TEXTO':
                        mostrar = tieneTexto;
                        break;

                    case 'FOTO':
                        mostrar = tieneFoto;
                        break;

                    case 'HUELLA':
                        mostrar = tieneHuella;
                        break;

                    case 'COMBINADA':
                        mostrar =
                            esCombinada ||
                            (tieneFoto && tieneHuella);
                        break;

                    default:
                        mostrar =
                            String(
                                registro.attr(
                                    'data-tipo'
                                ) || ''
                            )
                                .toUpperCase() === tipo;
                        break;
                }

                registro.toggle(
                    mostrar
                );
            });

        seleccionarPrimerResultadoVisible();
    }


    function seleccionarPrimerResultadoVisible() {
        if (
            $('.sic-resultado-persona.active:visible')
                .length > 0
        ) {
            return;
        }

        const primerResultadoVisible =
            $('.sic-resultado-persona:visible')
                .first();

        if (primerResultadoVisible.length > 0) {
            seleccionarResultadoDesdeElemento(
                primerResultadoVisible,
                !esVistaTabletDetalle()
            );

            return;
        }

        $('#panelDetalleCoincidencia')
            .html(
                '<div class="sic-detalle-vacio">' +
                '<i class="fa fa-filter"></i>' +
                '<strong>No hay resultados en esta categoría</strong>' +
                '<span>Seleccione otra pestaña para consultar las coincidencias.</span>' +
                '</div>'
            );
    }


    function cargarDetalleCoincidencia(
        idCoincidencia,
        usarModalTablet
    ) {
        const configuracion =
            window.sicCoincidenciasConfig || {};

        if (
            !configuracion.urlDetalle
        ) {
            mostrarMensaje(
                'No se configuró la dirección del detalle de coincidencia.',
                'error'
            );

            return;
        }

        cancelarSolicitudDetalle();

        const usarModal =
            usarModalTablet === true &&
            esVistaTabletDetalle();

        if (
            usarModal
        ) {
            asegurarModalDetalleTablet();
        }

        const panelDetalle =
            usarModal
                ? $('#sicModalDetalleContenido')
                : $('#panelDetalleCoincidencia');

        if (
            panelDetalle.length === 0
        ) {
            return;
        }

        panelDetalle
            .attr(
                'aria-busy',
                'true'
            )
            .html(
                '<div class="sic-detalle-loader">' +
                '<i class="fa fa-spinner fa-spin"></i>' +
                '<span>Cargando detalle...</span>' +
                '</div>'
            );

        const contexto =
            $('#sicResultadosContexto');

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
                        contexto.attr(
                            'data-tiene-fotografia'
                        ) === 'true',

                    tieneHuellaConsulta:
                        contexto.attr(
                            'data-tiene-huella'
                        ) === 'true'
                },

                success:
                    function (html) {
                        panelDetalle
                            .html(
                                html
                            );

                        sincronizarImagenesConsulta();
                    },

                error:
                    function (
                        xhr,
                        estado
                    ) {
                        if (
                            estado === 'abort'
                        ) {
                            return;
                        }

                        panelDetalle
                            .html(
                                '<div class="sic-detalle-vacio">' +
                                '<i class="fa fa-exclamation-circle"></i>' +
                                '<strong>No se pudo cargar el detalle</strong>' +
                                '<span>' +
                                escaparHtml(
                                    limpiarMensajeErrorServidor(
                                        xhr.responseText ||
                                        'No se pudo cargar el detalle.'
                                    )
                                ) +
                                '</span>' +
                                '</div>'
                            );
                    },

                complete:
                    function () {
                        solicitudDetalleActual =
                            null;

                        panelDetalle.attr(
                            'aria-busy',
                            'false'
                        );
                    }
            });
    }

    function cancelarSolicitudDetalle() {
        if (
            solicitudDetalleActual &&
            solicitudDetalleActual.readyState !== 4
        ) {
            solicitudDetalleActual.abort();
        }

        solicitudDetalleActual = null;
    }


    function sincronizarImagenesConsulta() {
        const fotoConsulta =
            ($('#previewFotografia').attr('src') || '')
                .trim();

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

        $('.js-foto-consulta-mini')
            .attr(
                'src',
                fotoConsulta || ''
            );

        $('.js-abrir-comparacion-foto')
            .toggle(
                !!fotoConsulta
            );

        $('.js-imagen-consulta-huella')
            .attr(
                'src',
                huellaConsulta || ''
            );

        $('.sic-comparaciones-seccion')
            .each(function () {
                $(this).toggle(
                    $(this)
                        .find(
                            '.sic-comparacion-card:visible'
                        )
                        .length > 0
                );
            });
    }


    /* ============================================================
       LIMPIEZA
       ============================================================ */

    function inicializarLimpieza() {
        $('#btnLimpiarBusqueda')
            .off(
                'click.sicCoincidencias'
            )
            .on(
                'click.sicCoincidencias',
                function () {
                    limpiarBusquedaCompleta();
                }
            );
    }


    function limpiarBusquedaCompleta() {
        cancelarSolicitudBusqueda();
        cancelarSolicitudDetalle();

        buscandoCoincidencias = false;

        limpiarArchivoBiometrico(
            'Fotografia'
        );

        limpiarHuellasSeleccionadas();

        $('#NombreBusqueda')
            .val('');

        $('#AliasBusqueda')
            .val('');

        $('#TextoBusqueda')
            .val('');

        establecerModoBusqueda(
            modoBusquedaPredeterminado,
            false
        );

        establecerModoCombinacion(
            modoCombinacionPredeterminado
        );

        $('#Municipio')
            .val('');

        $('#Sexo')
            .val('');

        $('#TipoCoincidencia')
            .val('');

        $('#EdadMinima')
            .val('');

        $('#EdadMaxima')
            .val('');

        $('#PorcentajeMinimo')
            .val(
                porcentajePredeterminado
            );

        $('#valorPorcentajeMinimo')
            .text(
                porcentajePredeterminado +
                '%'
            );

        establecerEstadoFiltrosAvanzados(
            false,
            false
        );

        actualizarConteoFiltrosAvanzados();
        restaurarEstadoInicialResultados();

        $('#sicBusquedaCoincidencias')
            .removeClass(
                'sic-modo-resultados'
            );

        establecerEstadoPanelCriterios(
            true,
            true
        );

        actualizarResumenCriteriosBusqueda();
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
                '<strong>Realice una búsqueda de coincidencias</strong>' +
                '<span>' +
                'Escriba un nombre o alias, agregue una fotografía, una huella o combine varios elementos.' +
                '</span>'
            );
    }


    function actualizarEstadoBotonBusqueda() {
        const habilitar =
            !buscandoCoincidencias &&
            (
                tieneNombreBusquedaValido() ||
                tieneAliasBusquedaValido() ||
                inputTieneArchivo('Fotografia') ||
                inputTieneArchivo('Huellas')
            );

        $('#btnBuscarCoincidencias')
            .prop(
                'disabled',
                !habilitar
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

        alert(
            mensaje
        );
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
                .html(
                    mensaje
                )
                .text()
                .replace(
                    /\s+/g,
                    ' '
                )
                .trim();

        return texto.length > 500
            ? texto.substring(0, 500) + '...'
            : texto;
    }


    function inicializarMandamientosDesplegables() {
        $(document)
            .off(
                'click.sicMandamientos',
                '.js-toggle-mandamientos'
            )
            .on(
                'click.sicMandamientos',
                '.js-toggle-mandamientos',
                function () {
                    const $boton =
                        $(this);

                    const selectorPanel =
                        $boton.attr(
                            'data-target'
                        );

                    if (!selectorPanel) {
                        return;
                    }

                    const $panel =
                        $(selectorPanel);

                    if (!$panel.length) {
                        return;
                    }

                    const estaAbierto =
                        $boton.attr(
                            'aria-expanded'
                        ) === 'true';


                    if (estaAbierto) {
                        $panel
                            .stop(true, true)
                            .slideUp(220);

                        $panel.attr(
                            'aria-hidden',
                            'true'
                        );

                        $boton.attr(
                            'aria-expanded',
                            'false'
                        );

                        $boton
                            .closest(
                                '.sic-mandamientos-alerta'
                            )
                            .removeClass(
                                'sic-mandamientos-abierto'
                            )
                            .addClass(
                                'sic-mandamientos-colapsado'
                            );
                    }
                    else {
                        $panel
                            .stop(true, true)
                            .slideDown(220);

                        $panel.attr(
                            'aria-hidden',
                            'false'
                        );

                        $boton.attr(
                            'aria-expanded',
                            'true'
                        );

                        $boton
                            .closest(
                                '.sic-mandamientos-alerta'
                            )
                            .removeClass(
                                'sic-mandamientos-colapsado'
                            )
                            .addClass(
                                'sic-mandamientos-abierto'
                            );
                    }
                }
            );
    }


})(jQuery);
