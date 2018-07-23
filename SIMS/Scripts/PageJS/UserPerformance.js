var app = angular.module('EPortal', ['ngNotify', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, ngNotify, $http, $window, directiveName) {
  
    $scope.TestId = "";

     $http({
         url: '/UserPerformance/GetAllTestList',
        method: "POST",
        
    })
.success(function (data) {

    $scope.OverallTestLabel = [];
    $scope.OverallTestData = [];
   
    $scope.AlltestData = data.OveralPerformance;
    $(data.OveralPerformance).each(function (index,eleemt) {

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

     $scope.TestWiseData = function ()
     {
         $scope.TestId;

         $http({
             url: '/UserPerformance/GetTestData?testid=' + $scope.TestId,
             method: "POST",

         })
.success(function (data) {

    $scope.SelectedTestData=[];
    if ($(data).length == 0)
    {
        $scope.SelectedTestData = [];        
        $("#canvas-holder").html('');
    }
    $(data).each(function (index, eleemt) {
        if (index == 0)
        {
            $("#canvas-holder").html("<canvas id='chart-area' width='300' height='300' />");
        }
        var d = {
            value: eleemt.TestSectionScore,
            color: index == 0 ? "#F7464A" : index == 1 ? "#46BFBD" : index == 2 ? "#949FB1" : index == 3 ? "#4D5360" : "#4D5360",
            highlight: "#FF5A5E",
            label: eleemt.TestSectionName
        }
        $scope.SelectedTestData.push(d);
    });
    var ctx1 = document.getElementById("chart-area").getContext("2d");
    window.myPie = new Chart(ctx1).Pie($scope.SelectedTestData);
},
function (response) { // optional
    // failed

});

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
