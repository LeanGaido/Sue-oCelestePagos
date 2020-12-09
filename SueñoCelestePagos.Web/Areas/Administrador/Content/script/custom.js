$(document).ready(function () {
    if ($('#ObraSocialId').length && !$('#editPaciente').length) {
        CargarPlanes();
    }

    $('#ObraSocialId').change(function () {
        CargarPlanes();
    });

    /******************************* Inicio Conf de Campos de la Carga del Detalle de la Prestacion *******************************/

    if ($('#ObraSocialIdCamposConfig').length) {
        RecargarConfig();
    }

    $('#ObraSocialIdCamposConfig').change(function (e) {
        RecargarConfig();
    });

    $('#PlanIdCamposConfig').change(function (e) {
        RecargarConfig();
    });

    $('#CodPracticaIdCamposConfig').change(function (e) {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();

        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        CargarCamposReq(ObraSocialId, CodPractica);
        CargarCamposVisibles(ObraSocialId, CodPractica);
        CargarCamposMultiples(ObraSocialId, CodPractica);

        CargarEsConsulta();
        CargarFueraDelLimite();
        CargarDibujoPractica();
        CargarCooldown();
    });


    $('#btnGuardarCamposConfig').click(function (e) {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        GuardarCamposReq(ObraSocialId, CodPractica);
        GuardarCamposVisibles(ObraSocialId, CodPractica);
        GuardarCamposMultiples(ObraSocialId, CodPractica);
    });

    $('#btnRecargarCamposConfig').click(function (e) {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        CargarCamposReq(ObraSocialId, CodPractica);
        CargarCamposVisibles(ObraSocialId, CodPractica);
        CargarCamposMultiples(ObraSocialId, CodPractica);
    });


    $('#btnCantPracticasMax').click(function (e) {
        GuardarCantPracticasMax();
    });

    $('#btnRecargarCantPracticasMax').click(function (e) {
        CargarCantPracticasMax();
    });

    $('#btnGuardarCooldown').click(function (e) {
        GuardarCooldown();
    });


    $('#btnLimite').click(function (e) {
        toggleSwitch('Limite', true);
        $('#Limite').prop("checked", true);
    });

    $('#btnNoLimite').click(function (e) {
        toggleSwitch('Limite', false);
        $('#Limite').prop("checked", false);
    });

    /* Guardar si es o no Consulta */
    $('#btnEsConsulta').click(function (e) {
        toggleSwitch('EsConsulta', true);
        $('#esConsulta').prop("checked", true);
        GuardarEsConsulta();
    });

    $('#btnNoEsConsulta').click(function (e) {
        toggleSwitch('EsConsulta', false);
        $('#esConsulta').prop("checked", false);
        GuardarEsConsulta();
    });

    /* Guardar si entra o no en el Limite */
    $('#btnFueraDelLimite').click(function (e) {
        toggleSwitch('FueraDelLimite', true);
        $('#fueraDelLimite').prop("checked", true);
        GuardarFueraDelLimite();
    });

    $('#btnNoFueraDelLimite').click(function (e) {
        toggleSwitch('FueraDelLimite', false);
        $('#fueraDelLimite').prop("checked", false);
        GuardarFueraDelLimite();
    });

    $('.btn-dibujo').click(function (e) {
        e.preventDefault();
        if (!$(this).hasClass("btn-dibujo-disabled")) {
            var Dibujo = 0;
            if (!$(this).hasClass("btn-dibujo-selected")) {
                if ($('.btn-dibujo-selected').length) {
                    $('.btn-dibujo-selected').removeClass("btn-dibujo-selected");
                }
                $(this).addClass("btn-dibujo-selected");

                Dibujo = $(this).data('dibujo');
            }
            else {
                $(this).removeClass("btn-dibujo-selected");
            }
            GuardarDibujoPractica(Dibujo);
        } else {
            //Evito que guarde el dibujo, cuando el "boton" esta desabilitado
        }
    });

    /* Botones de Configuracion de campos del Detalle Prestacion */

    /* Dientes */
    $('#btnNrosDientes').click(function (e) {
        toggleSwitch('NrosDientes', true);
        $('#ReqNrosDientes').prop("checked", true);
        toggleSwitch('ShowNrosDientes', true);
        $('#ShowNrosDientes').prop("checked", true);
    });

    $('#btnNoNrosDientes').click(function (e) {
        toggleSwitch('NrosDientes', false);
        $('#ReqNrosDientes').prop("checked", false);
    });

    $('#btnShowNrosDientes').click(function (e) {
        toggleSwitch('ShowNrosDientes', true);
        $('#ShowNrosDientes').prop("checked", true);
    });

    $('#btnNoShowNrosDientes').click(function (e) {
        toggleSwitch('ShowNrosDientes', false);
        $('#ShowNrosDientes').prop("checked", false);
        toggleSwitch('NrosDientes', false);
        $('#ReqNrosDientes').prop("checked", false);
    });

    $('#btnMultipleNrosDientes').click(function (e) {
        toggleSwitch('MultipleNrosDientes', true);
        $('#MultipleNrosDientes').prop("checked", true);

        toggleSwitch('ShowNrosDientes', true);
        $('#ShowNrosDientes').prop("checked", true);

        $('#btnNoCaras').click();
        $('#btnNoShowCaras').click();
        $('#btnNoMultipleCaras').click();

        toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', true);
        toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', false);
    });

    $('#btnNoMultipleNrosDientes').click(function (e) {
        toggleSwitch('MultipleNrosDientes', false);
        $('#MultipleNrosDientes').prop("checked", false);

        toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', false);
        toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', true);
    });

    /* Caras */
    $('#btnCaras').click(function (e) {
        toggleSwitch('Caras', true);
        $('#ReqCaras').prop("checked", true);
        toggleSwitch('ShowCaras', true);
        $('#ShowCaras').prop("checked", true);
    });

    $('#btnNoCaras').click(function (e) {
        toggleSwitch('Caras', false);
        $('#ReqCaras').prop("checked", false);
    });

    $('#btnShowCaras').click(function (e) {
        toggleSwitch('ShowCaras', true);
        $('#ShowCaras').prop("checked", true);
    });

    $('#btnNoShowCaras').click(function (e) {
        toggleSwitch('ShowCaras', false);
        $('#ShowCaras').prop("checked", false);
        toggleSwitch('Caras', false);
        $('#ReqCaras').prop("checked", false);
    });

    $('#btnMultipleCaras').click(function (e) {
        toggleSwitch('MultipleCaras', true);
        $('#MultipleCaras').prop("checked", true);

        toggleSwitch('ShowCaras', true);
        $('#ShowCaras').prop("checked", true);

        toggleSwitch('MultipleNrosDientes', false);
        $('#MultipleNrosDientes').prop("checked", false);

        toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', false);
        toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', true);
    });

    $('#btnNoMultipleCaras').click(function (e) {
        toggleSwitch('MultipleCaras', false);
        $('#MultipleCaras').prop("checked", false);

        //toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', true);
        //toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', false);
    });

    /* Observacion */
    $('#btnObservacion').click(function (e) {
        toggleSwitch('Observacion', true);
        $('#ReqObservacion').prop("checked", true);
        toggleSwitch('ShowObservacion', true);
        $('#ShowObservacion').prop("checked", true);
    });

    $('#btnNoObservacion').click(function (e) {
        toggleSwitch('Observacion', false);
        $('#ReqObservacion').prop("checked", false);
    });

    $('#btnShowObservacion').click(function (e) {
        toggleSwitch('ShowObservacion', true);
        $('#ShowObservacion').prop("checked", true);
    });

    $('#btnNoShowObservacion').click(function (e) {
        toggleSwitch('ShowObservacion', false);
        $('#ShowObservacion').prop("checked", false);
        toggleSwitch('Observacion', false);
        $('#ReqObservacion').prop("checked", false);
    });


    $('#btnTodoElDientoOdontograma').click(function (e) {
        toggleSwitch('TodoElDientoOdontograma', true);
        toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', true);
        toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', false);
    });

    $('#btnNoTodoElDientoOdontograma').click(function (e) {
        toggleSwitch('TodoElDientoOdontograma', false);
        toggleBtnDibujoDisabled('.btnDibujoCaraDelDiente', false);
        toggleBtnDibujoDisabled('.btnDibujoTodoElDiente', true);
    });

/* Botones de Configuracion de campos del Detalle Prestacion */

    function RecargarConfig() {

        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();

        var PlanId = $('#PlanIdCamposConfig').val();

        CargarPlanesCamposReq();
        $('#PlanIdCamposConfig').val(PlanId);

        CargarCantPracticasMax();

        $("#CodPracticaIdCamposConfig").empty();
        CargarPracticasCamposReq($('#PlanIdCamposConfig').val());

        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        CargarCamposReq($('#ObraSocialIdCamposConfig').val(), CodPractica);
        CargarCamposVisibles($('#ObraSocialIdCamposConfig').val(), CodPractica);
        CargarCamposMultiples($('#ObraSocialIdCamposConfig').val(), CodPractica);

        CargarEsConsulta();
        CargarFueraDelLimite();
        CargarDibujoPractica();
        CargarCooldown();
    }

    function CargarCantPracticasMax() {
        var PlanId = $('#PlanIdCamposConfig').val();
        $.ajax({
            type: "GET",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetCantPracticasMax',
            data: {
                PlanId: PlanId
            },
            dataType: 'json',
            success: function (data) {
                $('#tab-general').show();
                $('#confCantPracticasMax').val(data.CantPracticasMaximas);
                $('#Limite').prop('checked', data.Limite);
                toggleSwitch('Limite', data.Limite);
                
            },
            error: function (ex) {
                $("#CodPracticaIdCamposConfig").empty();
                $("#CodPracticaIdCamposConfig").val('');

                $('#tab-general').hide();
            }
        });
    }

    function CargarPlanesCamposReq() {
        $("#PlanIdCamposConfig").empty();
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ObrasSociales/GetPlanes',
            data: {
                ObraSocialId: $("#ObraSocialIdCamposConfig").val()
            },
            dataType: 'json',
            success: function (data) {
                $('#panelConfig').show();
                $.each(data, function (i, data) {
                    $('#PlanIdCamposConfig').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                });
            },
            error: function (ex) {
                $('#panelConfig').hide();
            }
        });
    }

    function CargarPracticasCamposReq(PlanId) {
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/Planes/GetPracticasCamposReq',
            data: {
                PlanId: PlanId
            },
            dataType: 'json',
            success: function (Data) {
                $('#panelConfig').show();
                $.each(Data, function (i, Data) {
                    $("#CodPracticaIdCamposConfig").append('<option value="'
                        + Data.ID + '">'
                        + Data.CodPractica + '</option>');
                });
            },
            error: function (ex) {
                $('#panelConfig').hide();
            }
        });
    }


    function CargarCamposReq(ObraSocialId, CodPractica) {
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetCamposRequeridos',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (Data) {
                $('#panelConfig').show();
                $.each(Data, function (i, Data) {
                    //alert(Data.CampoName + ' ' + Data.Requerido);
                    if (Data.Requerido) {
                        $('#Req' + Data.CampoName).prop("checked", true);
                    }
                    else {
                        $('#Req' + Data.CampoName).prop("checked", false);
                    }
                    toggleSwitch(Data.CampoName, Data.Requerido);
                });
            },
            error: function (ex) {
                $('#panelConfig').hide();
            }
        });
    }

    function CargarCamposVisibles(ObraSocialId, CodPractica) {
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetCamposVisibles',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (Data) {
                $.each(Data, function (i, Data) {
                    //alert("Coming Soon");
                    //alert(Data.CampoName + ' ' + Data.Requerido);
                    if (Data.Visible) {
                        $('#Show' + Data.CampoName).prop("checked", true);
                    }
                    else {
                        $('#Show' + Data.CampoName).prop("checked", false);
                    }
                    toggleSwitch('Show' + Data.CampoName, Data.Visible);
                });
            },
            error: function (ex) {

            }
        });
    }

    function CargarCamposMultiples(ObraSocialId, CodPractica) {
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetCamposMultiples',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (Data) {
                $.each(Data, function (i, Data) {
                    //alert("Coming Soon");
                    //alert(Data.CampoName + ' ' + Data.Requerido);
                    if (Data.Multiple) {
                        $('#Multiple' + Data.CampoName).prop("checked", true);
                    }
                    else {
                        $('#Multiple' + Data.CampoName).prop("checked", false);
                    }
                    toggleSwitch('Multiple' + Data.CampoName, Data.Multiple);
                    $('#CantMax' + Data.CampoName).val(Data.CantMax)
                });
            },
            error: function (ex) {

            }
        });
    }


    function CargarEsConsulta() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetEsConsulta',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (data) {
                if (data) {
                    $('#esConsulta').prop('checked', true);
                    $('#btnEsConsulta').click();
                } else {
                    $('#esConsulta').prop('checked', false);
                    $('#btnNoEsConsulta').click();
                }
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function CargarFueraDelLimite() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetEsFueraDelLimite',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (data) {
                if (data) {
                    $('#fueraDelLimite').prop('checked', true);
                    $('#btnFueraDelLimite').click();
                } else {
                    $('#fueraDelLimite').prop('checked', false);
                    $('#btnNoFueraDelLimite').click();
                }
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function CargarDibujoPractica() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetDibujoPractica',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (data) {
                if ($('.btn-dibujo-selected').length) {

                    $('.btn-dibujo-selected').removeClass("btn-dibujo-selected");
                }

                $('.btn-dibujo[data-dibujo="' + data + '"]').addClass('btn-dibujo-selected');

                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function CargarCooldown() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/GetCooldown',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica
            },
            dataType: 'json',
            success: function (data) {
                if ($('#Cooldown').length) {
                    $('#Cooldown').val(data)
                }
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }


    function GuardarEsConsulta() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetEsConsulta',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                EsConsulta: $('#esConsulta').prop('checked')
            },
            dataType: 'json',
            success: function (data) {
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarFueraDelLimite() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetEsFueraDelLimite',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                FueraDelLimite: $('#fueraDelLimite').prop('checked')
            },
            dataType: 'json',
            success: function (data) {
                
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarDibujoPractica(Dibujo) {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetDibujoPractica',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                Dibujo: Dibujo
            },
            dataType: 'json',
            success: function (data) {
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarCantPracticasMax() {
        var PlanId = $('#PlanIdCamposConfig').val();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetCantPracticasMax',
            data: {
                PlanId: PlanId,
                CantPracticasMax: $('#confCantPracticasMax').val(),
                Limite: $('#Limite').prop('checked')
            },
            dataType: 'json',
            success: function (data) {
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarCooldown() {
        var ObraSocialId = $('#ObraSocialIdCamposConfig').val();
        var CodPractica = $('#CodPracticaIdCamposConfig option:selected').text();
        var Cooldown = $('#Cooldown').val();

        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetCooldown',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                Cooldown: Cooldown
            },
            dataType: 'json',
            success: function (data) {
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }


    function GuardarCamposReq(ObraSocialId, CodPractica) {
        $.ajax({
            type: "GET",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetCamposRequeridos',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                ReqNrosDientes: $('#ReqNrosDientes').prop('checked'),
                ReqCaras: $('#ReqCaras').prop('checked'),
                ReqObservacion: $('#ReqObservacion').prop('checked')
            },
            dataType: 'json',
            success: function (data) {
                //$.each(data, function (i, data) {
                //    $('#PlanIdCamposReq').append('<option value = "' + data.ID + '">' + data.Nombre + '</option>')
                //});
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarCamposVisibles(ObraSocialId, CodPractica) {
        $.ajax({
            type: "GET",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetCamposVisibles',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                ShowNrosDientes: $('#ShowNrosDientes').prop('checked'),
                ShowCaras: $('#ShowCaras').prop('checked'),
                ShowObservacion: $('#ShowObservacion').prop('checked')
            },
            dataType: 'json',
            success: function (data) {
               
            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }

    function GuardarCamposMultiples(ObraSocialId, CodPractica) {
        $.ajax({
            type: "GET",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ConfDetallePrestacion/SetCamposMultiples',
            data: {
                ObraSocialId: ObraSocialId,
                CodPractica: CodPractica,
                MultipleNrosDientes: $('#MultipleNrosDientes').prop('checked'),
                MultipleCaras: $('#MultipleCaras').prop('checked'),
                CantMaxNrosDientes: $('#CantMaxNrosDientes').val(),
                CantMaxCaras: $('#CantMaxCaras').val()
            },
            dataType: 'json',
            success: function (data) {

            },
            error: function (ex) {
                alert('Error, por favor intente mas tarde.');
            }
        });
    }


    function toggleSwitch(CampoName, Toggle) {
        //alert(CampoName + ' - ' + Toggle)
        if (Toggle) {
            $('#btn' + CampoName).addClass("btn-primary");
            $('#btnNo' + CampoName).removeClass("btn-primary");
        }
        else {
            $('#btnNo' + CampoName).addClass("btn-primary");
            $('#btn' + CampoName).removeClass("btn-primary");
        }
    }

    function toggleBtnDibujoDisabled(btnDibujo, toggle) {
        if (toggle) {
            $(btnDibujo).addClass('btn-dibujo-disabled');
        } else {
            $(btnDibujo).removeClass('btn-dibujo-disabled');
        }
    }


/******************************* Inicio Conf de Campos de la Carga del Detalle de la Prestacion *******************************/

    function CargarPlanes() {
        //var plan = $("#PlanId").val();
        //$("#PlanId").empty();
        $('#planesArancel').empty();
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/ObrasSociales/GetPlanes',
            data: {
                ObraSocialId: $("#ObraSocialId").val()
            },
            dataType: 'json',
            success: function (data) {
                $.each(data, function (i, data) {
                    $('#planesArancel').append('<input type = "checkbox" name = "PlanesId[]" value = "' + data.ID + '" /><label> ' + data.Nombre + '</label> <br/>')
                });
            },
            error: function (ex) {
                alert('Selecciona una Provincia Valida.');
            }
        });
    }

    /****************************** Aranceles ******************************/
    $('#ObraSocialIdAranceles').change(function () {
        $('#btnFiltrarAranceles').click();
    });
});