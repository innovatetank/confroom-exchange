angular.module('confRoomClientApp.controllers', [])

.controller('AvailabilityCtrl', function ($scope, $log, ApiService) {
  $scope.roomName = "[LOADING]";

  var email = 'Chicago.Room@usa-truck.com';

  $log.info('Getting mailbox info');
  ApiService.exchangeMailboxInfo(email, function (response) {
      $log.info('callback');
      $scope.roomName = response.displayName;
  });

  var getTimeString = function(dt) {
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
  };

  $log.info('Getting exchange test data.');
  ApiService.exchangeTest(email, function (response) {
      $log.info('callback');

      for (var i=0; i<response.appointments.length; i++) {
        var a = response.appointments[i];

        a.startTime = getTimeString(new Date(a.start));
        a.endTime = getTimeString(new Date(a.end));        
      }
      
      $scope.appointments = response.appointments;
  });



})

.controller('AppCtrl', function($scope, $ionicModal, $timeout) {

  // With the new view caching in Ionic, Controllers are only called
  // when they are recreated or on app start, instead of every page change.
  // To listen for when this page is active (for example, to refresh data),
  // listen for the $ionicView.enter event:
  //$scope.$on('$ionicView.enter', function(e) {
  //});

  // Form data for the login modal
  $scope.loginData = {};

  // Create the login modal that we will use later
  $ionicModal.fromTemplateUrl('templates/exchangeSettingsModal.html', {
    scope: $scope
  }).then(function(modal) {
    $scope.modal = modal;
  });

  // Triggered in the login modal to close it
  $scope.closeLogin = function() {
    $scope.modal.hide();
  };

  // Open the login modal
  $scope.login = function() {
    $scope.modal.show();
  };

  // Perform the login action when the user submits the login form
  $scope.doLogin = function() {
    console.log('Doing login', $scope.loginData);

    // Simulate a login delay. Remove this and replace with your login
    // code if using a login system
    $timeout(function() {
      $scope.closeLogin();
    }, 1000);
  };
})

.controller('PlaylistsCtrl', function($scope) {
  $scope.playlists = [
    { title: 'Reggae', id: 1 },
    { title: 'Chill', id: 2 },
    { title: 'Dubstep', id: 3 },
    { title: 'Indie', id: 4 },
    { title: 'Rap', id: 5 },
    { title: 'Cowbell', id: 6 }
  ];
})

.controller('PlaylistCtrl', function($scope, $stateParams) {
});
