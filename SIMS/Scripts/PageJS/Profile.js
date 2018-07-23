var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {

    $scope.TestId = "";
    $scope.callmethod = function ()
    {
        directiveName.GetMessageList($http, $scope);
        $http({
            url: '/UserPerformance/GetAllTestList',
            method: "POST",

        })
.success(function (data) {

    $scope.OverallTestLabel = [];
    $scope.OverallTestData = [];

    $scope.AlltestData = data.OveralPerformance;
    $(data.OveralPerformance).each(function (index, eleemt) {

        $scope.OverallTestLabel.push(eleemt.TestName);
        $scope.OverallTestData.push(eleemt.TestScore);
    });

    var lineChartData = {
        labels: $scope.OverallTestLabel,
        datasets: [
            {
                label: "My First dataset",
                fillColor: "rgba(255,0,0,1)",
                strokeColor: "rgba(0,255,0,1)",
                pointColor: "rgba(220,220,220,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(220,220,220,1)",
                data: $scope.OverallTestData
            }

        ]

    }

    var ctx = document.getElementById("canvas").getContext("2d");
    window.myLine = new Chart(ctx).Line(lineChartData, {
        responsive: true
    });
    $scope.TestId = data.OveralPerformance[0].TestId;
    $scope.TestWiseData();
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


    


   
   

});