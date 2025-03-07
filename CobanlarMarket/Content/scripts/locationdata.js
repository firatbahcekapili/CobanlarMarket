$(document).ready(function () {
    loadIller();

    // İl seçimi değiştiğinde ilçe ve mahalleleri yükle
    $("#il").change(function () { loadIlceler("Add"); });
    $("#ilce").change(function () { loadMahalleler("Add"); });

    $("#ilEdit").change(function () { loadIlceler("Edit"); });
    $("#ilceEdit").change(function () { loadMahalleler("Edit"); });
});

function loadIller() {
    $.ajax({
        url: '/Home/GetIller',
        type: 'GET',
        success: function (data) {
            console.log(data);
            const ilSelect = $("#il");
            const ilEditSelect = $("#ilEdit");

            ilSelect.empty().append('<option value="">İl Seçin</option>');
            ilEditSelect.empty().append('<option value="">İl Seçin</option>');

            $.each(data, function (index, il) {
                ilSelect.append($('<option></option>').val(il.Name).text(il.Name).attr('id', 'il_' + index));
                ilEditSelect.append($('<option></option>').val(il.Name).text(il.Name).attr('id', 'ilEdit_' + index)); // id'si farklı olsun
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading iller:', error);
        }
    });
}

function loadIlceler(type) {
    return new Promise((resolve, reject) => {
        var il = (type === "Edit") ? $("#ilEdit") : $("#il");
        var ilce = (type === "Edit") ? $("#ilceEdit") : $("#ilce");
        var mahalle = (type === "Edit") ? $("#mahalleEdit") : $("#mahalle");

        if (!il.val()) {
            ilce.empty().append('<option value="" disabled selected id="ilce_-1">İlçe Seçin</option>');
            ilce.prop("disabled", true);
            mahalle.empty().append('<option value="" disabled selected id="mahalle_-1">Mahalle Seçin</option>');
            mahalle.prop("disabled", true);
            resolve();
            return;
        }

        $.ajax({
            url: `/Home/GetIlceler?il=${encodeURIComponent(il.val())}`,
            type: 'GET',
            success: function (data) {
                ilce.removeAttr("disabled").empty().append('<option value="" disabled selected id="ilce_-1">İlçe Seçin</option>');
                mahalle.empty().append('<option value="" disabled selected id="mahalle_-1">Mahalle Seçin</option>');
                mahalle.prop("disabled", true);

                $.each(data, function (index, ilcee) {
                    ilce.append($('<option></option>').val(ilcee.Name).text(ilcee.Name).attr('id', 'ilce_' + index));
                });
                resolve(); // İşlem başarılıysa çözülür
            },
            error: function (xhr, status, error) {
                console.error('Error loading ilceler:', error);
                reject(error); // Hata varsa reddedilir
            }
        });
    });
}

function loadMahalleler(type) {
    return new Promise((resolve, reject) => {
        var il = (type === "Edit") ? $("#ilEdit") : $("#il");
        var ilce = (type === "Edit") ? $("#ilceEdit") : $("#ilce");
        var mahalle = (type === "Edit") ? $("#mahalleEdit") : $("#mahalle");

        if (!il.val() || !ilce.val()) {
            mahalle.prop("disabled", true);
            mahalle.empty().append('<option value="" disabled selected id="mahalle_-1">Mahalle Seçin</option>');
            resolve();
            return;
        }

        $.ajax({
            url: `/Home/GetMahalleler?il=${encodeURIComponent(il.val())}&ilce=${encodeURIComponent(ilce.val())}`,
            type: 'GET',
            success: function (data) {
                mahalle.prop("disabled", false).empty().append('<option value="" disabled selected id="mahalle_-1">Mahalle Seçin</option>');
                $.each(data, function (index, mahallee) {
                    mahalle.append($('<option></option>').val(mahallee.Name).text(mahallee.Name).attr('id', 'mahalle_' + index));
                });
                resolve(); // İşlem başarılıysa çözülür
            },
            error: function (xhr, status, error) {
                console.error('Error loading mahalleler:', error);
                reject(error); // Hata varsa reddedilir
            }
        });
    });
}