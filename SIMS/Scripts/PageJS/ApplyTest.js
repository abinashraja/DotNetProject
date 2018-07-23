
var app = angular.module('EPortal', ['ngNotify', 'ngSanitize', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, $window, directiveName) {

    $scope.HaveTest = false;
    $scope.TestInstruction = "";
    $scope.AfterOpen = false;
    $scope.EnebleNext = false;    
    $http({
        url: '/ApplyTest/GetTestInstruction',
        method: "POST"
    })
.success(function (data) {
    $scope.HaveTest = data.havetest;
    $scope.TestInstruction = data.testinstruction;
},
function (response) { // optional
    // failed

});

    $scope.OPenTest = function () {        
        $scope.AfterOpen = true;
        //$("#Bodyid").addClass("fade");

        $window.location.href = '/ApplyTest/TestPage';

        //$window.open('/ApplyTest/TestPage', 'Apply Test ', 'height=' + screen.height + ',width=' + screen.width + ',resizable=no,scrollbars=yes,toolbar=yes,menubar=yes,location=yes')
        //$window.opener = null;
    }
    $scope.callmethod = function () {
        directiveName.GetMessageList($http, $scope);
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
    
});






