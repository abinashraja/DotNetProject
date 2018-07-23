var app = angular.module('EPortal', ['ngNotify', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {





    $scope.ShowUnique = true;
    $scope.ShowGroup = false;
    $scope.NoOFUniquesQuestion = 0;
    $scope.NoOFGroupQuestion = 0;
    $scope.currentPage = 0;
    $scope.currentPageGroup = 0;
    $scope.NewTestQuestion = [];
    $http({
        url: '/TestQuestion/GetTestList',
        method: "POST"
    })
.success(function (data) {
    $scope.NewTestQuestion.TestList = data.testlist;
    $scope.NewTestQuestion.QuestionTypeList = data.QuestionTypelIst;
    $scope.NewTestQuestion.TestSectionList = data.testsection;
    $scope.NewTestQuestion.TestId = "0";
    $scope.NewTestQuestion.TestSectionId = "0";
    $scope.NewTestQuestion.QuestionTypeId = "0";
    $scope.ShowQnique();

},
function (response) { // optional
    // failed

});

    $scope.GetTestSection = function () {

        $http({
            url: '/TestQuestion/GetTestSectionList',
            method: "POST",
            data: { testid: $scope.NewTestQuestion.TestId }
        })
.success(function (data) {

    $scope.NewTestQuestion.TestSectionList = data;
    $scope.NewTestQuestion.TestSectionId = "0";

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

    $scope.ShowUniqueBar = function () {
        $scope.ShowUnique = true;
        $("#unique").addClass("selectedcol");
        $("#groupques").removeClass("selectedcol");
    }
    $scope.ShowGorupBar = function () {
        $scope.ShowUnique = false;
        $("#groupques").addClass("selectedcol");
        $("#unique").removeClass("selectedcol");
    }
    $scope.ShowQnique = function (type) {
        $scope.ShowUnique = true;
        $("#unique").addClass("selectedcol");
        $("#groupques").removeClass("selectedcol");
        if ($scope.NewTestQuestion.QuestionTypeId != "0" && $scope.NewTestQuestion.TestId != "0" && $scope.NewTestQuestion.TestSectionId != "0" && ($scope.NewTestQuestion.NoOFQuestion != undefined || NewTestQuestion.NoOFQuestion != "")) {
            $scope.GetUniqueQuestion();
        }
    }
    $scope.ShowGroupQuestion = function (type) {
        $scope.ShowUnique = false;

        $("#groupques").addClass("selectedcol");
        $("#unique").removeClass("selectedcol");
        $scope.GetGroupQuestion();
    }
    $scope.CallDefineFunction = function () {
        if ($scope.NewTestQuestion.QuestionTypeId != "0" && $scope.NewTestQuestion.TestId != "0" && $scope.NewTestQuestion.TestSectionId != "0" && $scope.NewTestQuestion.NoOFQuestion > 0) {

            $scope.currentPage = 0;
            $scope.currentPageGroup = 0;
            $scope.GetUniqueQuestion();
            $scope.GetGroupQuestion();


        }
        else {
            var errormsg = "";
            if ($scope.NewTestQuestion.TestId == "0") {
                errormsg += "Please select Test.";
            }
            if ($scope.NewTestQuestion.TestSectionId == "0") {
                errormsg += "Please select Test Section.";
            }
            if ($scope.NewTestQuestion.QuestionTypeId == "0") {
                errormsg += "Please select Question Type.";
            }
            if (($scope.NewTestQuestion.NoOFQuestion == undefined || $scope.NewTestQuestion.NoOFQuestion == "")) {
                errormsg += "Please enter No. Of Question.";
            }
            if (errormsg != "") {
                ngNotify.set(errormsg, 'error');
            }
        }
    }
    $scope.GetUniqueQuestion = function () {
        $http({
            url: '/TestQuestion/GetUniqueQuestionList',
            method: "POST",
            data: { questiontypeid: $scope.NewTestQuestion.QuestionTypeId, testid: $scope.NewTestQuestion.TestId, testsectionid: $scope.NewTestQuestion.TestSectionId }
        })
.success(function (data) {
    $(data.questionlist).each(function (index, element) {
        element.QuestionText = $scope.htmlToPlaintext(element.QuestionText);
    });
    $scope.NoOFUniquesQuestion = data.noofquestionsel;
    $scope.GridUniqueItemList = data.questionlist;
    $scope.GetPageRequestData();
},
function (response) { // optional
    // failed

});
    }
    $scope.GetGroupQuestion = function () {
        $http({
            url: '/TestQuestion/GetGroupQuestionList',
            method: "POST",
            data: { questiontypeid: $scope.NewTestQuestion.QuestionTypeId, testid: $scope.NewTestQuestion.TestId, testsectionid: $scope.NewTestQuestion.TestSectionId }
        })
.success(function (data) {
    $(data.questionlist).each(function (index, element) {
        element.QuestionText = $scope.htmlToPlaintext(element.QuestionText);
    });
    $scope.NoOFGroupQuestion = data.noofgroupquestion;
    $scope.GridItemGorupList = data.questionlist;
    $scope.GetPageRequestDataGroup();
},
function (response) { // optional
    // failed

});
    }
    $scope.htmlToPlaintext = function (text) {
        return (text ? String(text).replace(/<[^>]+>/gm, '') : '').replace(/&nbsp;/g, '').replace(/\<br\s*[\/]?>/gi, '');;
    }
    $scope.SelectUniqueQuestion = function (question, type) {
        if (question.Operation == "Edit") {
            return false;
        }
        if (type == 0) {

            var totalquestionadded = $scope.NoOFUniquesQuestion + $scope.NoOFGroupQuestion;
            if (totalquestionadded < parseInt($scope.NewTestQuestion.NoOFQuestion)) {
                question.Selected = true;
                $scope.NoOFUniquesQuestion = $scope.NoOFUniquesQuestion + 1;
            }
            else {

                ngNotify.set('Already ' + $scope.NewTestQuestion.NoOFQuestion + ' selected  for above test.', 'error');
            }
        }
        else {
            question.Selected = false;
            $scope.NoOFUniquesQuestion = $scope.NoOFUniquesQuestion - 1;
        }


    }

    $scope.SelectGroupQuestion = function (question, type) {
        if (question.Operation == "Edit") {
            return false;
        }
        if (type == 0) {
            var totalquestionadded = $scope.NoOFUniquesQuestion + $scope.NoOFGroupQuestion;
            if (totalquestionadded < parseInt($scope.NewTestQuestion.NoOFQuestion)) {
                if ((totalquestionadded + question.NoOfQuestion) <= parseInt($scope.NewTestQuestion.NoOFQuestion)) {
                    question.Selected = true;
                    $scope.NoOFGroupQuestion = $scope.NoOFGroupQuestion + question.NoOfQuestion;
                }
                else {
                    ngNotify.set('selected question should not be more than No. of question.', 'error');
                }
            }
            else {
                ngNotify.set('Already ' + $scope.NewTestQuestion.NoOFQuestion + ' Question selected  for above test.', 'error');
            }
        }

        else {
            question.Selected = false;
            $scope.NoOFGroupQuestion = $scope.NoOFGroupQuestion - question.NoOfQuestion;
        }
    }
    $scope.GetNewRecord = function (nextorprev) {

        if ($scope.currentPage < 0) {
            $scope.disabledPrevious = true;
        }

        if (nextorprev == 1) {

            if (($scope.currentPage + 10) >= $($scope.GridUniqueItemList).length) {
                $scope.disabledNext = true;
            }
            else {

                $scope.currentPage = $scope.currentPage + 10;
            }

        }
        else {

            if ($scope.currentPage <= 0) {
                $scope.disabledPrevious = true;
            }
            else {
                $scope.currentPage = $scope.currentPage - 10;
            }

        }
        $scope.GetPageRequestData();

    }
    $scope.GetPageRequestData = function () {
        if ($scope.currentPage <= 0) {
            $scope.disabledPrevious = true;
        }
        if ($($scope.GridItemList).length != ($scope.currentPage)) {
            $scope.NewTestQuestion.UniqueQuestionList = $scope.GridUniqueItemList.slice($scope.currentPage, $scope.currentPage + 10);
        }
        else {
            $scope.NewTestQuestion.UniqueQuestionList = $scope.GridUniqueItemList;
        }
    }
    $scope.GetNewRecordGroup = function (nextorprev) {

        if ($scope.currentPageGroup < 0) {
            $scope.disabledPrevious = true;
        }

        if (nextorprev == 1) {

            if (($scope.currentPageGroup + 10) >= $($scope.GridItemGorupList).length) {
                $scope.disabledNext = true;
            }
            else {

                $scope.currentPageGroup = $scope.currentPageGroup + 10;
            }

        }
        else {

            if ($scope.currentPageGroup <= 0) {
                $scope.disabledPrevious = true;
            }
            else {
                $scope.currentPageGroup = $scope.currentPageGroup - 10;
            }

        }
        $scope.GetPageRequestDataGroup();

    }
    $scope.GetPageRequestDataGroup = function () {
        if ($scope.currentPageGroup <= 0) {
            $scope.disabledPrevious = true;
        }
        if ($($scope.GridItemList).length != ($scope.currentPage)) {
            $scope.NewTestQuestion.GroupQuestionList = $scope.GridItemGorupList.slice($scope.currentPageGroup, $scope.currentPageGroup + 10);
        }
        else {
            $scope.NewTestQuestion.GroupQuestionList = $scope.GridItemGorupList;
        }
    }
    $scope.saveTestQuestion = function () {
        var erromsg = ";"
        var isquestionselected = false;
        if ($scope.NewTestQuestion.TestId == "0") {
            erromsg = "Error";
        }
        if ($scope.NewTestQuestion.TestSectionId == "0") {
            erromsg = "Error";
        }
        if ($scope.NewTestQuestion.QuestionTypeId == "0") {
            erromsg = "Error";
        }
        if ($scope.NewTestQuestion.NoOFQuestion == undefined || $scope.NewTestQuestion.NoOFQuestion == "") {
            erromsg = "Error";
        }
        if (erromsg == "Error") {
            ngNotify.set("One or more mendatory field is not selected.", 'error');
            return false;
        }
        var uniquelist = new Object();
        var unique = [];
        var grouplist = new Object();
        var groupl = [];
        $($scope.NewTestQuestion.UniqueQuestionList).each(function (index, element) {

            if (element.Selected == true) {
                isquestionselected = true;
                unique.push({ Id: element.Id });
            }
        });
        uniquelist = unique;
        $($scope.NewTestQuestion.GroupQuestionList).each(function (index, element) {

            if (element.Selected == true) {
                isquestionselected = true;
                groupl.push({ Id: element.Id });
            }
        });

        if (isquestionselected == false) {
            ngNotify.set("Please select atleast one question.", 'error');
            return false;
        }

        grouplist = groupl;
        $http({
            url: '/TestQuestion/SaveTestQuestion',
            method: "POST",
            data: { usniids: uniquelist, groupid: grouplist, testid: $scope.NewTestQuestion.TestId, testsection: $scope.NewTestQuestion.TestSectionId }
        })
.success(function (data) {
    if (data == true) {
        ngNotify.set('Question added for selected Test.', 'success');
    }
},
function (response) { // optional
    // failed

});
    }

    $scope.callmethod = function ()
    {
        directiveName.GetMessageList($http, $scope);
    }
});