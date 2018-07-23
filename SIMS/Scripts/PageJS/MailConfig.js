var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {
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

    $scope.MailConfiguration = new Object();
    $scope.GetTestSectionList = function () {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/MailConfig/GetMailConfiguration',
            method: "POST"
        })
.success(function (data) {
    if (data == null || data =="") {
        $scope.MailConfiguration.UserCreation = false;
        $scope.MailConfiguration.TestAssign = false;
        $scope.MailConfiguration.Login = false;
        $scope.MailConfiguration.ChangePassword = false;
        $scope.MailConfiguration.ResultAfterTest = false;
        $scope.MailConfiguration.Questionpaper = false;

    }
    else {
        $scope.MailConfiguration = data;
    }
    jQuery.event.trigger("ajaxStop");
},
function (response) { // optional
    // failed

});
    }

    $scope.SaveUser = function () {
        $http({
            url: '/MailConfig/SaveMailConfig',
            method: "POST",
            data:$scope.MailConfiguration
        })
        .success(function (data) {
            if (data == true) {
                $scope.callmethod();
                ngNotify.set('Mail configuration save successfully.', 'success');
            }
            else {
                ngNotify.set('Error occured! please try again.', 'error');
            }



        }).error(function (xhr, status, error) {

        },
        function (response) { // optional


        });
    }

    $scope.callmethod = function () {
        directiveName.GetMessageList($http, $scope);
        $scope.GetTestSectionList();
    }
    $scope.selectquestion = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.Questionpaper = true;
        }
        else {
            $scope.MailConfiguration.Questionpaper = false;
        }
    }
    $scope.selectresultafter = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.ResultAfterTest = true;
        }
        else {
            $scope.MailConfiguration.ResultAfterTest = false;
        }
    }
    $scope.selectchpassword = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.ChangePassword = true;
        }
        else {
            $scope.MailConfiguration.ChangePassword = false;
        }
    }
    $scope.selectlogin = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.Login = true;
        }
        else {
            $scope.MailConfiguration.Login = false;
        }
    }
    $scope.selecttest = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.TestAssign = true;
        }
        else {
            $scope.MailConfiguration.TestAssign = false;
        }
    }
    $scope.selectuser = function (type) {
        if (type == 0) {
            $scope.MailConfiguration.UserCreation = true;
        }
        else {
            $scope.MailConfiguration.UserCreation = false;
        }
    }


});