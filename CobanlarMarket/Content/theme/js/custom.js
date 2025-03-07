/**
 * WEBSITE: https://themefisher.com
 * TWITTER: https://twitter.com/themefisher
 * FACEBOOK: https://www.facebook.com/themefisher
 * GITHUB: https://github.com/themefisher/
 */

/* ====== Index ======

1. SCROLLBAR CONTENT
2. TOOLTIPS AND POPOVER
3. JVECTORMAP HOME WORLD
4. JVECTORMAP USA REGIONS VECTOR MAP
5. COUNTRY SALES RANGS
6. JVECTORMAP HOME WORLD
7. CODE EDITOR
8. QUILL TEXT EDITOR
9. MULTIPLE SELECT
10. LOADING BUTTON
11. TOASTER
12. INFO BAR
13. PROGRESS BAR
14. DATA TABLE
15. OWL CAROUSEL

====== End ======*/

$(document).ready(function () {
    "use strict";

    /*======== 1. SCROLLBAR CONTENT ========*/

    /*======== 2. TOOLTIPS AND POPOVER ========*/
    $('[data-toggle="tooltip"]').tooltip({
        container: "body",
        template:
            '<div class="tooltip" role="tooltip"><div class="arrow"></div><div class="tooltip-inner"></div></div>',
    });
    $('[data-toggle="popover"]').popover();

    /*======== 3. JVECTORMAP HOME WORLD ========*/
    var homeWorld = $("#home-world");
    if (homeWorld.length != 0) {
        var colorData = {
            CA: 106,
            US: 166,
            RU: 166,
            AR: 166,
            AU: 120,
            IN: 106,
        };
        homeWorld.vectorMap({
            map: "world_mill",
            backgroundColor: "#fff",
            zoomOnScroll: false,
            regionStyle: {
                initial: {
                    fill: "#cbccd4",
                },
            },
            series: {
                regions: [
                    {
                        values: colorData,
                        scale: ["#9e6cdf", "#dfe0e4", "#f9aec9"],
                    },
                ],
            },
        });
    }

    /*======== 4. JVECTORMAP USA REGIONS VECTOR MAP ========*/
    var usVectorMap = $("#us-vector-map-marker");
    if (usVectorMap.length != 0) {
        usVectorMap.vectorMap({
            map: "us_aea",
            backgroundColor: "#transparent",
            zoomOnScroll: false,
            regionStyle: {
                initial: {
                    fill: "#eff0f5",
                },
            },
            markerStyle: {
                hover: {
                    stroke: "transparent",
                },
            },
            markers: [
                {
                    latLng: [39.55, -105.78],
                    name: "Colorado",
                    style: { fill: "#46c79e", stroke: "#46c79e" },
                },
                {
                    latLng: [40.26, -86.13],
                    name: "Indiana",
                    style: { fill: "#fec402", stroke: "#fec402" },
                },
                {
                    latLng: [43.8, -120.55],
                    name: "Oregon",
                    style: { fill: "#9e6de0", stroke: "#9e6de0" },
                },
            ],
        });
    }

    /*======== 5. COUNTRY SALES RANGS ========*/
    var countrySalesRange = $("#country-sales-range");
    if (countrySalesRange.length != 0) {
        var start = moment().subtract(29, "days");
        var end = moment();

        function cb(start, end) {
            $("#country-sales-range .date-holder").html(
                start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY")
            );
        }

        countrySalesRange.daterangepicker(
            {
                startDate: start,
                endDate: end,
                opens: "left",
                ranges: {
                    Today: [moment(), moment()],
                    Yesterday: [
                        moment().subtract(1, "days"),
                        moment().subtract(1, "days"),
                    ],
                    "Last 7 Days": [moment().subtract(6, "days"), moment()],
                    "Last 30 Days": [moment().subtract(29, "days"), moment()],
                    "This Month": [moment().startOf("month"), moment().endOf("month")],
                    "Last Month": [
                        moment().subtract(1, "month").startOf("month"),
                        moment().subtract(1, "month").endOf("month"),
                    ],
                },
            },
            cb
        );

        cb(start, end);
    }
    var miniStatusRanges = $("#mini-status-range");

    if (miniStatusRanges.length != 0) {
        var start = moment().subtract(29, "days");
        var end = moment();

        function cb(start, end) {
            $("#mini-status-range .date-holder").html(
                start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY")
            );
        }

        miniStatusRanges.daterangepicker(
            {
                startDate: start,
                endDate: end,
                opens: "left",
                ranges: {
                    Today: [moment(), moment()],
                    Yesterday: [
                        moment().subtract(1, "days"),
                        moment().subtract(1, "days"),
                    ],
                    "Last 7 Days": [moment().subtract(6, "days"), moment()],
                    "Last 30 Days": [moment().subtract(29, "days"), moment()],
                    "This Month": [moment().startOf("month"), moment().endOf("month")],
                    "Last Month": [
                        moment().subtract(1, "month").startOf("month"),
                        moment().subtract(1, "month").endOf("month"),
                    ],
                },
            },
            cb
        );

        cb(start, end);
    }

    /*======== 6. JVECTORMAP HOME WORLD ========*/
    var countryWithMarker = $("#world-country-with-marker");
    if (countryWithMarker.length != 0) {
        var colorData = {
            CA: 106,
            US: 166,
            RU: 166,
            AR: 166,
            AU: 120,
            IN: 106,
        };
        countryWithMarker.vectorMap({
            map: "world_mill",
            backgroundColor: "#fff",
            zoomOnScroll: false,
            regionStyle: {
                initial: {
                    fill: "#cbccd4",
                },
            },
            series: {
                regions: [
                    {
                        values: colorData,
                        scale: ["#9e6cdf", "#dfe0e4", "#f9aec9"],
                    },
                ],
            },
            markers: [
                { latLng: [56.13, -106.34], name: "Vatican City" },
                { latLng: [37.09, -95.71], name: "Washington" },
                { latLng: [-14.23, -51.92], name: "Brazil" },
                { latLng: [17.6078, 8.0817], name: "Tuvalu" },
                { latLng: [47.14, 9.52], name: "Liechtenstein" },
                { latLng: [20.59, 78.96], name: "India" },
                { latLng: [61.52, 105.31], name: "Russia" },
            ],
        });
    }

    var usVectorMapWithoutMarker = $("#us-vector-map-without-marker");
    if (usVectorMapWithoutMarker.length != 0) {
        usVectorMapWithoutMarker.vectorMap({
            map: "us_aea",
            backgroundColor: "#transparent",
            zoomOnScroll: false,
            regionStyle: {
                initial: {
                    fill: "#eff0f5",
                },
            },
            markerStyle: {
                hover: {
                    stroke: "transparent",
                },
            },
        });
    }

    /*======== 7. CODE EDITOR ========*/
    var codeEditor = document.getElementById("code-editor");
    if (codeEditor) {
        var htmlCode = `<html style="color: green">
  <!-- this is a comment -->
  <head>"
    <title>HTML Example</title>
  </head>
  <body>
    The indentation tries to be <em>somewhat &quot;do what
    I mean&quot;</em>... but might not match your style.
  </body>
</html>`;

        var myCodeMirror = CodeMirror(codeEditor, {
            value: htmlCode,
            mode: "xml",
            extraKeys: { "Ctrl-Space": "autocomplete" },
            lineNumbers: true,
            indentWithTabs: true,
            lineWrapping: true,
        });
    }

    /*======== 8. QUILL TEXT EDITOR ========*/


    /*======== 9. MULTIPLE SELECT ========*/
    var select2Multiple = $(".js-example-basic-multiple");
    if (select2Multiple.length != 0) {
        select2Multiple.select2();
    }
    var select2Country = $(".country");
    if (select2Country.length != 0) {
        select2Country.select2({
            minimumResultsForSearch: -1,
        });
    }

    /*======== 10. LOADING BUTTON ========*/
    var laddaButton = $(".ladda-button");
    if (laddaButton.length != 0) {
        Ladda.bind(".ladda-button", {
            timeout: 1000,
        });
    }

    /*======== 11. TOASTER ========*/
    //var toaster = $("#toaster");
    //function callToaster(positionClass) {
    //  toastr.options = {
    //    closeButton: true,
    //    debug: false,
    //    newestOnTop: false,
    //    progressBar: true,
    //    positionClass: positionClass,
    //    preventDuplicates: false,
    //    onclick: null,
    //    showDuration: "300",
    //    hideDuration: "1000",
    //    timeOut: "5000",
    //    extendedTimeOut: "1000",
    //    showEasing: "swing",
    //    hideEasing: "linear",
    //    showMethod: "fadeIn",
    //    hideMethod: "fadeOut",
    //  };
    //  toastr.success("Welcome to Mono Dashboard", "Howdy!");
    //}

    //if (toaster.length != 0) {
    //  if (document.dir != "rtl") {
    //    callToaster("toast-top-right");
    //  } else {
    //    callToaster("toast-top-left");
    //  }
    //}

    /*======== 12. INFO BAR ========*/
    var infoTeoaset = $(
        "#toaster-info, #toaster-success, #toaster-warning, #toaster-danger"
    );
    if (infoTeoaset !== null) {
        infoTeoaset.on("click", function () {
            toastr.options = {
                closeButton: true,
                debug: false,
                newestOnTop: false,
                progressBar: false,
                positionClass: "toast-top-right",
                preventDuplicates: false,
                onclick: null,
                showDuration: "3000",
                hideDuration: "1000",
                timeOut: "5000",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut",
            };
            var thisId = $(this).attr("id");
            if (thisId === "toaster-info") {
                toastr.info("Welcome to Mono", " Info message");
            } else if (thisId === "toaster-success") {
                toastr.success("Welcome to Mono", "Success message");
            } else if (thisId === "toaster-warning") {
                toastr.warning("Welcome to Mono", "Warning message");
            } else if (thisId === "toaster-danger") {
                toastr.error("Welcome to Mono", "Danger message");
            }
        });
    }

    /*======== 13. PROGRESS BAR ========*/

    /*======== 14. DATA TABLE ========*/
    var productsTable = $("#productsTable");
    if (productsTable.length != 0) {
        productsTable.DataTable({

            info: false,
            lengthChange: true,
            responsive: true,


        
            scrollX: true,
            order: [[2, "asc"]],
            columnDefs: [
                {
                    orderable: false,
                    targets: [, 0, 6, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
                lengthMenu: "_MENU_ ürün göster",
            },
        });
    }
    var attributeTable = $("#attributesTable");
    if (attributeTable.length != 0) {
        attributeTable.DataTable({
            info: false,
            lengthChange: false,
            responsive: true,


            scrollX: true,
            order: [[1, "asc"]],
            columnDefs: [
                {
                    orderable: false,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }
    var categoryTable = $("#categoryTable");
    if (categoryTable.length != 0) {
        categoryTable.DataTable({
            info: false,
            lengthChange: false,
            responsive: true,


            scrollX: true,
            order: [[0, "asc"]],
            columnDefs: [
                {
                    orderable: true,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }
    var subCategoryTable = $("#subCategoryTable");
    if (subCategoryTable.length != 0) {
        subCategoryTable.DataTable({
            info: false,
            lengthChange: false,
            responsive: true,
            scrollX: true,
            order: [[0, "asc"]],
            columnDefs: [
                {
                    orderable: true,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }
    var subCategoryTable = $("#subsubCategoryTable");
    if (subCategoryTable.length != 0) {
        subCategoryTable.DataTable({
            info: false,
            lengthChange: false,
            responsive: true,
            scrollX: true,
            order: [[0, "asc"]],
            columnDefs: [
                {
                    orderable: true,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }
    var userTable = $("#userTable");
    if (userTable.length != 0) {
        userTable.DataTable({
            info: false,
            lengthChange: false,
            responsive: true,

            scrollX: true,
            order: [[1, "asc"]],
            columnDefs: [
                {
                    orderable: false,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }




    var couponTable = $("#couponTable");
    if (couponTable.length != 0) {
        couponTable.DataTable({
            order: [
                [2, 'desc']
            ],
            info: false,

            lengthChange: false,
            responsive: true,


            scrollX: true,

            columnDefs: [
                {
                    orderable: false,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }

    var campaignTable = $("#campaignTable");
    if (campaignTable.length != 0) {
        campaignTable.DataTable({
            order: [
                [2, 'desc']
            ],
            info: false,

            lengthChange: false,
            responsive: true,


            scrollX: true,
            columnDefs: [
                {
                    orderable: false,
                    targets: [, 0, 4, -1],
                },
            ],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }

    var productSale = $("#product-sale");
    if (productSale.length != 0) {
        productSale.DataTable({
            info: false,
            paging: true,
            searching: true,
            responsive: true,

            scrollX: false,
            order: [[2, "desc"]],
            columnDefs: [
                {
                    orderable: false,
                    targets: [-1],
                },
            ], language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            }

        });
    }




    var mailTable = $("#mailTable");
    if (mailTable.length != 0) {
        mailTable.DataTable({
            order: [
                [2, 'desc']
            ],
            info: false,

            lengthChange: false,
            responsive: true,


            scrollX: true,

         
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Ara...",
            },
        });
    }


    /*======== 15. OWL CAROUSEL ========*/
    var slideOnly = $(".slide-only");
    if (slideOnly.length != 0) {
        slideOnly.owlCarousel({
            items: 1,
            autoplay: true,
            loop: true,
            dots: false,
        });
    }

    var carouselWithControl = $(".carousel-with-control");
    if (carouselWithControl.length != 0) {
        carouselWithControl.owlCarousel({
            items: 1,
            autoplay: true,
            loop: true,
            dots: false,
            nav: true,
            navText: [
                '<i class="mdi mdi-chevron-left"></i>',
                '<i class="mdi mdi-chevron-right"></i>',
            ],
            center: true,
        });
    }

    var carouselWithIndicators = $(".carousel-with-indicators");
    if (carouselWithIndicators.length != 0) {
        carouselWithIndicators.owlCarousel({
            items: 1,
            autoplay: true,
            loop: true,
            nav: true,
            navText: [
                '<i class="mdi mdi-chevron-left"></i>',
                '<i class="mdi mdi-chevron-right"></i>',
            ],
            center: true,
        });
    }

    var caoruselWithCaptions = $(".carousel-with-captions");
    if (caoruselWithCaptions.length != 0) {
        caoruselWithCaptions.owlCarousel({
            items: 1,
            autoplay: true,
            loop: true,
            nav: true,
            navText: [
                '<i class="mdi mdi-chevron-left"></i>',
                '<i class="mdi mdi-chevron-right"></i>',
            ],
            center: true,
        });
    }

    var carouselUser = $(".carousel-user");
    if (carouselUser.length != 0) {
        carouselUser.owlCarousel({
            items: 4,
            margin: 80,
            autoplay: true,
            loop: true,
            nav: true,
            navText: [
                '<i class="mdi mdi-chevron-left"></i>',
                '<i class="mdi mdi-chevron-right"></i>',
            ],
            responsive: {
                0: {
                    items: 1,
                    margin: 0,
                },
                768: {
                    items: 2,
                },
                1000: {
                    items: 3,
                },
                1440: {
                    items: 4,
                },
            },
        });
    }

    var carouselTestimonial = $(".carousel-testimonial");
    if (carouselTestimonial.length != 0) {
        carouselTestimonial.owlCarousel({
            items: 3,
            margin: 135,
            autoplay: false,
            loop: true,
            nav: true,
            navText: [
                '<i class="mdi mdi-chevron-left"></i>',
                '<i class="mdi mdi-chevron-right"></i>',
            ],
            responsive: {
                0: {
                    items: 1,
                    margin: 0,
                },
                768: {
                    items: 1,
                },
                1000: {
                    items: 2,
                },
                1440: {
                    items: 3,
                },
            },
        });
    }

    /*======== 7. CIRCLE PROGRESS ========*/
    var circle = $(".circle");
    var gray = "#f5f6fa";

    if (circle.length != 0) {
        circle.circleProgress({
            lineCap: "round",
            startAngle: 4.8,
            emptyFill: [gray],
        });
    }


    //----------------------------------------------------------------------------------------------
    var orderTable = $("#orderTable").DataTable({

        order: [
            [2, 'desc']
        ],
        rowGroup: {
            dataSrc: 2,
            startRender: function (rows, group) {
                var payId = $("#orderTable").find("[orderId='" + group + "']").attr("payId");
                var groupHtml = $(`
                <div class="float-right d-flex align-items-center">
               <span id="icon-${group}" class="mdi mdi-package-variant-closed-remove"></span>
               <label class="switch switch-icon switch-primary switch-pill form-control-label mr-2">
                      <input type="checkbox" class="switch-input form-check-input deliver-switch" id="delivered-${group}" >
                      <span class="switch-label"></span>
                      <span class="switch-handle">
                      </span>
                </label>


                     
                    <a class="mr-2" href="/Management/OrderDetail/${group}"><span class="mdi mdi-eye text-bg-warning"></span></a>
                    <a class="btn btn-danger" href="javascript:void(0);" onclick="Refund('${payId}')">İade Et</a>
                </div>
            `);

                $.ajax({
                    url: '/Management/isDelivered/',
                    type: 'POST',
                    data: { Id: parseInt(group) },
                    success: function (response) {
                        var isDelivered = response.toString();
                        if (isDelivered === "True") {

                            $(`#delivered-${group}`).prop('checked', true);
                            $(`#delivered-${group}`).prop('value', "on");
                            $(`#icon-${group}`).removeClass();
                            $(`#icon-${group}`).addClass("mdi mdi-package-variant-closed-check")

                        }
                    },
                    error: function (e) {
                        console.log(e);
                    }
                });

                return groupHtml;
            }
        },
        responsive: true,
        info: false,
        lengthChange: false,
        scrollX: true,
        columnDefs: [
            {
                orderable: false,
                targets: [0, 4, -1],
            },
        ],
        language: {
            search: "_INPUT_",
            "emptyTable": "Sipariş Bulunmamaktadır",
            searchPlaceholder: "Ara..",
        }
    });

    $(document).on('change', '.deliver-switch', function (e) {

        var element = $(e.target);

        var id = element.attr("id").split("-")[1];
        var status = element.is(':checked') ? 'delivered' : 'undelivered';

        $.ajax({
            url: '/Management/ChangeDeliveryStatus/',
            type: 'POST',
            data: { Id: parseInt(id) },
            success: function (response) {

                if (response.success) {
                    toastr.success(response.message);
                    if (response.status == true) {
                        $('#orderTable [orderid="' + id + '"]').removeClass('notDelivered blink-effect');
                        $(`#icon-${id}`).removeClass();
                        $(`#icon-${id}`).addClass("mdi mdi-package-variant-closed-check")

                    } else {
                        $('#orderTable [orderid="' + id + '"]').addClass('notDelivered blink-effect');
                        $(`#icon-${id}`).removeClass();
                        $(`#icon-${id}`).addClass("mdi mdi-package-variant-closed-remove")
                    }

                }
                else {
                    toastr.error(response.message);
                    if (status == "delivered") {
                        element.removeAttr("checked");
                    } else {
                        element.attr("checked", "checked");
                    }
                }

            },
            error: function (e) {
                console.log(e);
            }
        });

    });


    var notificationHub = $.connection.notificationHub;

    notificationHub.client.receiveNotification = function (message, orderItems, paymentDetails, orderDetails, notifications) {
        toastr.success(message, notifications.text);

        if (paymentDetails != null) {

            paymentDetails.forEach(function (p) {

                toastr.success("₺" + p.paidPrice + "'lik bir sipariş aldınız.");

            });
        }

        $('#myTabContent #all').empty();
        var anyUnread = false;
        notifications.forEach(function (notification) {

            var element = '<div class="media media-sm ' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                '  <div class="media-sm-wrapper">' +
                '<a href="/Management/' +
                (notification.type == "campaign" ? "EditCampaign" :
                    (notification.type == "order" ? "OrderDetail" :
                        (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                (notification.type == "campaign" ? notification.campaign_id :
                    (notification.type == "order" ? notification.order_id :
                        (notification.type == "product" ? notification.product_id : ""))) + '">' +

                ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image" style="width:50px;height:50px;object-fit: cover;"></a>' +
                ' </div>' +
                ' <div class="media-body">' +
                '  <a href="/Management/' +
                (notification.type == "campaign" ? "EditCampaign" :
                    (notification.type == "order" ? "OrderDetail" :
                        (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                (notification.type == "campaign" ? notification.campaign_id :
                    (notification.type == "order" ? notification.order_id :
                        (notification.type == "product" ? notification.product_id : ""))) + '">' +
                ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                '<span class="discribe">' + notification.text + '</span>' +
                ' </a>' +
                '</div>' +
                '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' +
                (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +
                '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + '"></span>' +
                '</a>' +
                '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                '<span class="mdi mdi-trash-can-outline text-danger"></span>' +
                '</a>';


            $('#myTabContent #all').append(element);

            if (notification.is_read == false) {
                anyUnread = true;
            }
        });




        if (orderTable != null) {
            if (orderItems != null) {

                orderItems.forEach(function (order) {
                    var orderId = order.order_id;


                    var rowData = [
                        '<img src="' + (order.products ? order.products.cover : '') + '" alt="Alternate Text" />', // 1. sütun
                        order.id, // 2. sütun
                        order.order_id, // 3. sütun
                        order.orderDetails && order.orderDetails.User ? order.orderDetails.User.username : '', // 4. sütun
                        order.products ? order.products.name : '', // 5. sütun
                        order.quantity, // 6. sütun
                        order.products && order.products.price ? (order.quantity * order.products.price) : 0, // 7. sütun
                        order.orderDetails ? order.orderDetails.payment_details.status : '', // 8. sütun
                        order.orderDetails ? order.orderDetails.created_at : '', // 9. sütun
                        '' // 10. sütun
                    ];


                    var addedRow = orderTable.row.add(rowData).draw(false).node();

                    addedRow.classList.add("notDelivered", "blink-effect");
                });
            }

        }



        $('.notify-toggler .badge').text(notifications.length);
        $('#all-tabs').text("Bildirimler (" + notifications.length + ")");
        if (anyUnread) {
            startBlinking(); // Animasyonu başlat
        } else {
            stopBlinking(); // Animasyonu durdur
        }

    };

    $.connection.hub.start().done(function () {
        console.log("SignalR bağlantısı kuruldu.");
    }).fail(function (error) {
        console.error('SignalR bağlantı hatası: ' + error);
    });


});

$.ajax({
    url: '/Management/GetNotification/',
    type: 'POST',

    success: function (response) {


        if (response.success) {


            $('#myTabContent #all').empty();
            var anyUnread = false;

            response.list.forEach(function (notification) {

                var element = '<div class="media media-sm ' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                    '  <div class="media-sm-wrapper" >' +
                    '<a href="/Management/' +
                    (notification.type == "campaign" ? "EditCampaign" :
                        (notification.type == "order" ? "OrderDetail" :
                            (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                    (notification.type == "campaign" ? notification.campaign_id :
                        (notification.type == "order" ? notification.order_id :
                            (notification.type == "product" ? notification.product_id : ""))) + '">' +
                    ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                    ' </div >' +
                    ' <div class="media-body">' +
                    '  <a href="/Management/' +
                    (notification.type == "campaign" ? "EditCampaign" :
                        (notification.type == "order" ? "OrderDetail" :
                            (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                    (notification.type == "campaign" ? notification.campaign_id :
                        (notification.type == "order" ? notification.order_id :
                            (notification.type == "product" ? notification.product_id : ""))) + '">' +
                    ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                    '<span class="discribe">' + notification.text + '</span>' +
                    ' </a>  ' +
                    '</div > ' +
                    '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                    '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                    '</a> ' +
                    '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                    '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                    '</a> ';

                $('#myTabContent #all').append(element);

                if (notification.is_read == false) {
                    anyUnread = true;
                }
            });

            $('.notify-toggler .badge').text(response.list.length);
            $('#all-tabs').text("Bildirimler (" + response.list.length + ")");

            if (anyUnread) {
                startBlinking(); // Animasyonu başlat
            } else {
                stopBlinking(); // Animasyonu durdur
            }

        }



    }, error: function (e) {
        console.log(e)

    }



});
function blink() {
    $('.notify-toggler .badge').fadeOut(1000).fadeIn(1000);
}

$('#refress-button').on("click", function () {


    $.ajax({
        url: "/Management/GetNotification",
        type: "POST",
        success: function (response) {
            if (response.success) {

                $('#myTabContent #all').empty();
                var anyUnread = false;

                response.list.forEach(function (notification) {

                    var element = '<div class="media media-sm ' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                        '  <div class="media-sm-wrapper" >' +
                        '<a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                        ' </div >' +
                        ' <div class="media-body">' +
                        '  <a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                        '<span class="discribe">' + notification.text + '</span>' +
                        ' </a>  ' +
                        '</div > ' +
                        '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                        '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                        '</a> ' +
                        '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                        '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                        '</a> ';

                    $('#myTabContent #all').append(element);

                    if (notification.is_read == false) {
                        anyUnread = true;
                    }
                });

                $('.notify-toggler .badge').text(response.list.length);
                $('#all-tabs').text("Bildirimler (" + response.list.length + ")");


                if (anyUnread) {
                    startBlinking(); // Animasyonu başlat
                } else {
                    stopBlinking(); // Animasyonu durdur
                }
            }

        }



    });

});
function startBlinking() {
    $('.notify-toggler .badge').addClass('blink-effect'); // Blink efektini başlat
}

function stopBlinking() {
    $('.notify-toggler .badge').removeClass('blink-effect'); // Blink efektini durdur
}




function RemoveNotification(id) {
    $.ajax({
        url: "/Management/RemoveNotification",
        type: "POST",
        data: { id: parseInt(id) },
        success: function (response) {
            if (response.success) {
                toastr.success("Bildirim silindi");

                $('#myTabContent #all').empty();
                var anyUnread = false;

                response.list.forEach(function (notification) {

                    var element = '<div class="media media-sm ' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                        '  <div class="media-sm-wrapper" >' +
                        '<a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                        ' </div >' +
                        ' <div class="media-body">' +
                        '  <a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                        '<span class="discribe">' + notification.text + '</span>' +
                        ' </a>  ' +
                        '</div > ' +
                        '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                        '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                        '</a> ' +

                        '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                        '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                        '</a> ';

                    $('#myTabContent #all').append(element);

                    if (notification.is_read == false) {
                        anyUnread = true;
                    }
                });

                $('.notify-toggler .badge').text(response.list.length);
                $('#all-tabs').text("Bildirimler (" + response.list.length + ")");


                if (anyUnread) {
                    startBlinking(); // Animasyonu başlat
                } else {
                    stopBlinking(); // Animasyonu durdur
                }


            } else {
                toastr.error("Bir Hata Oluştu");
            }

        }



    });
}

function RemoveAllNotifiactions() {
    $.ajax({
        url: "/Management/RemoveAllNotification",
        type: "POST",
        success: function (response) {
            if (response.success) {
                toastr.success("Bildirimler silindi");

                $('#myTabContent #all').empty();
                var anyUnread = false;

                response.list.forEach(function (notification) {

                    var element = '<div class="media media-sm' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                        '  <div class="media-sm-wrapper" >' +
                        '<a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                        ' </div >' +
                        ' <div class="media-body">' +
                        '  <a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                        '<span class="discribe">' + notification.text + '</span>' +
                        ' </a>  ' +
                        '</div > ' +
                        '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                        '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                        '</a> ' +
                        '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                        '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                        '</a> ';

                    $('#myTabContent #all').append(element);
                    if (notification.is_read == false) {
                        anyUnread = true;
                    }
                });

                $('.notify-toggler .badge').text(response.list.length);
                $('#all-tabs').text("Bildirimler (" + response.list.length + ")");


                if (anyUnread) {
                    startBlinking(); // Animasyonu başlat
                } else {
                    stopBlinking(); // Animasyonu durdur
                }

            } else {
                toastr.error("Bir Hata Oluştu");
            }

        }



    });
}
function ReadNotification(id) {
    $.ajax({
        url: "/Management/ReadNotification",
        type: "POST",
        data: { id: parseInt(id) },
        success: function (response) {
            if (response.success) {

                $('#myTabContent #all').empty();
                var anyUnread = false;

                response.list.forEach(function (notification) {

                    var element = '<div class="media media-sm ' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                        '  <div class="media-sm-wrapper" >' +
                        '<a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                        ' </div >' +
                        ' <div class="media-body">' +
                        '  <a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                        '<span class="discribe">' + notification.text + '</span>' +
                        ' </a>  ' +
                        '</div > ' +
                        '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                        '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                        '</a> ' +
                        '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                        '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                        '</a> ';

                    $('#myTabContent #all').append(element);

                    if (notification.is_read == false) {
                        anyUnread = true;
                    }
                });

                $('.notify-toggler .badge').text(response.list.length);
                $('#all-tabs').text("Bildirimler (" + response.list.length + ")");

                if (anyUnread) {
                    startBlinking(); // Animasyonu başlat
                } else {
                    stopBlinking(); // Animasyonu durdur
                }


            } else {
                toastr.error("Bir Hata Oluştu");
            }

        }



    });
}



function ReadAllNotifiactions() {
    $.ajax({
        url: "/Management/ReadAllNotification",
        type: "POST",
        success: function (response) {
            if (response.success) {

                $('#myTabContent #all').empty();
                var anyUnread = false;

                response.list.forEach(function (notification) {

                    var element = '<div class="media media-sm' + (notification.is_read == false ? "bg-warning-10" : "") + ' border p-4 mb-0">' +
                        '  <div class="media-sm-wrapper" >' +
                        '<a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <img src="' + (notification.User.avatar == null ? "/content/theme/images/User_Icon.png" : notification.User.avatar) + '" alt="User Image"style="width:50px;height:50px;object-fit: cover;"></a>' +


                        ' </div >' +
                        ' <div class="media-body">' +
                        '  <a href="/Management/' +
                        (notification.type == "campaign" ? "EditCampaign" :
                            (notification.type == "order" ? "OrderDetail" :
                                (notification.type == "product" ? "EditProduct" : ""))) + '/' +
                        (notification.type == "campaign" ? notification.campaign_id :
                            (notification.type == "order" ? notification.order_id :
                                (notification.type == "product" ? notification.product_id : ""))) + '">' +
                        ' <span class="title mb-0">' + notification.User.first_name + '</span>' +
                        '<span class="discribe">' + notification.text + '</span>' +
                        ' </a>  ' +
                        '</div > ' +
                        '<a class="readNotification" onclick="ReadNotification(' + notification.id + ')" data-toggle="tooltip" title="' + (notification.is_read == false ? "Okundu Olarak İşaretle" : "Okundu") + '">' +

                        '<span class="mdi ' + (notification.is_read == false ? "mdi-eye-remove-outline" : "mdi-eye-check-outline") + ' " ></span >' +

                        '</a> ' +
                        '<a class="removeNotification" onclick="RemoveNotification(' + notification.id + ')">' +
                        '<span class="mdi mdi-trash-can-outline text-danger" ></span >' +

                        '</a> ';

                    $('#myTabContent #all').append(element);
                    if (notification.is_read == false) {
                        anyUnread = true;
                    }
                });

                $('.notify-toggler .badge').text(response.list.length);
                $('#all-tabs').text("Bildirimler (" + response.list.length + ")");


                if (anyUnread) {
                    startBlinking(); // Animasyonu başlat
                } else {
                    stopBlinking(); // Animasyonu durdur
                }

            } else {
                toastr.error("Bir Hata Oluştu");
            }

        }



    });
}




function Refund(payId) {

    $.ajax({
        url: '/Management/Refund/',
        type: 'POST',
        data: { PaymentId: payId },
        beforeSend: function () {
            NProgress.start();

        },
        success: function (result) {
            if (result.success) {
                toastr.success(result.message, 'İşlem Başarılı');

                var tr = $("#orderTable tbody [orderId='" + result.list[0].order_id + "']");

                tr.find(".paymentstatus").text(result.list[0].paymentstatus);






            } else {
                toastr.error(result.message, 'İşlem Başarısız');

            }






        }, complete: function () {
            NProgress.done();

        }


    });




}
