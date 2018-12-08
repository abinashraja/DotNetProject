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

    $scope.SubjectList = [];
    $scope.GridItemList = [];
    $scope.searchText = "";
    $scope.NewSubject = new Object();
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
    $scope.SaveSubject = function () {
        var erormsg = "";
        if ($scope.NewSubject.Code == undefined || $scope.NewSubject.Code == "") {
            erormsg = "Error";
        }
        if ($scope.NewSubject.Name == undefined || $scope.NewSubject.Name == "") {
            erormsg = "Error";
        }

        if (erormsg == "") {
            jQuery.event.trigger("ajaxStart");
            if ($scope.NewSubject.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/Subject/SaveSubject',
                method: "POST",
                data: { Subjectinfo: $scope.NewSubject }
            })
                .success(function (data) {
                    if (data.result == true) {
                        $scope.callmethod();
                        $scope.NewSubject = new Object();
                        $('#SubjectModel').modal('hide');
                        ngNotify.set('Subject ' + $scope.Operation + ' successfully.', 'success');
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
    $scope.EditSubject = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Subject/GetSubjectInfo',
            method: "POST",
            data: org
        })
            .success(function (data) {
                $scope.NewSubject = data.subjectdata;              
                $('#SubjectModel').modal('show');
                jQuery.event.trigger("ajaxStop");
            },
                function (response) { // optional
                    // failed

                });
    }
    $scope.DeleteSubject = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Subject/DeleteSubject',
            method: "POST",
            data: org
        })
            .success(function (data) {
                if (data.result == true) {
                    $scope.callmethod();
                    $scope.NewSubject = new Object();
                    ngNotify.set('Subject deleted successfully.', 'success');
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
    $scope.CourseList = [];
    $scope.MakeObjectEMpty = function () {
        $scope.NewSubject = new Object();
        $scope.NewSubject.IsApplicant = false;
        $scope.NewSubject.Operation = "Create";
        //$scope.GetCourseList = [];
        //$scope.GetCourseListMethod();


    }
    //$scope.GetCourseListMethod = function () {
    //    //jQuery.event.trigger("ajaxStart");
    //    $http({
    //        url: '/Subject/GetCourseList',
    //        method: "POST"
    //    })
    //        .success(function (data) {
    //            $scope.GetCourseList = data;
    //            //jQuery.event.trigger("ajaxStop");
    //        },
    //            function (response) { // optional
    //                // failed

    //            });

    //}



    $scope.GetSubjectForRole = function (operation) {
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
    $scope.SelectedSubjectFun = function (Subject) {
        $scope.SelectedSubject = Subject;
    }
    $scope.EnableDisable = function () {
        if ($($scope.SelectedSubject).length > 0) {
            return false;
        }
        else {
            return true;
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
                    url: '/Subject/GetSubjectList',
                    data: { searchtext: ft }
                })
                    .success(function (largeLoad) {
                        $scope.setPagingData(largeLoad, page, pageSize);
                    });
            } else {
                $http.get("/Subject/GetSubjectList").success(function (largeLoad) {
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
        { field: 'Code', displayName: 'Code', cellClass: 'CodeClass' },
        { field: 'Name', displayName: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
        //{ field: 'CourseName', displayName: 'Course Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
        { field: 'Operation', width: 100, cellClass: 'OperationSubject', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteSubject(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditSubject(row.entity)" ng-click="SelectedSubjectFun(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End







});