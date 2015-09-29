angular.module('confRoomClientApp.services', [])

.factory('ApiService',     
    function ($http, $log) {

        var service = {};

        service.getBaseApiUrl = function () {
            return "http://localhost:8977/";
        };

        service.post = function (route, request, callback) {
            var url = service.getBaseApiUrl() + route;

            var jsonRequest = angular.toJson(request, true);
            $log.info("REQUEST to " + url + ':\r\n' + jsonRequest);

            var promise = $http({
                method: 'POST',
                url: url,
                data: jsonRequest,
                headers: { 'Content-Type': 'application/json' }
            })
            .success(function (response) {
                $log.info("RESPONSE from " + url + ':\r\n' + angular.toJson(response, true));
                if (callback) callback(response);
            });

            return promise;
        };

        service.get = function (route, callback) {
            var url = service.getBaseApiUrl() + route;

            $log.info("REQUEST to " + url);

            var promise = $http({
                method: 'GET',
                url: url,
                headers: { 'Content-Type': 'application/json' }
            })
            .success(function (response) {
                $log.info("RESPONSE from " + url + ':\r\n' + angular.toJson(response, true));
                if (callback) callback(response);
            });

            return promise;
        };

        service.exchangeTest = function (email, callback) {
            // GET: /exchange/test?mailbox=emailaddress@domain.com
            return service.get('exchange/test?mailbox=' + email, callback);
        };

        service.exchangeMailboxInfo = function (email, callback) {
            // GET: /exchange/test?email=emailaddress@domain.com
            return service.get('exchange/mailbox?email=' + email, callback);
        };

        service.configGetConfig = function (email, callback) {
            // GET: /config?email=mailboxemail@domain.com
            return service.get('config?email=' + email, callback);
        };

        service.exchangeBook = function (email, minutes, callback) {
            var request = {
                email: email,
                minutes: minutes
            };
            return service.post('exchange/book', request, callback);
        };

        return service;
    });