angular.module('confRoomClientApp.controllers', [])

.controller('AvailabilityCtrl', function ($scope, $log, $ionicModal, ApiService) {
  $scope.roomName = "[LOADING]";

  $scope.appointmentsLoaded = false;
  
  var email = window.localStorage['confroom_email'];
  var host = window.localStorage['confroom_host'];
  var port = window.localStorage['confroom_port'];

  //$scope.companyLogoUrl = "";
  $scope.companyLogoImage = "";
  $scope.asOf = null;
  $scope.roomColor = {"background-color":'#ccc'};
  $scope.roomAvailable = false;

  $scope.loadMailboxInfo = function () {
    $log.info('Getting mailbox info');
    ApiService.exchangeMailboxInfo(email, function (response) {
        $log.info('callback');
        $scope.roomName = response.displayName;
    });
  };
  $scope.loadMailboxInfo();

  $scope.getTimeString = function(dt) {
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

  $scope.loadConfiguration = function () {
    $log.info('Getting configuration');
    if (email && host && port) {
      ApiService.configGetConfig(email, function (response) {
        $scope.configSettings = response.configSettings;        
        $scope.companyLogoImage = response.configSettings.companyLogoImage;
        $scope.loadExchangeItems();
          setInterval(function () {
            $scope.loadExchangeItems();
          }, $scope.configSettings.pollingIntervalSeconds * 1000);
      });      
    }
    else {
      $scope.roomName = "[PLEASE CONFIGURE SERVER NAME]";
    }
  };

  $scope.getRoomColorStyle = function () {
    return $scope.roomColor;
  };

  $scope.setRoomColorBasedOnAppointments = function () {
    if (!$scope.appointmentsLoaded) return;
    $log.info('setRoomColorBasedOnAppointments');

    var available = true;
    var now = new Date();
    for (var i = 0; i < $scope.appointments.length; i++) {
      var a = $scope.appointments[i];
      a.isCurrentAppointment = false;

      if (a.startDate <= now && a.endDate > now) {
        available = false;
        a.isCurrentAppointment = true;
        break;
      }
    }

    $scope.roomAvailable = available;
    if (available) {
      $scope.roomColor = {"background-color":$scope.configSettings.availableColor}; 
    } else {
      $scope.roomColor = {"background-color":$scope.configSettings.busyColor}; 
    }      
  };
  
  $scope.loadExchangeItems = function () {
    $log.info('Getting exchange items.');
    ApiService.exchangeItems(email, function (response) {
        $log.info('callback');

        for (var i=0; i<response.appointments.length; i++) {
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

  $ionicModal.fromTemplateUrl('templates/bookRoomModal.html', {
    scope: $scope
  }).then(function(modal) {
    $scope.bookRoomModal = modal;
  });

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
    $scope.bookRoomModal.hide();
  };
  
  $scope.loadConfiguration();
})

.controller('AppCtrl', function($scope, $ionicModal, $timeout) {

  // With the new view caching in Ionic, Controllers are only called
  // when they are recreated or on app start, instead of every page change.
  // To listen for when this page is active (for example, to refresh data),
  // listen for the $ionicView.enter event:
  //$scope.$on('$ionicView.enter', function(e) {
  //});

  // Form data for the Server Connection modal
  $scope.serverConnectionData = {
    email: window.localStorage['confroom_email'],
    host: window.localStorage['confroom_host'],
    port: parseInt(window.localStorage['confroom_port']) || 8977
  };

  $scope.closeServerConnectionModal = function() {
    $scope.serverConnectionModal.hide();
  };

  $scope.openServerConnectionModal = function() {
    $scope.serverConnectionModal.show();
  };
  
  $scope.saveServerConnectionData = function() {
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
    scope: $scope
  }).then(function(modal) {
    $scope.serverConnectionModal = modal;
  });

});


