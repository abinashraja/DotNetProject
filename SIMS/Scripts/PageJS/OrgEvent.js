var app = angular.module('EPortal', ['ngNotify', 'EAssessmentModule']);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datetimepicker({
                    changeMonth: true,
                    changeYear: true,
                    minDate: "1D"
                   
                });
            });
        }
    }
}).controller('EPortalCont', function ($scope, ngNotify, $http, $window, directiveName) {

    $scope.ShowOrnot = false;

    $scope.callmethod = function () {

        directiveName.GetMessageList($http, $scope);
        $scope.GetEventDetail();
        jQuery.event.trigger("ajaxStart");        
        //myService.foo();

        $http({
            url: '/UserHome/GetOrgData',
            method: "POST"
        })
     .success(function (data) {
         $scope.DashBoardData = data;
         jQuery.event.trigger("ajaxStop");
         //$scope.GetMail();
         //if ($scope.DashBoardData.IsApplicant == true) {
         //    $scope.RedirectTOPerformance();
         //}

         // $confirm({ text: 'Are you sure you want to delete?', title: 'Delete it', ok: 'Yes', cancel: 'No' })
         //.then(function () {
         //    alert("f");
         //});

         if ($scope.DashBoardData.IsApplicant == true && ($scope.DashBoardData.IsSuper == false)) {

             $window.location.href = '/Profile/Index';
         }
         if (($scope.DashBoardData.IsApplicant == false) && ($scope.DashBoardData.IsSuper == false)) {
             $scope.ShowOrnot = true;

         }
         else {
             $scope.ShowOrnot = false;
         }
     },
     function (response) { // optional
         // failed

     });
    }
    $scope.GetEventDetail = function () {
        jQuery.event.trigger("ajaxStart");

        $http({
            url: '/UserHome/GetAllEventCalender',
            method: "POST"
        })
  .success(function (data) {
      $scope.AllEventList = data;
      jQuery.event.trigger("ajaxStop");
      $scope.GetAllEventDetails();
  },
  function (response) { // optional
      // failed

  });

    }
    $scope.CalDetailsList = [];
    $scope.GetAllEventDetails = function ()
    {
        
        var AllEventList = [];

        $($scope.AllEventList).each(function (index, elemnt) {
            $scope.d = {
                "title": elemnt.EventName,
                "allday": "false",
                "description": elemnt.EventDescription,
                "start":elemnt.EventDate ,//moment().subtract('days', 14),
                "end":elemnt.EventDate //moment().subtract('days', 14),
            };
            AllEventList.push($scope.d);
        });
        
        $scope.AllEventList = AllEventList;

        $('#bootstrapModalFullCalendar').fullCalendar({
            header: {
                left: '',
                center: 'prev title next',
                right: ''
            },
            eventClick: function (event, jsEvent, view) {
                $('#modalTitle').html(event.title);
                $('#modalBody').html(event.description);
                $('#eventUrl').attr('href', event.url);
                $('#fullCalModal').modal();
                return false;
            },
            events: $scope.AllEventList
            //[
            //   {
            //       "title": "Free Pizza",
            //       "allday": "false",
            //       "description": "<p>This is just a fake description for the Free Pizza.</p><p>Nothing to see!</p>",
            //       "start": moment().subtract('days', 14),
            //       "end": moment().subtract('days', 14),
            //       "url": "http://www.mikesmithdev.com/blog/coding-without-music-vs-coding-with-music/"
            //   },
            //   {
            //       "title": "DNUG Meeting",
            //       "allday": "false",
            //       "description": "<p>This is just a fake description for the DNUG Meeting.</p><p>Nothing to see!</p>",
            //       "start": moment().subtract('days', 10),
            //       "end": moment().subtract('days', 10),
            //       "url": "http://www.mikesmithdev.com/blog/youtube-video-event-tracking-with-google-analytics/"
            //   },
            //   {
            //       "title": "Staff Meeting",
            //       "allday": "false",
            //       "description": "<p>This is just a fake description for the Staff Meeting.</p><p>Nothing to see!</p>",
            //       "start": moment().subtract('days', 6),
            //       "end": moment().subtract('days', 6),
            //       "url": "http://www.mikesmithdev.com/blog/what-if-your-website-were-an-animal/"
            //   },
            //   {
            //       "title": "Poker Night",
            //       "allday": "false",
            //       "description": "<p>This is just a fake description for the Poker Night.</p><p>Nothing to see!</p>",
            //       "start": moment().subtract('days', 2),
            //       "end": moment().subtract('days', 2),
            //       "url": "http://www.mikesmithdev.com/blog/how-to-make-a-qr-code-in-asp-net/"
            //   },
            //   {
            //       "title": "NES Gamers",
            //       "allday": "false",
            //       "description": "<p>This is just a fake description for the NES Gamers.</p><p>Nothing to see!</p>",
            //       "start": moment(),
            //       "end": moment(),
            //       "url": "http://www.mikesmithdev.com/blog/name-that-nes-soundtrack/"
            //   }

            //]
        });

    }
    $scope.Changepassword = function () {
        if ($scope.ChangePassword == undefined) {
            ngNotify.set('Please enter detsils.', 'error');
            return false;
        }
        if ($scope.ChangePassword.oldpassword == undefined || $scope.ChangePassword.oldpassword == "") {
            ngNotify.set('Please enter Current password', 'error');
            return false;
        }
        if ($scope.ChangePassword.newpassword == undefined || $scope.ChangePassword.newpassword == "") {
            ngNotify.set('Please enter New Password.', 'error');
            return false;
        }
        if ($scope.ChangePassword.oldpassword == $scope.ChangePassword.newpassword) {
            ngNotify.set('Current password and New Password should not be same.', 'error');
            return false;
        }

        if ($scope.ChangePassword.newpassword == $scope.ChangePassword.renewpassword) {
            $http({
                url: '/Home/ChangePassword',
                method: "POST",
                data: $scope.ChangePassword
            })
           .success(function (data) {
               if (data.result == true) {
                   $('#passwordmodel').modal('hide');
                   $scope.ChangePassword = [];
                   ngNotify.set("Password change successfully.", 'success');

               }
               else {
                   if (data.msg == "") {
                       ngNotify.set('Error ocured!please try again.', 'error');
                   }
                   else {
                       ngNotify.set(data.msg, 'error');
                   }
               }
           },
           function (response) { // optional
               // failed

           });
        }
        else {
            ngNotify.set('Confirm New Password do not match with New Password. ', 'error');
        }
    }

    //$scope.RedirectTOPerformance = function ()
    //{
    //    $window.Location = "/UserPerformance/Index";
    //}

    
    
    
    
});
