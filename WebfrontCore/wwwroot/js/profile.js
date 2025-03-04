﻿$(document).ready(function () {
	/*
	Expand alias tab if they have any
	*/
    $('#profile_aliases_btn').click(function (e) {
        const aliases = $('#profile_aliases').text().trim();
        if (aliases && aliases.length !== 0) {
            $('#profile_aliases').slideToggle(150);
            $(this).toggleClass('oi-caret-top');
        }
    });

    /*
     * load context of chat 
     */
    $(document).off('click', '.client-message');
    $(document).on('click', '.client-message', function (e) {
        showLoader();
        const location = $(this);
        $.get('/Stats/GetMessageAsync', {
            'serverId': $(this).data('serverid'),
            'when': $(this).data('when')
        })
            .done(function (response) {
                $('.client-message-context').remove();
                location.after(response);
                hideLoader();
            })
            .fail(function (jqxhr, textStatus, error) {
                errorLoader();
            });
    });

    /*
 * load info on ban/flag
 */
    $(document).off('click', '.automated-penalty-info-detailed');
    $(document).on('click', '.automated-penalty-info-detailed', function (e) {
        showLoader();
        const location = $(this).parent();
        $.get('/Stats/GetAutomatedPenaltyInfoAsync', {
            'clientId': $(this).data('clientid'),
        })
            .done(function (response) {
                $('.penalty-info-context').remove();
                location.after(response);
                hideLoader();
            })
            .fail(function (jqxhr, textStatus, error) {
                errorLoader();
            });
    });

    /*
     get ip geolocation info into modal
     */
    $('.ip-locate-link').click(function (e) {
        e.preventDefault();
        const ip = $(this).data("ip");
        $.getJSON('https://extreme-ip-lookup.com/json/' + ip)
            .done(function (response) {
                $('#mainModal .modal-title').text(ip);
                $('#mainModal .modal-body').text("");
                if (response.ipName.length > 0) {
                    $('#mainModal .modal-body').append("Hostname &mdash; " + response.ipName + '<br/>');
                }
                if (response.isp.length > 0) {
                    $('#mainModal .modal-body').append("ISP &mdash; " + response.isp + '<br/>');
                }
                if (response.org.length > 0) {
                    $('#mainModal .modal-body').append("Organization &mdash; " + response.org + '<br/>');
                }
                if (response['businessName'].length > 0) {
                    $('#mainModal .modal-body').append("Business &mdash; " + response.businessName + '<br/>');
                }
                if (response['businessWebsite'].length > 0) {
                    $('#mainModal .modal-body').append("Website &mdash; " + response.businessWebsite + '<br/>');
                }
                if (response.city.length > 0 || response.region.length > 0 || response.country.length > 0) {
                    $('#mainModal .modal-body').append("Location &mdash; ");
                }
                if (response.city.length > 0) {
                    $('#mainModal .modal-body').append(response.city);
                }
                if (response.region.length > 0) {
                    $('#mainModal .modal-body').append(', ' + response.region);
                }
                if (response.country.length > 0) {
                    $('#mainModal .modal-body').append(', ' + response.country);
                }

                $('#mainModal').modal();
            })
            .fail(function (jqxhr, textStatus, error) {
                $('#mainModal .modal-title').text("Error");
                $('#mainModal .modal-body').html('<span class="text-danger">&mdash;' + error + '</span>');
                $('#mainModal').modal();
            });
    });
});
