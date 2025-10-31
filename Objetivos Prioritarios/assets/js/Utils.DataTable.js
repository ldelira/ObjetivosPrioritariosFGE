var dataTable_language = {
    lengthMenu: 'Mostrar _MENU_ registros por página.',
    info: 'Mostrando página _PAGE_ de _PAGES_',
    zeroRecords: 'No registros encontrados.',
    infoEmpty: 'No registros disponibles',
    infoFiltered: '(Se encontrarón _MAX_ registros.)',
    paginate: {
        first: 'Primero',
        last: 'Último',
        next: 'Siguiente',
        previous: 'Anterior'
    }
};
/* Ejemplo de llamado para agregar en una columna varias aciones.
    data: "AppGrouperId", render: function (data, type, full, meta) {
            var actionParamsArray = [
                { divId: id, data: data, functionToExec: 'alert', toolTip: 'Eliminar', icon: 'fa fa-trash' },
                { divId: id, data: data, functionToExec: 'alert', toolTip: 'Nuevo', icon: 'fa fa-plus' }
            ];
            return AddActionsToGrid(actionParamsArray);
        }
*/
function AddActionsToGrid(actionParamsArray) {
    var htmlToAddActionsGrid = '';
    if (actionParamsArray != undefined) {
        for (var x = 0; x < actionParamsArray.length; x++) {
            htmlToAddActionsGrid += AddActionToGrid(actionParamsArray[x].divId, actionParamsArray[x].data, actionParamsArray[x].functionToExec, actionParamsArray[x].toolTip, actionParamsArray[x].icon, actionParamsArray[x].html);
        }
    }
    return htmlToAddActionsGrid;
}
function AddIconToGrid(toolTipText, faIcon) {
    return AddActionToGrid(null, null, null, toolTipText, faIcon);
}

function AddIconToGrid(divId, toolTipText, faIcon) {
    return AddActionToGrid(divId, null, null, toolTipText, faIcon);
}

function AddActionToGrid(divId, data, functionNameString, toolTipText, faIcon, html) {
    var tag = '<div class="div-action-table" ';
    if (divId != null && divId != '') {
        tag = '<div class="div-action-table" id="' + divId + '"';
    }
    if (functionNameString == null || functionNameString == '') {
        //return '<div style="text-align:center;"><span class="action-icon"><i class="' + faIcon + '" data-toggle="tooltip" data-placement="top" title="' + toolTipText + '" class="btn btn-xs btn-primary"></i></span></div>';
        tag += ' style="text-align:center;"><span class="action-icon"><i class="' + faIcon + '" data-toggle="tooltip" data-placement="top" title="' + toolTipText + '" ></i></span>';
    }
    else {
        data = "'" + data + "'";
        tag += ' style="text-align:center;"><span class="action-icon"><a href="javascript:' + functionNameString + '(' + data + ');" data-toggle="tooltip" data-placement="top" title="' + toolTipText + '" class="btn-action-table"><i class="' + faIcon + '"></i></a></span>';
    }

    if (html == null || html == undefined)
        html = "";

    tag += html + "</div>";

    return tag;
}

