// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

$(function () {
    var reportContainer = $("#report-container").get(0);

    // Retrieve TrackingCode from the URL
    var queryParams = new URLSearchParams(window.location.search); // Parse the URL
    var TrackingCode = queryParams.get('TrackingCode');

    // Initialize iframe for embedding report
    powerbi.bootstrap(reportContainer, { type: "report" });

    var models = window["powerbi-client"].models;
    var reportLoadConfig = {
        type: "report",
        tokenType: models.TokenType.Embed,

        // Enable this setting to remove gray shoulders from embedded report
        // settings: {
        //     background: models.BackgroundType.Transparent
        // }
    };

    $.ajax({
        type: "GET",
        url: "/getembedinfo",
        dataType: "json",
        success: function (data) {
            embedData = $.parseJSON(JSON.stringify(data));
            reportLoadConfig.accessToken = embedData.accessToken;

            // You can embed different reports as per your need
            reportLoadConfig.embedUrl = embedData.reportConfig[0].embedUrl;

            // Add filter here
            reportLoadConfig.filters = [
                {
                    $schema: "http://powerbi.com/product/schema#basic",
                    target: {
                        table: "Shipment Tracking",
                        column: "GUID"
                    },
                    operator: "In",
                    values:  [TrackingCode] //["b2ad91b0-c868-4479-84ba-51692ff5d9a8"] //
                }
            ];
            console.log(TrackingCode)

            // Use the token expiry to regenerate Embed token for seamless end user experience
            // Refer https://aka.ms/RefreshEmbedToken
            tokenExpiry = embedData.tokenExpiry;

            // Embed Power BI report when Access token and Embed URL are available
            var report = powerbi.embed(reportContainer, reportLoadConfig);

            // Triggers when a report schema is successfully loaded
            report.on("loaded", function () {
                console.log("Report load successful")
                const newSettings = {
                    panes: {
                      filters :{
                        visible: false // hide filter pane
                      },
                    pageNavigation: {
                        visible: false
                    }
                    }
                  };
                  report.updateSettings(newSettings)
                    .catch(error => { console.log(error) });
            });

            // Triggers when a report is successfully embedded in UI
            report.on("rendered", function () {
                console.log("Report render successful")
            });

            // Clear any other error handler event
            report.off("error");

            // Below patch of code is for handling errors that occur during embedding
            report.on("error", function (event) {
                var errorMsg = event.detail;

                // Use errorMsg variable to log error in any destination of choice
                console.error(errorMsg);
                return;
            });

        },
        error: function (err) {

            // Show error container
            var errorContainer = $(".error-container");
            $(".embed-container").hide();
            errorContainer.show();

            // Format error message
            var errMessageHtml = "<strong> Error Details: </strong> <br/>" + $.parseJSON(err.responseText)["errorMsg"];
            errMessageHtml = errMessageHtml.split("\n").join("<br/>")

            // Show error message on UI
            errorContainer.html(errMessageHtml);
        }
    });
});