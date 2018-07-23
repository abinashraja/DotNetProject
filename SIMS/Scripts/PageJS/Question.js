var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);
app.service('fileUpload', ['$http', function ($http, ngNotify) {

}]);
app.directive('ckEditor', [function () {
    return {
        require: '?ngModel',
        link: function ($scope, elm, attr, ngModel) {

            var ck = CKEDITOR.replace(elm[0]);

            ck.on('pasteState', function () {
                $scope.$apply(function () {
                    ngModel.$setViewValue(ck.getData());
                });
            });

            ngModel.$render = function (value) {
                ck.setData(ngModel.$modelValue);
            };
        }
    };
}]).controller('EPortalCont', function ($scope, $http, ngNotify, fileUpload, directiveName) {



    $scope.uploadFileToUrl = function (file, uploadUrl) {
        var fd = new FormData();
        fd.append('file', file);
        fd.append('questiontypeid', $scope.QuestionTypeId);

        $http.post(uploadUrl, fd, {
            transformRequest: angular.identity,
            file: file,
            headers: { 'Content-Type': undefined }
        })

        .success(function (data) {
            if (data.result == true) {
                $("#importModel").modal('hide');
                ngNotify.set("Question Imported Successfully.", 'success');
                jQuery.event.trigger("ajaxStop");

            }
            else {

                if (data.errormsg == "") {
                    ngNotify.set("error occured!please try again.", 'success');
                    jQuery.event.trigger("ajaxStop");

                }
                else {
                    ngNotify.set(data.errormsg, 'error');
                    jQuery.event.trigger("ajaxStop");
                }
            }
        })

        .error(function () {
            return false;
        });
    }

    $scope.uploadFile = function () {

        jQuery.event.trigger("ajaxStart");
        if ($scope.myFile == undefined) {
            ngNotify.set("Please select the file.", 'error');
            jQuery.event.trigger("ajaxStop");
            return false;
        }
        var filedata = $scope.myFile;

        console.log('file is ');
        console.dir(filedata);

        var uploadUrl = "/Question/fileUpload";
        $scope.uploadFileToUrl(filedata, uploadUrl);

    }

    $scope.QuestionTypeList = [];
    $scope.QuestionTypeId = "";
    $scope.ckEditors = [];
    $scope.searchText = "";
    $scope.QuestionInfo = "Enter your Question Here ";
    $scope.addEditor = function () {
        var rand = "" + (Math.random() * 10000);
        $scope.ckEditors.push({ OptionText: "" });
    }


    $http({
        url: '/Question/GetAllQuestionType',
        method: "POST"
    })
.success(function (data) {
    $scope.QuestionTypeList = data.qtype;  
    $scope.QuestionSourceList = data.sourlist;
    $scope.QuestionTypeId = "0";
    $scope.QuestionSourceId = "0";
    $scope.currentPage = 0;
    $scope.disabledthisone = false;
    $scope.QuestionId = "";
    $scope.OperationQuestion = "Create";
    $scope.ShowInsertQuestion();
},
function (response) { // optional
    // failed

});

    $scope.GetQuestionTypeResource = function ()
    {
        var questiontypeid = $scope.QuestionTypeId;
        $http({
            url: '/Question/GetSource?questiontypeid' + $scope.QuestionTypeId,
            method: "POST",
            data: { questiontypeid: $scope.QuestionTypeId }
        })
.success(function (data) {
    $scope.QuestionSourceList = data;
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
    $scope.SaveQuestion = function () {          
        
        if ($scope.OperationQuestion == "Create")
        {
            $scope.QuestionId = "";
        }
        if ($scope.QuestionTypeId == "0")
        {
            ngNotify.set('Please select Question Type.', 'error');
            return false;
        }
        if ($scope.QuestionInfo == "Enter your Question Here" || $scope.QuestionInfo == "") {
            ngNotify.set('Please enter the Question.', 'error');
            return false;
        }
        if ($scope.QuestionMark == undefined || $scope.QuestionMark == "")
        {
            ngNotify.set('Please enter Question Marks.', 'error');
            return false;
        }
        if ($($scope.ckEditors).length == 0)
        {
            ngNotify.set('Please add atleast one Option.', 'error');
            return false;
        }
        var isansselec = false;
        var optionempty = false;
        var count = 0;
        $($scope.ckEditors).each(function (index, element) {
            if (element.OptionText == "") {
                optionempty = true;
            }
            if (element.Selected == true) {
                isansselec = true;
            }
            if ($scope.HaveMultipleAnswer)
            {
                if (element.Selected == true)
                {
                    count = count + 1;
                }
            }
        });
        if (optionempty == true) {
            ngNotify.set('One or more Option is empty.', 'error');
            return false;
        }
        if (isansselec == false)
        {
            ngNotify.set('Please select answer for Question.', 'error');
            return false;
        }
        if (count < 2 && count!=0)
        {
            ngNotify.set('Please select Multiple answer for entered Question. ', 'error');
            return false;
        }
       
        var questionobj = new Object();
        questionobj.QuestionInfo = $scope.QuestionInfo;
        questionobj.QuestionTyeId = $scope.QuestionTypeId;
        questionobj.QuestionSourceId = $scope.QuestionSourceId;
        questionobj.QuestionMark = $scope.QuestionMark;
        questionobj.QuestionId = $scope.QuestionId;
        if ($scope.HaveMultipleAnswer == undefined ) {
            questionobj.HaveMultiAns = false;
        }
        else {
            questionobj.HaveMultiAns = $scope.HaveMultipleAnswer;
        }        
        $http({
            method: 'POST',
            url: '/Question/SaveQuestion',
            dataType: 'json',
            data: { optionlist: $scope.ckEditors, question: questionobj},
            //headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).success(function (data) {
            if (data == true) {
                $scope.ShowViewQuestion();
                ngNotify.set('Question save successfully.', 'success');

            }
            else {
                ngNotify.set('Error ocured!please try again.', 'error');
            }
            
        }).error(function (data) {
            alert(data);
        });


       

    }
    $scope.ShowInsertQuestion = function ()
    {        
        $scope.OperationQuestion = "Create";
        $("#QuestionInsert").show();
        $("#Questionview").hide();
        $scope.ckEditors = [];
        $scope.QuestionInfo = "";
        $scope.QuestionSourceList = [];
        $scope.QuestionTypeId = "0";
        $scope.QuestionSourceId = "0";       
        $scope.disabledthisone = false;
    }
    $scope.ShowViewQuestion = function ()
    {
        $("#QuestionInsert").hide();
        $("#Questionview").show();
        $scope.callmethod();
        //$http({
        //    method: 'POST',
        //    url: '/Question/GetAllQuestion',
        //    dataType: 'json'  
        //}).success(function (data) {
        //    $(data).each(function (index,element) {
        //        element.QuestionText = $scope.htmlToPlaintext(element.QuestionText);
        //    });            
        //    $scope.GridItemList = data;
            
        //}).error(function (data) {
            
        //});


    }
    $scope.abortDeletequestion = function (question) {
        question.DeleteConformation = false;
    }
    $scope.DeletequestionshowCOnform = function (question) {
        question.DeleteConformation = true;
    }
    $scope.DeleteQuestion = function (question)
    {        
        $http({
            method: 'POST',
            url: '/Question/DeleteQuestion',
            data: question

        }).success(function (data) {
            if (data.result == true) {
                $scope.ShowViewQuestion();             
                ngNotify.set('Question deleted successfully.', 'success');
            }
            else {
                if (data.errormsg == "") {
                    ngNotify.set('Error ocured!please try again.', 'error');
                }
                else {
                    ngNotify.set(data.errormsg, 'error');
                }
            }
           
        }).error(function (data) {

        });

    }
    $scope.Editquestion = function (question)
    {
        var quesid=question.Id;
        $http({
            method: 'POST',
            url: '/Question/GetQuestionDetail?quesid=' + quesid          
        }).success(function (data) {           
            $scope.ckEditors = data.optionlist;
            $scope.QuestionInfo = data.questiontxt;
            $scope.QuestionSourceList = data.sourcelist;
            $scope.QuestionSourceId = data.sourceid;
            $scope.QuestionTypeId = data.questiontype;
            $scope.disabledthisone = true;
            $scope.QuestionMark = data.Questionmarks;
            $("#QuestionInsert").show();
            $("#Questionview").hide();
            $scope.OperationQuestion = "Update";
            $scope.QuestionId = quesid;
        }).error(function (data) {

        });
    }
    $scope.htmlToPlaintext=function(text) {
        return (text ? String(text).replace(/<[^>]+>/gm, '') : '').replace(/&nbsp;/g, '').replace(/\<br\s*[\/]?>/gi, '');;
    }
    

    //Grid Code Start 

    //now add the above object to your ngGrid
    $scope.totalServerItems = 0;
    $scope.pagingOptions = {
        pageSizes: ["12", "25", "50", "100", "250", "500", "1000"],
        pageSize: "12",
        currentPage: 1
    };
    $scope.searchText = "";
    $scope.filterOptions = {
        filterText: "",
        useExternalFilter: true
    };
    $scope.setPagingData = function (data, page, pageSize) {
        var pagedData = data.slice((page - 1) * pageSize, page * pageSize);
        $scope.myData = pagedData;
        $scope.totalServerItems = data.length;
        if (!$scope.$$phase) {
            $scope.$apply();
        }
    };
    $scope.searchText = "";
    $scope.getPagedDataAsync = function (pageSize, page, searchText) {
        setTimeout(function () {
            var data;
            if ($scope.searchText != null && $scope.searchText != "") {
                $scope.pagingOptions.currentPage = 1;
                var ft = $scope.searchText.toLowerCase();
                $http({
                    method: 'POST',
                    url: '/Question/GetAllQuestion',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $(largeLoad).each(function (index, element) {
                     element.QuestionText = $scope.htmlToPlaintext(element.QuestionText);
                 });
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/Question/GetAllQuestion").success(function (largeLoad) {
                    $(largeLoad).each(function (index, element) {
                        element.QuestionText = $scope.htmlToPlaintext(element.QuestionText);
                    });
                    $scope.setPagingData(largeLoad, page, pageSize);

                });


            }
        }, 100);
    };
    $scope.callmethod = function () {
        directiveName.GetMessageList($http, $scope);
        $scope.getPagedDataAsync($scope.pagingOptions.pageSize, $scope.pagingOptions.currentPage, $scope.searchText);
    }

    $scope.$watch('pagingOptions', function (newVal, oldVal) {
        if (newVal !== oldVal && newVal.currentPage !== oldVal.currentPage) {
            $scope.getPagedDataAsync($scope.pagingOptions.pageSize, $scope.pagingOptions.currentPage, $scope.filterOptions.filterText);
        }
    }, true);
    $scope.$watch('filterOptions', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.getPagedDataAsync($scope.pagingOptions.pageSize, $scope.pagingOptions.currentPage, $scope.filterOptions.filterText);
        }
    }, true);

    $scope.SelectedRowData = function (row) {
        alert(row.entity.Name);
    }
    //$scope.gridOptions.ngGrid.config.selectedItems
    $scope.myGrid = {
        data: 'myData', enablePaging: true,
        totalServerItems: 'totalServerItems',
        pagingOptions: $scope.pagingOptions,
        showFooter: true,
        multiSelect: false,
        showFilter: true,
        showSelectionCheckbox: true,
        columnDefs: [{ field: 'Id', displayName: 'Id', visible: false },
                                    { field: 'QuestionText', displayName: 'Question', cellClass: 'CodeClass' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteQuestion(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="Editquestion(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End


});