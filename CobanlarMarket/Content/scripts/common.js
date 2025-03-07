function validatePhoneNumber(phoneNumber) {
    // Numaranın 10 haneli olmasını veya başka kuralları kontrol edin
    var phoneRegex = /([(]?)([5])([0-9]{2})([)]?)([\s]?)([0-9]{3})([\s]?)([0-9]{2})([\s]?)([0-9]{2})$/;
    return phoneRegex.test(phoneNumber);
}

$(document).ready(function () {


    //$(function () {
    //    $('#tt').bootstrapTable()
    //})

    $('#submitbtn').on("click", function (e) {
        e.preventDefault();

        if ($('#AddressForm').valid()) {
            var formData = new FormData();

            var il = $('#il').val();
            var ilce = $('#ilce').val();
            var mahalle = $('#mahalle').val();

            if (!il || !ilce || !mahalle) {
                toastr.error("İl, İlce ve Mahalle Seçilmelidir.")
                return;
            }



            var name = $('#name').val();
            var surname = $('#surname').val();
            var phone = $('#phone').val() || "";
            var title = $('#title').val();
            var address = $('#addres').val();

            var token = $('#AddressForm input[name="__RequestVerificationToken"]').val();


            var telIsValid = validatePhoneNumber(phone);

            if (!telIsValid) {
                toastr.error("Geçerli bir telefon numarası girin.", "Hata");
                return; // Eğer numara geçersizse form gönderimini durdur
            }



            formData.append('Name', name);
            formData.append('Surname', surname);
            formData.append('Phone', phone);
            formData.append('Title', title);
            formData.append('Address', address);
            formData.append('il', il);
            formData.append('ilce', ilce);
            formData.append('mahalle', mahalle);
            formData.append("__RequestVerificationToken", token);

            $.ajax({
                url: '/Home/AddAdress/',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                beforeSend: function () {
                    $('#loading').show();
                },
                success: function (result) {
                    if (result.success) {
                        toastr.success(result.message);




                        var addresscards = $("#address-cards");
                        addresscards.empty();
                        var i = 1;
                        result.adressList.forEach(function (address) {
                            var card = " <div class='col-12'><div class='card mb-2'><div class='card-body'><h5 class='card-title'>" + address.title + "</h5> <h6 class='card-subtitle mb-2 text-body-secondary'>" + address.name + " " + address.surname + "</h6><p class='card-text'>" + address.address + "</p>  <a href='#Adres' class='card-link editbtn' data-bs-toggle='offcanvas' data-bs-target='#editAddress' aria-controls='editAddress' aId='" + address.id + "' >Düzenle</a> <a onclick='RemoveAddress(" + address.id + ")' class='card-link'>Sil</a>   <a onclick='SelectAddress(" + address.id + ")' class='card-link select-btn'>Seç</a> </div></div></div>";

                            addresscards.append(card);
                            i++;
                        });




                    } else {
                        toastr.error(result.message);
                    }
                },
                complete: function () {
                    $('#loading').hide();
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Error: ' + error);
                }
            });
        }
    });

});




