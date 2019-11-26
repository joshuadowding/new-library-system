// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $("#clear-filters").click(function () {
        $("#Author").val("");
        $("#Publisher").val("");
        $("#Series").val("");
        $("#Type").val("");
        $("#Genre").val("");
        $("#Form").val("");
        $("#Age").val("");
        $("#Location").val("");
    });

    $("#warning-toast .close").click(function () {
        $("#warning-toast").hide();
    });
});
