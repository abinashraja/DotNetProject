var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
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
}]).controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {




    $scope.QuestionSourceList = [];
    $scope.GridItemList = [];
    $scope.searchText = "";
    $scope.NewQuestionSource = new Object();
    $scope.Operation = "Insert";
    $scope.currentPage = 0;
    $scope.disabledNext = false;
    $scope.disabledPrevious = false;
    $scope.SourceText = "";

    $scope.ckEditors = [];
    $scope.addEditor = function () {
        var rand = "" + (Math.random() * 10000);
        $scope.ckEditors.push({ OptionText: rand });
    }
    $scope.GetPageRequestData = function () {
        if ($scope.currentPage <= 0) {
            $scope.disabledPrevious = true;
        }
        if ($($scope.GridItemList).length != ($scope.currentPage)) {
            $scope.QuestionSourceList = $scope.GridItemList.slice($scope.currentPage, $scope.currentPage + 10);
        }
        else {
            $scope.QuestionSourceList = $scope.GridItemList;
        }
    }
    $scope.GetNewRecord = function (nextorprev) {

        if ($scope.currentPage < 0) {
            $scope.disabledPrevious = true;
        }

        if (nextorprev == 1) {

            if (($scope.currentPage + 10) >= $($scope.GridItemList).length) {
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

    $scope.SaveQuestionSource = function () {
        var erormsg = "";
        if ($scope.NewQuestionSource.SourceCode == undefined || $scope.NewQuestionSource.SourceCode == "") {
            erormsg = "Error";
        }
        if ($scope.NewQuestionSource.SourceName == undefined || $scope.NewQuestionSource.SourceName == "") {
            erormsg = "Error";
        }
        if ($scope.SourceText == "" || $scope.SourceText == "Enter your question description here.") {
            erormsg = "Error";
        }
        if ($scope.NewQuestionSource.QuestionTypeId == "0") {
            ngNotify.set('Please select Question Type.', 'error');
            return false;
        }
        if (erormsg == "") {
            jQuery.event.trigger("ajaxStart");
            if ($scope.NewQuestionSource.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/QuestionSource/SaveQuestionSource',
                method: "POST",
                data: { QuestionSourceInfo: $scope.NewQuestionSource, sourcetestinfo: $scope.SourceText }
            })
            .success(function (data) {
                if (data.result == true) {
                    $scope.callmethod();
                    $scope.NewQuestionSource = new Object();
                    $('#QuestionSourceModel').modal('hide');
                    ngNotify.set('QuestionSource ' + $scope.Operation + ' successfully.', 'success');
                }
                else {
                    if (data.errormsg != "") {
                        ngNotify.set(data.errormsg, 'error');
                    }
                    else {

                        ngNotify.set('Error ocured!please try again.', 'error');
                    }

                }
                jQuery.event.trigger("ajaxStop");

            },
            function (response) { // optional
                // failed

            });
        }
        else {
            if (erormsg == "Error") {
                ngNotify.set("One or more mendatory field is laft blank.", 'error');
            }
        }
    }
    $scope.EditQuestionSource = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/QuestionSource/GetQuestionSourceInfo',
            method: "POST",
            data: org
        })
        .success(function (data) {
            $('#QuestionSourceModel').modal('show');
            $scope.NewQuestionSource = data.sourcedata;
            $scope.SourceText = data.sourcedata.ResourceText;
            $scope.NewQuestionSource.QuestionTypeList = data.atypelist;
            jQuery.event.trigger("ajaxStop");
        },
        function (response) { // optional
            // failed

        });
    }
    $scope.DeleteQuestionSource = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/QuestionSource/DeleteQuestionSource',
            method: "POST",
            data: org
        })
        .success(function (data) {
            if (data.result == true) {
                $scope.callmethod();
                $scope.NewQuestionSource = new Object();
                ngNotify.set('QuestionSource deleted successfully.', 'success');
            }
            else {
                if (data.errormsg == "") {
                    ngNotify.set('Error ocured!please try again.', 'error');
                }
                else {
                    ngNotify.set(data.errormsg, 'error');
                }
            }
            jQuery.event.trigger("ajaxStop");
        },
        function (response) { // optional
            // failed

        });
    }
    $scope.MakeObjectEMpty = function () {
        $scope.NewQuestionSource = new Object();
        $scope.NewQuestionSource.IsApplicant = false;
        $scope.NewQuestionSource.QuestionTypeId = "0";
        $scope.NewQuestionSource.Operation = "Create";
        $scope.SourceText = "Enter your question description here.";
        $scope.addEditor();
        $scope.GetQuestionTypeList();
    }


    $scope.GetClassForRole = function (operation) {
        if (operation == "Create") {

            return "";
        }
        else {

            return "UpdateButton";
        }
    }

    $scope.GetQuestionTypeList = function () {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/QuestionSource/GetAllQuestionType',
            method: "POST"
        })
.success(function (data) {
    $scope.NewQuestionSource.QuestionTypeList = data;
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
                    url: '/QuestionSource/GetQuestionSourceList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/QuestionSource/GetQuestionSourceList").success(function (largeLoad) {

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

        columnDefs: [{ field: 'Id', displayName: 'Id', visible: false },
                                    { field: 'SourceCode', displayName: 'Code', cellClass: 'CodeClass' },
                                    { field: 'SourceName', displayName: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteQuestionSource(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditQuestionSource(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End






});