var DateTimePickerProperties = { locale: 'es', format: 'DD-MM-YYYY', defaultDate: new Date() };

function fillCbo(data, nameCbo, selectDefault) {

    let cbo = $("#" + nameCbo);
    cbo.empty();
    if (data.length == 0)
        cbo.append($("<option value='-1'>No existe datos</option>"));
    else {
        let items = [];
        $.each(data, function (i, item) {

            let attr = (item.Attr != null ? "attr='" + item.Attr + "'" : "");
            items.push("<option value='" + item.Value + "' " + attr + " >" + item.Text + "</option>");
        });
        cbo.append(items);
        if (selectDefault != undefined && selectDefault != null && selectDefault!="")
            cbo.find("option[value='" + selectDefault + "']").prop("selected", true);
    }
}

function getData(action, paramsData) {    
    var post = {
        Action: action,
        Params: paramsData
    }
    return ExecutePostAjaxPromise(post);
}

function ExecutePostAjaxPromise(post) {
    return $.ajax({
        url: post.Action,
        type: 'POST',
        async: true,
        data: post.Params
    }).promise();
}

jQuery.fn.visible = function () {
    return this.css('visibility', 'visible');
};

jQuery.fn.invisible = function () {
    return this.css('visibility', 'hidden');
};

jQuery.fn.visibilityToggle = function () {
    return this.css('visibility', function (i, visibility) {
        return (visibility == 'visible') ? 'hidden' : 'visible';
    });
};

jQuery.fn.limpiar = function () {
    let obj = $(this);
    obj.val("");
    obj.parent().removeClass("has-error");
    
};

function convertDateFormat(string) {
    let info = "";
    if (string != null & string!="")
        info = string.split('-').reverse().join('/');

    return info;
}


function validaCamposRequeridos(contenedor, fnc) {    
    let erroresMsj = [];

    $("#msjConfirmacion").hide();
    $("#message").html("");
    $("#" + contenedor + " .has-error").removeClass("has-error");

    $("#" + contenedor +" input[campo-required=true]").each(function (index, elem) {

        let objE = $(elem);
        if (!objE.hasClass("tt-hint")) {
            if ($.trim(objE.val()).length == 0) {
                erroresMsj.push(objE.attr("campo-message"))
                objE.parent().addClass("has-error");                
            }
        }
    });

    if (fnc!=null)
        erroresMsj = fnc(erroresMsj);

    if (erroresMsj.length>0)
        printMsjError("alert-danger", erroresMsj.join('<br>'));
    
    return erroresMsj.length==0;
}

function printMsjError(cls, msj) {
    let objMsj = $("#msjConfirmacion");
    objMsj.attr("class", "alert " + cls);
    objMsj.show();
    $("#message").html("");
    $("#message").append(msj);
}
function SetControlInError(controlName) {
    let objSet = typeof controlName == 'string' ? $('#' + controlName)["0"] : controlName;
    if (typeof objSet === 'object') {
        if (objSet.type == 'select-multiple')
            $(objSet.parentElement).addClass("select2-has-error");
        else
            $(objSet.parentElement).addClass("has-error");

        objSet.focus();
    }
}
function unSetControlInError(controlName) {
    let objUnSet = typeof controlName == 'string' ? $('#' + controlName)["0"] : controlName;
    if (typeof objUnSet === 'object') {
        if (objUnSet.type == 'select-multiple')
            $(objUnSet.parentElement).removeClass("select2-has-error");
        else
            $(objUnSet.parentElement).removeClass("has-error");
    }
}
function SetControlInErrorCollection(coll_controlName) {
    if (coll_controlName != null && coll_controlName.length > 0) {
        for (var i = 0; i < coll_controlName.length; i++) {
            var objName = coll_controlName[i];
            SetControlInError(objName);
        }
    }
}
function unSetControlInErrorCollection(coll_controlName) {
    if (coll_controlName != null && coll_controlName.length > 0) {
        for (var i = 0; i < coll_controlName.length; i++) {
            var objName = coll_controlName[i];
            unSetControlInError(objName);
        }
    }
}
function isValidaForm(coll_objs) {
    let isValid = true;
    unSetControlInErrorCollection(coll_objs);
    for (var i = 0; i < coll_objs.length; i++) {
        let obj = $('#' + coll_objs[i])["0"];
        if (typeof obj === 'object') {
            if (obj.validationMessage != '') {
                SetControlInError(obj);
                isValid = false;
            }
        }
    }
    return isValid;
}