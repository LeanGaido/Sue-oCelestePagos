$(document).ready(function () {
    
    if ($('#ProgressBarAvisoDeuda').length) {
        let ComprasConDeuda = 0;
        //ObtenerComprasConDeuda
        $.ajax({
            type: "POST",
            traditional: true,
            async: false,
            cache: false,
            url: '/Administrador/Compras/ObtenerComprasConDeuda',
            data: {
            },
            dataType: 'json',
            success: function (Compras) {
                ComprasConDeuda = Compras.length;
                $("#NroTotalCorreos").text(ComprasConDeuda);
                var time = 5000;
                $.each(Compras, function (i, item) {
                    //for (var i = 0; i < ComprasConDeuda; i++) {
                    setTimeout(function () {
                        $.ajax({
                            type: "POST",
                            traditional: true,
                            async: false,
                            cache: false,
                            url: '/Administrador/Compras/EnvioAvisoDeuda',
                            data: {
                                id: item.ID
                            },
                            dataType: 'json',
                            success: function (data) {
                                var porcentaje = ((i + 1) / ComprasConDeuda) * 100;
                                $('.progress-bar').width(porcentaje + '%');
                                $("#NroCorreo").text(i + 1);
                                    
                                if (i == Compras.length - 1) {
                                    $("#tituloEnvioCorreos").text("Correos Enviado Correctamente");
                                    tituloEnvioCorreos
                                    //alert("last");
                                }
                            },
                            error: function (ex) {
                                    
                            }
                        });
                    }, time)
                    time += 5000;
                    //}
                });
            },
            error: function (ex) {
            }
        });
    }

    //moveProgressBar();
    // on browser resize...
    $(window).resize(function () {
        moveProgressBar();
    });

    // SIGNATURE PROGRESS
    function moveProgressBar() {
        console.log("moveProgressBar");
        var getPercent = ($('.progress-wrap').data('progress-percent') / 100);
        var getProgressWrapWidth = $('.progress-wrap').width();
        var progressTotal = getPercent * getProgressWrapWidth;
        var animationLength = 2500;

        // on page load, animate percentage bar to data percentage length
        // .stop() used to prevent animation queueing
        $('.progress-bar').stop().animate({
            left: progressTotal
        }, animationLength);
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
});