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



    $scope.TestInstructionList = [];
    $scope.GridItemList = [];
    $scope.searchText = "";
    $scope.NewTestInstruction = new Object();
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
    $scope.SaveTestInstruction = function () {
        var erormsg = "";
        if ($scope.NewTestInstruction.Code == undefined || $scope.NewTestInstruction.Code == "") {
            erormsg = "Error";
        }
        if ($scope.NewTestInstruction.Name == undefined || $scope.NewTestInstruction.Name == "") {
            erormsg = "Error";
        }
        if ($scope.NewTestInstruction.TestId == "0") {
            ngNotify.set('Please select Test.', 'error');
            return false;
        }
        if ($scope.SourceText == "" || $scope.SourceText == "Enter your Test Instruction here.") {
            erormsg = "Error";
        }

        if (erormsg == "") {
            jQuery.event.trigger("ajaxStart");
            if ($scope.NewTestInstruction.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/TestInstruction/SaveTestInstruction',
                method: "POST",
                data: { TestInstructionInfo: $scope.NewTestInstruction, sourcetestinfo: $scope.SourceText }
            })
            .success(function (data) {
                if (data.result == true) {
                    $scope.callmethod();
                    $scope.NewTestInstruction = new Object();
                    $('#TestInstructionModel').modal('hide');
                    ngNotify.set('Test Instruction ' + $scope.Operation + ' successfully.', 'success');
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
    $scope.EditTestInstruction = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/TestInstruction/GetTestInstructionInfo',
            method: "POST",
            data: org
        })
        .success(function (data) {
            $('#TestInstructionModel').modal('show');
            $scope.NewTestInstruction = data.sourcedata;
            $scope.SourceText = data.sourcedata.ResourceText;
            $scope.NewTestInstruction.TestList = data.atypelist;
            jQuery.event.trigger("ajaxStop");
        },
        function (response) { // optional
            // failed

        });
    }
    $scope.DeleteTestInstruction = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/TestInstruction/DeleteTestInstruction',
            method: "POST",
            data: org
        })
        .success(function (data) {
            if (data.result == true) {
                $scope.callmethod();
                $scope.NewTestInstruction = new Object();
                ngNotify.set('TestInstruction deleted successfully.', 'success');
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
        $scope.NewTestInstruction = new Object();
        $scope.NewTestInstruction.IsApplicant = false;
        $scope.NewTestInstruction.QuestionTypeId = 0;
        $scope.NewTestInstruction.Operation = "Create";
        $scope.SourceText = "Enter your Test Instruction here.";
        $scope.addEditor();
        $scope.GetQuestionTypeList();
    }
    //$scope.abortDeleteTestInstruction = function (org) {
    //    org.DeleteConformation = false;
    //}
    //$scope.DeleteTestInstructionshowCOnform = function (org) {
    //    if (org.IsTestPublish == true) {
    //        ngNotify.set('Operation conflict:Operation cannot be performed.', 'error');
    //    }
    //    else {
    //        org.DeleteConformation = true;
    //    }
    //}

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
            url: '/TestInstruction/GetTestList',
            method: "POST"
        })
.success(function (data) {
    $scope.NewTestInstruction.TestList = data;
    $scope.NewTestInstruction.TestId = "0";
    jQuery.event.trigger("ajaxStop");
},
function (response) { // optional
    // failed

});
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
                    url: '/TestInstruction/GetTestInstructionList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/TestInstruction/GetTestInstructionList").success(function (largeLoad) {

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
                                    { field: 'Code', displayName: 'Code', cellClass: 'CodeClass' },
                                    { field: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteTestInstruction(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditTestInstruction(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End








});