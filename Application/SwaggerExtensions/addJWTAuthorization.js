(function () {
    $(function () {
        $('#input_apiKey').off();
        $('#input_apiKey').attr('placeholder', 'JWT Token');
        $('#input_apiKey').on('change', function () {
            var key = this.value;
            if (key && key.trim() !== '') {
                window.authorizations.add('key', new SwaggerClient.ApiKeyAuthorization('Authorization', 'Bearer ' + key, 'header'));
            }
        });
    });
})();