// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/// https://stackoverflow.com/questions/25606730/get-current-locale-of-chrome#answer-42070353
var language;
if (window.navigator.languages) {
    language = window.navigator.languages[0];
}
else {
    language = window.navigator.userLanguage || window.navigator.language;
}
