var app = angular.module('EPortal', ['ngNotify', 'EAssessmentModule']);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datepicker({ dateFormat: 'dd-mm-yy' });
            });
        }
    }
}).controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {

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
    $scope.UserNo = 10;
    $scope.callmethod = function () {
        directiveName.GetMessageList($http, $scope);
        $scope.GetUserLIst($scope.UserNo);
    }
    $scope.disabledmore = false;
    $scope.GetUserLIst = function (take) {
        $scope.UserNo = take;
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Home/GetUserList',
            method: "POST",
            data: { take: take,searchkey:$scope.searchkey }
        })
.success(function (data) {
    jQuery.event.trigger("ajaxStop");
    $scope.UserList = data.messagelistuser;
    $scope.GetUserMessage($scope.UserList[0]);
    $scope.disabledmore = data.moredisabled;
},
function (response) { // optional
    // failed

});
    }

    $scope.GetMoreUser = function () {
        $scope.UserList = [];
        $scope.UserNo = $scope.UserNo + 10
        $scope.GetUserLIst($scope.UserNo);

    }    
    $scope.GetSearchData = function (event)
    {
        $scope.disabledmore = false;
        $scope.UserNo = 10;
        var charpress = event.which || event.keyCode;
        if (charpress == 13)
        {
            $scope.GetMoreUser();
        }
    }
    $scope.MesageCount = 3;

    $scope.SetCssForSelectedRecord = function (user)
    {
        $($scope.UserList).each(function (index, elemnt) {
            $("#"+elemnt.UserId).removeClass("selectedclassname");
            $("#star" + elemnt.UserId).removeClass("starcolor");
        });        
        $("#" + user.UserId).addClass("selectedclassname");
        $("#star" + user.UserId).addClass("starcolor");
    }

    $scope.GetUserMessage = function (user)
    {
        $scope.SelectedUser = user;
        jQuery.event.trigger("ajaxStart");

        $http({
            url: '/Home/GetUserMessageList',
            method: "POST",
            data: { userid: user.UserId, megcount: $scope.MesageCount }
        })
.success(function (data) {
    jQuery.event.trigger("ajaxStop");
    $scope.UserMessageList = data;
    $scope.SetCssForSelectedRecord(user);
},
function (response) { // optional
    // failed

});
    }
    $scope.SaveMessage = function ()
    {
        jQuery.event.trigger("ajaxStart");
        if ($scope.MessageInfo == undefined || $scope.MessageInfo == null || $scope.MessageInfo == "") {
            return false;
        }
        else {

            $http({
                url: '/Home/SaveUserMessageList',
                method: "POST",
                data: { userid: $scope.SelectedUser.UserId, message: $scope.MessageInfo }
            })
    .success(function (data) {
        if (data == true) {
            jQuery.event.trigger("ajaxStop");
            $scope.MesageCount = $scope.MesageCount + 1;
            $scope.GetUserMessage($scope.SelectedUser);
            $scope.MessageInfo = "";
        }
    },
    function (response) { // optional
        // failed

    });
        }
    }
    $scope.SaveMessageData = function (event) {
        var charpress = event.which || event.keyCode;
        if (charpress == 13) {            
            $scope.SaveMessage();
        }
    }    
    $scope.GetOlderMessage = function ()
    {
        $scope.MesageCount = $scope.MesageCount + 3;
        $scope.GetUserMessage($scope.SelectedUser);
    }
});