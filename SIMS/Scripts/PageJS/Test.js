var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datepicker({
                    dateFormat: 'dd-mm-yy',
                    changeMonth: true,
                    changeYear: true,
                });
            });
        }
    }
}).controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {

    $scope.TestList = [];
    $scope.GridItemList = [];
    $scope.searchText = "";
    $scope.NewTest = new Object();
    $scope.Operation = "Insert";
    $scope.currentPage = 0;
    $scope.disabledNext = false;
    $scope.disabledPrevious = false;

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
    $scope.SaveTest = function () {
        var erormsg = "";
        if ($scope.NewTest.TestCode == undefined || $scope.NewTest.TestCode == "") {
            erormsg = "Error";
        }
        if ($scope.NewTest.TestName == undefined || $scope.NewTest.TestName == "") {
            erormsg = "Error";
        }
        //if ($scope.NewTest.HourTime == undefined || $scope.NewTest.HourTime == "") {
        //    erormsg = "Error";
        //}
        if ($scope.NewTest.HourTime == undefined || $scope.NewTest.HourTime == "") {
            $scope.NewTest.HourTime = 0;

        }
        if ($scope.NewTest.MinTime == undefined || $scope.NewTest.MinTime == "") {
            if ($scope.NewTest.MinTime != 0) {
                erormsg = "Error";
            }
        }
        if (($scope.NewTest.PeriodTo != undefined || $scope.NewTest.PeriodTo != "") && ($scope.NewTest.PeriodFrom != undefined || $scope.NewTest.PeriodFrom != "")) {
            if ($scope.NewTest.PeriodTo < $scope.NewTest.PeriodFrom) {
                erormsg = "Error";
                ngNotify.set("To Date should not be less tham From Date.", 'error');
                return false;
            }
        }
        if ($scope.NewTest.HourTime != undefined && $scope.NewTest.HourTime.length > 1) {
            if ($scope.NewTest.HourTime != "") {
                erormsg = "Error";
                ngNotify.set("Only one char allow for Hour.", 'error');
                return false;
            }
        }
        if ($scope.NewTest.MinTime != undefined && $scope.NewTest.MinTime.length > 2) {
            if ($scope.NewTest.MinTime != "") {
                erormsg = "Error";
                ngNotify.set("Only two char allow for Min.", 'error');
                return false;
            }
        }
        else {
            if (parseInt($scope.NewTest.MinTime) > 60) {
                erormsg = "Error";
                ngNotify.set("Please enter valid min.", 'error');
                return false;
            }
        }
        if (erormsg == "") {
            jQuery.event.trigger("ajaxStart");
            if ($scope.NewTest.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/Test/SaveTest',
                method: "POST",
                data: $scope.NewTest
            })
            .success(function (data) {
                if (data.result == true) {
                    $scope.callmethod();
                    $scope.NewTest = new Object();
                    $('#TestModel').modal('hide');
                    ngNotify.set('Test ' + $scope.Operation + ' successfully.', 'success');
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
    $scope.EditTest = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Test/GetTestInfo',
            method: "POST",
            data: org
        })
        .success(function (data) {
            $scope.NewTest = data;
            $('#TestModel').modal('show');
            if ($scope.NewTest.PeriodFrom != null) {
                if ($scope.NewTest.PeriodFrom != null) {
                    $scope.NewTest.PeriodFrom = new Date(parseInt($scope.NewTest.PeriodFrom.replace(/\D/g, "")));
                    $scope.NewTest.PeriodFrom = $scope.ChangeDateFormat($scope.NewTest.PeriodFrom);
                }
                if ($scope.NewTest.PeriodTo != null) {
                    $scope.NewTest.PeriodTo = new Date(parseInt($scope.NewTest.PeriodTo.replace(/\D/g, "")));
                    $scope.NewTest.PeriodTo = $scope.ChangeDateFormat($scope.NewTest.PeriodTo);
                }
            }
            jQuery.event.trigger("ajaxStop");
        },
        function (response) { // optional
            // failed

        });
    }
    $scope.DeleteTest = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Test/DeleteTest',
            method: "POST",
            data: org
        })
        .success(function (data) {
            if (data.result == true) {
                $scope.callmethod();
                $scope.NewTest = new Object();
                ngNotify.set('Test deleted successfully.', 'success');
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
        $scope.NewTest = new Object();
        $scope.NewTest.IsApplicant = false;
        $scope.NewTest.Operation = "Create";
    }
    //$scope.abortDeleteTest = function (org) {
    //    org.DeleteConformation = false;
    //}
    //$scope.DeleteTestshowCOnform = function (org) {
    //    if (org.IsPublish == true) {
    //        ngNotify.set('Publish record can not be deleted.', 'error');
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
    $scope.ChangeDateFormat = function (date) {
        if (date != null) {
            var year = date.getFullYear();
            var month = (1 + date.getMonth()).toString();
            month = month.length > 1 ? month : '0' + month;
            var day = date.getDate().toString();
            day = day.length > 1 ? day : '0' + day;
            return day + '/' + month + '/' + year;
        }
        else {
            return "";
        }
    }
    $scope.SelectedTestFun = function (test) {
        $scope.SelectedTest = test;
    }
    $scope.EnableDisable = function () {
        if ($($scope.SelectedTest).length > 0) {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.PublishTest = function () {
        $http({
            url: '/Test/PublishTest',
            method: "POST",
            data: $scope.SelectedTest
        })
         .success(function (data) {
             if (data == true) {
                 $scope.callmethod();
                 $scope.NewTest = new Object();
                 ngNotify.set('Test Publish successfully.', 'success');
             }
             else {

                 ngNotify.set('Error ocured!please try again.', 'error');
             }
             jQuery.event.trigger("ajaxStop");
         },
         function (response) { // optional
             // failed

         });

    }
    $scope.LockTest = function () {
        if ($scope.SelectedTest.IsPublish == true) {
            $http({
                url: '/Test/LockTest',
                method: "POST",
                data: $scope.SelectedTest
            })
            .success(function (data) {
                if (data == true) {
                    $scope.callmethod();
                    $scope.NewTest = new Object();
                    ngNotify.set('Test Locked successfully.', 'success');
                }
                else {

                    ngNotify.set('Error ocured!please try again.', 'error');
                }
                jQuery.event.trigger("ajaxStop");
            },
            function (response) { // optional
                // failed

            });
        }
        else {
            ngNotify.set('Selected record not yet publish.', 'error');
        }
    }

    $scope.isNumber = function (evt) {
        evt = (evt) ? evt : window.event;
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            evt.preventDefault();
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
                    url: '/Test/GetTestList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/Test/GetTestList").success(function (largeLoad) {

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
                                    { field: 'TestCode', displayName: 'Code', cellClass: 'CodeClass' },
                                    { field: 'TestName', displayName: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'IsPublishTxt', displayName: 'Publish', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'IslockedTxt', displayName: 'Lock', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteTest(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditTest(row.entity)" ng-click="SelectedTestFun(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End







});