var adminPassword = "54321";

angular.module('confRoomClientApp.controllers', [])

.controller('AvailabilityCtrl', function ($scope, $log, $ionicModal, $ionicPopup, $ionicLoading, ApiService) {
    $scope.roomName = "[LOADING]";

    $scope.appointmentsLoaded = false;

    // If the room is currently available, then we show the next appointment booked for the room. 
    $scope.nextAppointment = null;

    var email = window.localStorage['confroom_email'];
    var host = window.localStorage['confroom_host'];
    var port = window.localStorage['confroom_port'];

    //$scope.companyLogoUrl = "";
    $scope.companyLogoImage = "";
    $scope.asOf = null;
    $scope.roomColor = {
        "background-color": '#ccc'
    };
    $scope.roomAvailable = false;
    $scope.availableText = "AVAILABLE";

    $scope.loadMailboxInfo = function () {
        $log.info('Getting mailbox info');
        ApiService.exchangeMailboxInfo(email, function (response) {
            $log.info('callback');
            $scope.roomName = response.displayName;
        });
    };
    $scope.loadMailboxInfo();

    $scope.getTimeString = function (dt) {
        if (dt) {
            var h = dt.getHours();
            var m = dt.getMinutes();
            var ampm = "am";
            if (h > 11) {
                ampm = "pm";
                h -= 12;
                if (h == 0) h = 12;
            }
            if (m < 10) {
                m = '0' + m;
            }
            return h + ":" + m + ampm;
        }
    };
    $scope.getDateString = function (dt) {
        if (dt) {
            var monthNames = ["January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"];

            var m = dt.getMonth();
            var d = dt.getDay();
            var y = dt.getFullYear();

            return monthNames[m] + " " + d + ", " + y;
        }
    };

    $scope.loadConfiguration = function () {
        $log.info('Getting configuration');
        if (email && host && port) {
            ApiService.configGetConfig(email, function (response) {
                $scope.configSettings = response.configSettings;
                $scope.companyLogoImage = response.configSettings.companyLogoImage;
                $scope.availableRoomText = response.configSettings.availableRoomText;
                $scope.busyRoomText = response.configSettings.busyRoomText;
                adminPassword = response.configSettings.adminMenuPassword;
                $scope.loadExchangeItems();
                setInterval(function () {
                    $scope.loadExchangeItems();
                }, $scope.configSettings.pollingIntervalSeconds * 1000);
            });
        } else {
            $scope.roomName = "[PLEASE CONFIGURE SERVER NAME]";
        }
    };

    $scope.getRoomColorStyle = function () {
        return $scope.roomColor;
    };

    $scope.setRoomColorBasedOnAppointments = function () {
        if (!$scope.appointmentsLoaded) return;
        $log.info('setRoomColorBasedOnAppointments');

        // Is the room available and what is the current appointment?
        var available = true;
        var now = new Date();
        for (var i = 0; i < $scope.appointments.length; i++) {
            var a = $scope.appointments[i];

            // Is this appointment in progress?
            a.isCurrentAppointment = false;
            if (a.startDate <= now && a.endDate > now) {
                available = false;
                a.isCurrentAppointment = true;
            }
        }

        // Calculate the next appointment time 
        if (available) {
            for (var i = 0; i < $scope.appointments.length; i++) {
                var a = $scope.appointments[i];

                if (!a.isCurrentAppointment) {
                    if (a.startDate > now && ($scope.nextAppointment == null || a.startDate < $scope.nextAppointment.startDate)) {
                        $scope.nextAppointment = a;
                    }
                }
            }
        } else {
            // Room is busy
            $scope.nextAppointment = null;
        }

        // Set classes on scope for room availability color
        $scope.roomAvailable = available;
        if (available) {
            $scope.roomColor = {
                "background-color": $scope.configSettings.availableColor
            };
            $scope.availableText = $scope.availableRoomText;
        } else {
            $scope.roomColor = {
                "background-color": $scope.configSettings.busyColor
            };
            $scope.availableText = $scope.busyRoomText;
        }
    };

    $scope.loadExchangeItems = function () {

        if (window.StatusBar) {
            console.log("Hiding status bar");
            StatusBar.hide(); //styleDefault();
        } else {
            console.log("NOT hiding status bar");
        }

        $log.info('Getting exchange items.');
        ApiService.exchangeItems(email, function (response) {
            $log.info('callback');

            for (var i = 0; i < response.appointments.length; i++) {
                var a = response.appointments[i];

                a.startDate = new Date(a.start);
                a.endDate = new Date(a.end);
                a.startTime = $scope.getTimeString(a.startDate);
                a.endTime = $scope.getTimeString(a.endDate);
            }

            $scope.appointments = response.appointments;
            $scope.appointmentsLoaded = true;
            $scope.asOf = new Date();

            $scope.setRoomColorBasedOnAppointments();
        });
    };

    $scope.allowBookRoom = function () {
        return $scope.appointmentsLoaded && $scope.roomAvailable;
    };

    $scope.bookRoomButton = function () {
        $scope.bookMinutes = 15;
        $scope.bookRoomModal.show();
    };
    $scope.closeBookRoom = function () {
        $scope.bookRoomModal.hide();
    }

    $scope.clickPlus = function () {
        if ($scope.bookMinutes < 120) {
            $scope.bookMinutes += 15;
        }
    }

    $scope.clickMinus = function () {
        if ($scope.bookMinutes > 15) {
            $scope.bookMinutes -= 15;
        }
    }

    $scope.bookItButton = function () {
        $log.info("BOOK IT for " + $scope.bookMinutes + " MINUTES");

        $ionicLoading.show({
            template: '<h1>Saving...</h1>'
        });
        $scope.bookRoomModal.hide();

        ApiService.exchangeBook(email, $scope.bookMinutes, function (response) {
            if (response && response.success) {
                location.reload();
            } else {
                $ionicLoading.hide();
                $ionicPopup.alert({
                    title: 'Error',
                    template: response.errorMessage
                });
            }
        });
    };

    $ionicModal.fromTemplateUrl('templates/bookRoomModal.html', {
        scope: $scope
    }).then(function (modal) {
        $scope.bookRoomModal = modal;
    });

    $scope.loadConfiguration();
})

