$(document).ready(function () {
    var token = localStorage.getItem("token");
    var username = localStorage.getItem("username");
    $("#api_info").html('');
    if (token) {
        $("#api_info").append('<h2>Logged in as ' + username + '</h2>');
        $("#api_info").append('<button id="logout" style="float:right; margin-top:-30px;">Logout</button>');
        $("input[name='Authorization']").val("Bearer " + token);
    } else {
        $("#api_info").append('<input placeholder="Username" id="username" type="text">');
        $("#api_info").append('<input placeholder="password" id="password" type="password">');
        $("#api_info").append('<input placeholder="ApiKey" id="api_key" type="text" hidden>');
        $("#api_info").append('<button id="loginButton">Login</button>');
    }
    $("#input_baseUrl").hide();
    $("#input_apiKey").hide();
    $("#explore").hide();
});
$("#api_info").on('click', '#logout', function () {
    localStorage.removeItem("username");
    localStorage.removeItem("token");
    window.location.reload();
});
$("#api_info").on('click', '#loginButton', function () {
    var username = $("#username").val();
    var password = $("#password").val();
    var url = "";
    var path = window.location.href;
    if (path.indexOf('dev') !== -1) {
        url = "http://dev.inovas.net/campussafety2019/dev";
    }
    $.post(url + "/api/users/login", { 'UserId': username, 'Password': password }, function (d) {
        $("#api_info").html('');
        $("#api_info").append('<h2>Logged in as ' + username + '</h2>');
        $("#api_info").append('<button id="logout" style="float:right; margin-top:-30px;">Logout</button>');
        $("input[name='Authorization']").val("Bearer " + d);
        localStorage.setItem("token", d);
        localStorage.setItem("username", username);
    });
});