$(document).on("click", ".editbtn", function (event) {

    var clickedElement = $(event.target);
    var Id = clickedElement.attr('aId');

    $.ajax({
        url: '/Home/GetAddress/',
        type: 'POST',
        data: { Id: Id },
        success: async function (result) {
            if (result.success) {
                const address = result.adressList[0]; // Tek adres varsa listeyi açar

                $('#editname').val(address.name);
                $('#editsurname').val(address.surname);
                $('#editphone').val(address.phone_number);
                $('#edittitle').val(address.title);
                $('#editaddres').val(address.address);
                $('#editsubmitbtn').attr("aId", address.id);

                // Şehir seçimini yap
                $('#ilEdit option').each(function () {
                    if ($(this).val() === address.city) {
                        $(this).prop('selected', true);
                    }
                });
                $('#ilEdit').trigger('change');

                // İlçeleri yükle ve seçim yap
                await loadIlceler("Edit");
                $('#ilceEdit option').each(function () {
                    if ($(this).val() === address.district) {
                        $(this).prop('selected', true);
                    }
                });
                $('#ilceEdit').trigger('change');

                // Mahalleleri yükle ve seçim yap
                await loadMahalleler("Edit");
                $('#mahalleEdit option').each(function () {
                    if ($(this).val() === address.quarter) {
                        $(this).prop('selected', true);
                    }
                });
            } else {
                toastr.error(result.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error: ' + error);
        }
    });

});



function RemoveAddress(Id) {
    
    var token = $('#RemoveAddressForm input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Home/RemoveAddress/',
        type: 'POST',
        data: { __RequestVerificationToken: token, Id: Id },
        beforeSend: function () {
            $('#loading').show();
        },
        success: function (result) {
            if (result.success) {
                toastr.success(result.message);




                var addresscards = $("#address-cards");
                addresscards.empty();
                var i = 1;
                result.adressList.forEach(function (address) {
                    var card = " <div class='col-12'><div class='card mb-2'><div class='card-body'><h5 class='card-title'>" + address.title + "</h5> <h6 class='card-subtitle mb-2 text-body-secondary'>" + address.name + " " + address.surname + "</h6><p class='card-text'>" + address.address + "</p>  <a href='#Adres' class='card-link editbtn' data-bs-toggle='offcanvas' data-bs-target='#editAddress' aria-controls='editAddress' aId='" + address.id + "' >Düzenle</a> <a onclick='RemoveAddress(" + address.id + ")' class='card-link'>Sil</a>   <a onclick='SelectAddress(" + address.id + ")' class='card-link select-btn'>Seç</a> </div></div></div>";

                    addresscards.append(card);
                    i++;
                });




            } else {
                toastr.error(result.message);
            }
        }, complete: function () {
            $('#loading').hide();
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error: ' + error);
        }
    });
}


$("#editsubmitbtn").click(function (e) {

    e.preventDefault();

    if ($('#editAddressForm').valid()) {
        var formData = new FormData();

        var il = $('#ilEdit').val();
        var ilce = $('#ilceEdit').val();
        var mahalle = $('#mahalleEdit').val();

        if (!il || !ilce || !mahalle) {
            toastr.error("İl, İlce ve Mahalle Seçilmelidir.")
            return;
        }

        var id = $(e.target).attr('aId');
        var name = $('#editname').val();
        var surname = $('#editsurname').val();
        var phone = $('#editphone').val() || "";
        var title = $('#edittitle').val();
        var address = $('#editaddres').val();

        var token = $('#editAddressForm input[name="__RequestVerificationToken"]').val();


        var telIsValid = validatePhoneNumber(phone);

        if (!telIsValid) {
            toastr.error("Geçerli bir telefon numarası girin.", "Hata");
            return; // Eğer numara geçersizse form gönderimini durdur
        }



        formData.append('Id', id);
        formData.append('Name', name);
        formData.append('Surname', surname);
        formData.append('Phone', phone);
        formData.append('Title', title);
        formData.append('Address', address);
        formData.append('il', il);
        formData.append('ilce', ilce);
        formData.append('mahalle', mahalle);
        formData.append("__RequestVerificationToken", token);

        $.ajax({
            url: '/Home/EditAdress/',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            beforeSend: function () {
                $('#loading').show();
            },
            success: function (result) {
                if (result.success) {
                    toastr.success(result.message);




                    var addresscards = $("#address-cards");
                    addresscards.empty();
                    var i = 1;
                    result.adressList.forEach(function (address) {
                        var card = " <div class='col-12'><div class='card mb-2'><div class='card-body'><h5 class='card-title'>" + address.title + "</h5> <h6 class='card-subtitle mb-2 text-body-secondary'>" + address.name + " " + address.surname + "</h6><p class='card-text'>" + address.address + "</p>   <a href='#Adres' class='card-link editbtn' data-bs-toggle='offcanvas' data-bs-target='#editAddress' aria-controls='editAddress' aId='" + address.id + "' >Düzenle</a> <a onclick='RemoveAddress(" + address.id + ")' class='card-link'>Sil</a>   <a onclick='SelectAddress(" + address.id + ")' class='card-link select-btn'>Seç</a> </div></div></div>";

                        addresscards.append(card);


                        i++;
                    });

                    var currentOffcanvas = $('#editAddress');
                    currentOffcanvas.offcanvas('hide');
                    var newOffcanvas = $('#staticBackdrop');
                    newOffcanvas.offcanvas('show');

                    var cardadresid = $('.address-card').attr('addressid');


                    var a = result.adressList.find(function (address) {
                        if (address.id == cardadresid) {
                            return address;
                        } else {
                            return null;

                        }
                    });

                    if (a != null) {

                        var addresscards = $("#cartAddress");
                        addresscards.empty();
                        var i = 1;



                        var card = " <div class='col-12'><div class='card mb-2 text-start  address-card' addressId='" + a.id + "'><div class='card-body'><h5 class='card-title'>" + a.title + "</h5> <h6 class='card-subtitle mb-2 text-body-secondary'>" + a.name + " " + a.surname + "</h6><p class='card-text'>" + a.address + "</p> </div></div></div>   <a href='#Adres' data-bs-toggle='offcanvas' data-bs-target='#staticBackdrop' aria-controls='staticBackdrop'> Adres Değiştir / Ekle</a >";

                        addresscards.append(card);




                    }

                } else {
                    toastr.error(result.message);
                }
            }, complete: function () {
                $('#loading').hide();
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error: ' + error);
            }
        });
    }

});