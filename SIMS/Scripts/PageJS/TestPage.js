var app = angular.module('TestPage', ['ngSanitize']);
app.controller('TestPageModel', function ($scope, $http, $window) {

    $scope.TestSectionList = [];
    $scope.ShowOptionSelectionBelow = false;
    $http({
        url: '/ApplyTest/GetTestSection',
        method: "POST",
        data: { seqno: "1" }
    })
.success(function (data) {
    $scope.TestSectionList = data.testsectionlist;
    $scope.GetSctionQuestion($scope.TestSectionList[0], 0);




    // Timer
    var fiveMinutes = data.totalminute,
    display = document.querySelector('#time');
    $scope.startTimer(fiveMinutes, display);


    //Instruction 
    $http({
        url: '/ApplyTest/GetTestInstruction',
        method: "POST"
    })
.success(function (data) {
    $scope.TestInstruction = data.testinstruction;
},
function (response) { // optional
    // failed

});
    //Instruction 
},
function (response) { // optional
    // failed

});
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
    $scope.SetSelectedSection = function (section) {
        $($scope.TestSectionList).each(function (index, elment) {

            $("#" + elment.Id).removeClass("btn-success");
        });
        $("#" + section.Id).addClass("btn-success");
    }
    $scope.ShowButtonType = function (question) {
        if (question.NotAttended == true) {
            return "";
        }
        if (question.Attended == true) {
            return "backcolorselected";
        }
        if (question.MarkForView == true) {
            return "backcolorreview";
        }
    }

    $scope.GetSctionQuestion = function (section, index) {
        $scope.SetSelectedSection(section);

        $scope.SelectedSection = section;
        $scope.SelectedSectionIndex = index;
        if ($($scope.TestSectionList).length != index) {
            $scope.NextSection = $scope.TestSectionList[index + 1];
        }
        else {
            $scope.NextSection = $scope.TestSectionList[0];
        }
        $http({
            url: '/ApplyTest/GetTestSectionDetail',
            method: "POST",
            data: section
        })
.success(function (data) {
    //$scope.SectionQuestionOption = data.question;
    $scope.SelectedQuestion = data.totalnoofquestion[0];
    $scope.GetQuestionNo(data.totalnoofquestion[0]);
    $scope.TotalQuestionForSection = data.totalnoofquestion;
},
function (response) { // optional
    // failed

});
    }
    $scope.GetQuestionNo = function (questionno) {
        $http({
            url: '/ApplyTest/GetQuestionNo',
            method: "POST",
            data: { qno: questionno, section: $scope.SelectedSection }
        })
.success(function (data) {
    $scope.SectionQuestionOption = data.question;
    $scope.SelectedQuestion = data.question;
    $scope.TotalQuestionForSection = data.totalnoofquestion;
},
function (response) { // optional
    // failed

});

    }
    $scope.GetCharvalue = function (index) {
        return String.fromCharCode(index + 65);
    }
    $scope.ResetOption = function () {
        var selectedSection = $scope.SelectedSection;
        var Question = $scope.SelectedQuestion;

        $http({
            url: '/ApplyTest/ResetQuestionOption',
            method: "POST",
            data: { Question: Question, section: $scope.SelectedSection, ExamTIme: $scope.Examtime }
        })
.success(function (data) {
    if (data == true) {
        var qnoonject = new Object();
        qnoonject.Qno = $scope.SelectedQuestion.QuestionNo;
        $scope.GetQuestionNo(qnoonject);
    }
},
function (response) { // optional
    // failed

});
    }
    $scope.NextQuestion = function () {
        var qnoonject = new Object();
        if ($($scope.TotalQuestionForSection).length >= ($scope.SelectedQuestion.QuestionNo + 1)) {
            qnoonject.Qno = $scope.SelectedQuestion.QuestionNo + 1;
            $scope.GetQuestionNo(qnoonject);
        }
        else {
            //call next section
            if ($scope.SelectedSectionIndex + 1 <= $($scope.TestSectionList).length - 1) {
                $scope.GetSctionQuestion($scope.TestSectionList[$scope.SelectedSectionIndex + 1], $scope.SelectedSectionIndex + 1);
            }
            else {
                $scope.GetSctionQuestion($scope.TestSectionList[0], 0);

            }
        }
    }


    $scope.SaveAndNext = function (type) {
        var selectedSection = $scope.SelectedSection;
        var Question = $scope.SelectedQuestion;

        $http({
            url: '/ApplyTest/SaveQuestionOption',
            method: "POST",
            data: { Question: Question, section: $scope.SelectedSection, type: type, ExamTIme: $scope.Examtime }
        })
.success(function (data) {
    if (data == true) {
        var qnoonject = new Object();
        var sectionmaxqno = $scope.TotalQuestionForSection[$($scope.TotalQuestionForSection).length - 1];
        if (parseFloat(sectionmaxqno.Qno) >= ($scope.SelectedQuestion.QuestionNo + 1)) {
            qnoonject.Qno = $scope.SelectedQuestion.QuestionNo + 1;
            $scope.GetQuestionNo(qnoonject);
        }
        else {
            //call next section
            if ($scope.SelectedSectionIndex + 1 <= $($scope.TestSectionList).length - 1) {
                $scope.GetSctionQuestion($scope.TestSectionList[$scope.SelectedSectionIndex + 1], $scope.SelectedSectionIndex + 1);
            }
            else {
                $scope.GetSctionQuestion($scope.TestSectionList[0], 0);

            }
        }

    }
},
function (response) { // optional
    // failed

});



    }
    $scope.RadioCheck = function (option, type) {
        if (type == 0) {
            $($scope.SectionQuestionOption.TestQuestionoptionList).each(function (index, element) {
                element.Selected = false;
            });
            option.Selected = true;
        }
    }
    $scope.submitclicked = false;
    $scope.SubmitTest = function () {
        if ($scope.submitclicked == false) {
            $http({
                url: '/ApplyTest/SubmitTest',
                method: "POST",
            })
.success(function (data) {
    if (data == true) {
        $scope.ShowQuestion = false;
        //$scope.ShowResult();
    }
},
function (response) { // optional
    // failed

});

        }
        else {
            $scope.submitclicked = true;
            $scope.ShowQuestion = false;
        }


    }
    $scope.ShowQuestion = true;
    $scope.ShowResult = function () {

        $http({
            url: '/ApplyTest/GetResult',
            method: "POST"
        })
.success(function (data) {
    $scope.ShowQuestion = false;
    $scope.MarkList = data.useranslist;
    $scope.TestName = data.testname;
    $scope.USerName = data.usename;
},
function (response) { // optional
    // failed

});
    }
    $scope.CloseTest = function () {
        $window.location.href = "/Home/Signout";
    }
    $scope.Submitcall = 0;
    $scope.Examtime = 0;
    $scope.startTimer = function (duration, display) {
        var timer = duration, minutes, seconds;
        setInterval(function () {
            minutes = parseInt(timer / 60, 10)
            seconds = parseInt(timer % 60, 10);

            minutes = minutes < 10 ? "0" + minutes : minutes;
            seconds = seconds < 10 ? "0" + seconds : seconds;

            display.textContent = minutes + ":" + seconds;
            $scope.Examtime = timer;
            if (--timer < 0) {
                //submit call 

                if ($scope.Submitcall == 0) {
                    $scope.SubmitTest();
                    $scope.Submitcall = 1;
                }
            }
        }, 1000);


    }

    $scope.checkBoxCheck = function (question) {
        if (question.Selected == true) {
            question.Selected = false;
        }
        else {
            question.Selected = true;
        }
    }



});