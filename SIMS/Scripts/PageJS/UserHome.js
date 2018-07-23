var app = angular.module('EPortal', ['ngNotify', 'EAssessmentModule']);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datepicker({
                    changeMonth: true,
                    changeYear: true,
                    minDate: "1D"
                });
            });
        }
    }
})
.controller('EPortalCont', function ($scope, ngNotify, $http, $window, directiveName) {

    $scope.ShowOrnot = false;
    $scope.HourTime = [];
    $scope.MinTime = [];
    $scope.EventHour = "Hour";
    $scope.EventMin = "Min";
    $scope.HourTime.push("Hour");
    for (var i = 0; i < 24; i++) {
        if (i > 9) {
            $scope.HourTime.push(i);
        }
        else {
            $scope.HourTime.push("0"+i);
        }
        
    }
    //for (var j = 0; j <= 60; j+5) {
    //    $scope.MinTime.push(j);

    //}
    //$scope.HourTime = ["00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
    //                "10", "11", "12", "13", "14", "15",
    //                "16", "17", "18", "19", "20", "21", "22", "23"];
    $scope.MinTime = ["Min","00", "05", "10", "15", "20", "25", "30", "35", "40", "45",
                    "50", "55", "60"];

    $scope.callmethod = function () {

        directiveName.GetMessageList($http, $scope);
        jQuery.event.trigger("ajaxStart");
        $scope.GetEventDetail();
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
            url: '/UserHome/GetAllEvent',
            method: "POST"
        })
  .success(function (data) {
      $scope.AllEventList = data;
      jQuery.event.trigger("ajaxStop");
  },
  function (response) { // optional
      // failed

  });

    }

    $scope.ClrEventValue = function () {
        $scope.EventCreate.EventName = "";
        $scope.EventCreate.EventDate = "";
        $scope.EventCreate.EventDescription = "";
        $scope.EventCreate.Hour = 0;
        $scope.EventCreate.Min = 0;
        $scope.EventHour = "Hour";
        $scope.EventMin = "Min";
    }
    $scope.SaveEvent = function () {

        var ifsuccess = 0;

        $scope.EventCreate.Hour = $scope.EventHour;
        $scope.EventCreate.Min = $scope.EventMin;      

        if ($scope.EventCreate != undefined) {
            if ($scope.EventCreate.EventName == undefined || $scope.EventCreate.EventName == "") {
                ngNotify.set('Please enter Event Name', 'error');
                ifsuccess = 1;

            }
            if ($scope.EventCreate.EventDate == undefined || $scope.EventCreate.EventDate == "") {
                ngNotify.set('Please enter Event Date', 'error');
                ifsuccess = 1;
            }
        }
        else {
            ngNotify.set('Please enter Details', 'error');
            ifsuccess = 1;
        }
        if (ifsuccess == 0) {
            $http({
                url: '/UserHome/SaveEvent',
                method: "POST",
                data: $scope.EventCreate
            })
    .success(function (data) {

        if (data == true) {
            $scope.GetEventDetail();
            $('#eventmodel').modal('hide');
            ngNotify.set("Event Created Successfully..", 'success');
        }
        else {
            ngNotify.set('Error ocured!please try again.', 'error');
        }
        jQuery.event.trigger("ajaxStop");
    },
    function (response) { // optional
        // failed

    });

        }
        jQuery.event.trigger("ajaxStop");


    }

    $scope.GetMail = function () {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/UserHome/GetMail',
            method: "POST"
        })
     .success(function (data) {
         $scope.AllMail = data;
         jQuery.event.trigger("ajaxStop");
     },
     function (response) { // optional
         // failed

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

    $scope.CheckUser = function () {

    }
    $scope.MailData = new Object();
    $scope.SendMail = function () {

        var emailvalidation = validateEmail($scope.MailData.MailTo);
        if (emailvalidation == true) {
            jQuery.event.trigger("ajaxStart");
            $http({
                url: '/UserHome/SandMail',
                method: "POST",
                data: { maildata: $scope.MailData }
            })
         .success(function (data) {

             if (data.samdmail == true) {
                 jQuery.event.trigger("ajaxStop");
                 ngNotify.set("Mail send successfully.", 'success');
                 $scope.MailData.MailTo = "";
                 $scope.MailData.MailHeading = "";
                 $scope.MailData.MailBody = "";
             }
             else {
                 if (data.msg == "") {
                     ngNotify.set("Error occured!please try again.", 'error');
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
            ngNotify.set("Please enter valid Email.", 'error');
        }


    }
    $scope.EnableDIsable = function () {

        var returnvalue = false;
        if ($scope.MailData.MailTo == undefined || $scope.MailData.MailTo == null || $scope.MailData.MailTo == "") {
            returnvalue = true;
        }
        if ($scope.MailData.MailHeading == undefined || $scope.MailData.MailHeading == null || $scope.MailData.MailHeading == "") {
            returnvalue = true;
        }
        if ($scope.MailData.MailBody == undefined || $scope.MailData.MailBody == null || $scope.MailData.MailBody == "") {
            returnvalue = true;
        }
        return returnvalue;
    }
    function validateEmail(email) {
        var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(email);
    }
});
