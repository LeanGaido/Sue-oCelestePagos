$(document).ready(function () {
    
    if ($('#ProgressBarAvisoDeuda').length) {
        let ComprasConDeuda = 0;
        //ObtenerComprasConDeuda
        $.ajax({
            type: "GET",
            traditional: true,
            async: false,
            cache: false,
            url: '/ContentAdmin/Compras/ObtenerComprasConDeuda',
            data: {
            },
            dataType: 'json',
            success: function (Compras) {
                ComprasConDeuda = data.length;
                $.each(Compras, function (i, data) {
                    setTimeout(function () {
                        $.ajax({
                            type: "GET",
                            traditional: true,
                            async: false,
                            cache: false,
                            url: '/ContentAdmin/Compras/EnvioAvisoDeuda',
                            data: {
                                id : data.ID
                            },
                            dataType: 'json',
                            success: function (data) {
                                
                            },
                            error: function (ex) {
                            }
                        });
                    }, 3000);
                });
            },
            error: function (ex) {
            }
        });
    }

    moveProgressBar();
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