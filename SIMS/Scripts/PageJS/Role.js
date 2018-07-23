var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {


    //$scope.GetRoleList();


    $scope.NoChanges = false;
    $scope.RoleList = [];
    $scope.searchText = "";
    $scope.NewRole = new Object();
    $scope.Operation = "Insert";
    $scope.SelectedRowROle = [];
    $scope.ModuleList = [];
    $scope.ModulePageList = [];





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

    $scope.SaveRole = function () {

        var erormsg = "";
        if ($scope.NewRole.Code == undefined || $scope.NewRole.Code == "") {
            erormsg = "Error";
        }
        if ($scope.NewRole.Name == undefined || $scope.NewRole.Name == "") {
            erormsg = "Error";
        }

        if (erormsg == "") {
            if ($scope.NewRole.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/Role/SaveRole',
                method: "POST",
                data: $scope.NewRole
            })
            .success(function (data) {
                if (data.result == true) {
                    $scope.callmethod();
                    $scope.NewRole = new Object();
                    $('#RoleModel').modal('hide');
                    ngNotify.set('Role ' + $scope.Operation + ' successfully.', 'success');
                }
                else {

                    if (data.errormsg != "") {
                        ngNotify.set(data.errormsg, 'error');
                    }
                    else {

                        ngNotify.set('Error ocured!please try again.', 'error');
                    }
                }

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
    $scope.EditRole = function (org) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/Role/GetRoleInfo',
            method: "POST",
            data: org
        })
        .success(function (data) {
            $scope.NewRole = data;
            $('#RoleModel').modal('show');
            jQuery.event.trigger("ajaxStop");
        },
        function (response) { // optional
            // failed

        });
    }
    $scope.DeleteRole = function (org) {
        $http({
            url: '/Role/DeleteRole',
            method: "POST",
            data: org
        })
        .success(function (data) {
            if (data.result == true) {
                $scope.callmethod();
                $scope.NewRole = new Object();
                ngNotify.set('Role deleted successfully.', 'success');
            }
            else {

                if (data.errormsg == "") {
                    ngNotify.set('Error ocured!please try again.', 'error');
                }
                else {
                    ngNotify.set(data.errormsg, 'error');
                }
            }

        },
        function (response) { // optional
            // failed

        });
    }
    $scope.MakeObjectEMpty = function () {
        $scope.NewRole = new Object();
        $scope.NewRole.Operation = "Create";
    }


    $scope.SelectedRow = function (role) {
        $scope.SelectedRowROle = role;

        $($scope.RoleList).each(function (index, elemnt) {
            $("#" + elemnt.Id).removeClass("info");
        });
        $("#" + role.Id).addClass("info");
    }
    $scope.Assignprivilege = function () {
        if ($($scope.SelectedRowROle).length > 0) {

            $http({
                url: '/Role/GetprivilegeInfo',
                method: "POST",
                data: $scope.SelectedRowROle
            })
        .success(function (data) {
            $scope.ModuleList = data;
            if ($(data).length > 0) {
                if ($scope.SelectedRowROle.Name == "Applicant") {
                    $scope.GetModulePage(data[1]);
                }
                else {

                    $scope.GetModulePage(data[0]);
                }


            }
        },
        function (response) { // optional
            // failed

        });
        }
        else {
            ngNotify.set('Please select role.', 'error');

        }
    }
    $scope.checkdisabled = function () {
        if ($($scope.SelectedRowROle).length == 0) {
            return true;
        }
        else {
            return false;
        }
    }
    $scope.SelectedModule = [];
    $scope.GetModulePage = function (module) {
        $scope.SelectedModule = module;
        $($scope.ModuleList).each(function (index, element) {
            $("#" + element.Id).removeClass("active");
            $("#" + element.Code).removeClass("active");
        });
        $("#" + module.Id).addClass("active");
        $("#" + module.Code).addClass("active");
        $scope.GetCLassName(module);

    }
    $scope.getclass = function (index) {
        if (index == 0) {
            return "active";
        }
        else {
            return "";
        }
    }
    $scope.GetCLassName = function (module) {
        if ($scope.SelectedModule.Id == module.Id) {
            return "active";
        }
        else {
            return "";
        }
    }
    $scope.ItisClicked = function (whatclicked) {
        if (whatclicked == true) {
            $scope.NoChanges = true;

        }
        else {

            $scope.NoChanges = false;
        }
    }
    $scope.Saveprivilege = function () {
        $http({
            url: '/Role/SavePrivileges',
            method: "POST",
            data: { moduleprevlist: $scope.ModuleList, roleid: $scope.SelectedRowROle.Id }
        })
        .success(function (data) {
            if (data == true) {
                $('#PrevileageModel').modal('hide');
                ngNotify.set('Privileges added successfully,Will effect in next login. ', 'success');

            }
            else {
                ngNotify.set('Error occure!please try again.', 'error');
            }
        },
        function (response) { // optional
            // failed
        });



    }
    $scope.selectModuleforuser = function (module) {
        if ($scope.SelectedRowROle.Name == "admin") {
            if (module.Name == "OrganizationSetup") {
                return true;
            }
            else {

                return false;
            }
        }
        if ($scope.SelectedRowROle.Name == "Applicant") {
            if (module.Name == "UserOperation") {
                return true;
            }
            else {

                return false;
            }
        }
        return true;

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
                //$http.post('Home/GetDataForGird?searchtext=' + ft).success(function (largeLoad) {

                //});
                $http({
                    method: 'POST',
                    url: '/Role/GetRoleList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/Role/GetRoleList").success(function (largeLoad) {
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
                                    { field: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteRole(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-click="SelectedRow(row.entity)" ng-dblclick="EditRole(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End







});