.controller('AppCtrl', function ($scope, $ionicModal, $ionicPopup, $timeout) {

    // With the new view caching in Ionic, Controllers are only called
    // when they are recreated or on app start, instead of every page change.
    // To listen for when this page is active (for example, to refresh data),
    // listen for the $ionicView.enter event:
    //$scope.$on('$ionicView.enter', function(e) {
    //});

    if (window.StatusBar) {
        StatusBar.hide();
    }

    // Form data for the Server Connection modal
    $scope.serverConnectionData = {
        email: window.localStorage['confroom_email'],
        host: window.localStorage['confroom_host'],
        port: parseInt(window.localStorage['confroom_port']) || 8977
    };

    $scope.closeServerConnectionModal = function () {
        $scope.serverConnectionModal.hide();
    };

    $scope.openServerConnectionModal = function () {
        console.log('OPENING ADMIN MENU');

        $scope.adminPwd = {
            password: null
        };
        
        $scope.validateAdminPasswordAndOpenConnections = function () {
            if ($scope.adminPwd.password == adminPassword) {
                setTimeout(function() {
                    $scope.serverConnectionModal.show();    
                }, 1);                
            } else {
                setTimeout(function() {
                    var alertPopup = $ionicPopup.alert({
                        title: 'Incorrect password',
                        template: 'You have entered the incorrect password. Please try again.'
                    });                    
                });
            }
        };

        $scope.connectionsModal = $ionicPopup.show({
            template: '<form ng-submit="connectionsModal.close();validateAdminPasswordAndOpenConnections()"><input type="password" ng-model="adminPwd.password" style="padding-left:7px;padding-right:7px;font-size:32px;height:50px"></form>',
            title: 'Admin Password',
            subTitle: 'Please enter the administrator password to access this function:',
            scope: $scope,
            buttons: [
                {
                    text: 'Cancel'
                },
                {
                    text: '<b>Login</b>',
                    type: 'button-positive',
                    onTap: function (e) {
                        $scope.validateAdminPasswordAndOpenConnections();
                        return $scope.adminPwd.password;
                    }
                }
            ]
        });
    };

    $scope.saveServerConnectionData = function () {
        var data = $scope.serverConnectionData;
        console.log('Saving', data);

        window.localStorage['confroom_email'] = data.email;
        window.localStorage['confroom_host'] = data.host;
        window.localStorage['confroom_port'] = data.port;

        $scope.closeServerConnectionModal();
        location.reload();
    };

    // Create the Server Connection modal that we will use later
    $ionicModal.fromTemplateUrl('templates/serverConnectionModal.html', {
        scope: $scope,
        backdropClickToClose: false
    }).then(function (modal) {
        $scope.serverConnectionModal = modal;
    });

});