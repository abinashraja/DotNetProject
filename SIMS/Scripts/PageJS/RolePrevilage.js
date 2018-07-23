var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {

    $scope.TestSearch = "";
    $scope.searchtextRole = "";
    $scope.GridItemList = [];
    $scope.UserList = [];
    $scope.searchtext = "";
    $scope.searchtextTest = "";
    $scope.currentPage = 0;
    $scope.SelectedUser = [];
    $scope.SelectedRole = [];
    $scope.SelectUserType = "40";
    
       $scope.Changepassword = function ()
    {
        if ($scope.ChangePassword == undefined)
        {
            ngNotify.set('Please enter detsils.', 'error');
            return false;
        }
        if ($scope.ChangePassword.oldpassword == undefined || $scope.ChangePassword.oldpassword == "") {
            ngNotify.set('Please enter Current password', 'error');
            return false;
        }
        if ($scope.ChangePassword.newpassword == undefined || $scope.ChangePassword.newpassword == "")
        {
            ngNotify.set('Please enter New Password.', 'error');
            return false;
        }
        if ($scope.ChangePassword.oldpassword == $scope.ChangePassword.newpassword)
        {
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

    $scope.SandTestQA = function ()
    {
        if ($scope.SelectedUser.Email == null || $scope.SelectedUser.Email == "")
        {
            ngNotify.set("User don't have valid email id.", 'error');
            return false;
        }
        var selectedUser = $scope.SelectedUser;
        //jQuery.event.trigger("ajaxStart");
        $http({
            url: '/RollPrivilege/SendMailQA',
            method: "POST",
            data: selectedUser

        })
.success(function (data) {
    if (data.sendmail == true) {
        ngNotify.set("Mail send successfully.", 'success');
    }
    else {
        if (data.errmsg != "") {
            ngNotify.set(data.errmsg, 'error');
        }
        else {
            ngNotify.set("Some error occured!please try again.", 'error');
        }
    }
},
function (response) { // optional
    // failed

});

    }
    $scope.SelectUserOnlyone = function (use) {
        use.Selected = true;
        $scope.SelectedUser = use;     
    }

    $scope.SelectUserOnlyoneRole = function (use, type) {

        if ($scope.SelectedUser.ApplicantOrUser =="Applicant")
        {
            return false;
        }
        if ($scope.SelectedRole.Code == "Applicant" && use.Code == "admin") {
            use.Selected = false;
        }
        else {

            $($scope.RoleList).each(function (index, element) {
                element.Selected = false;
            });
            if (type == 0) {

                $scope.SelectedRole = use;
                use.Selected = true;
            }
            else {

                use.Selected = false;

            }
        }
    }
    $scope.SelectUserOnlyoneTest = function (use, type) {
        $($scope.TestList).each(function (index, element) {
            element.Selected = false;
        });
        if (type == 0) {
            $scope.SelectedTest = use;
            use.Selected = true;
        }
        else {
            $scope.SelectedTest = [];
            use.Selected = false;

        }
    }
    $scope.GetRoleList = function () {
        var selectedUser = $scope.SelectedUser;
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/RollPrivilege/GetRoleList',
            method: "POST",
            data: selectedUser

        })
.success(function (data) {
    $scope.RoleList = data;
    $($scope.RoleList).each(function (index, element) {

        if (element.Selected == true) {
            $scope.SelectedRole = element;
        }
    });
    jQuery.event.trigger("ajaxStop");
},
function (response) { // optional
    // failed

});


    }
    $scope.AssignTest = function () {
        var selectedUser = $scope.SelectedUser;
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/RollPrivilege/GetTestList',
            method: "POST",
            data: { user: selectedUser }

        })
.success(function (data) {
    $scope.TestList = data;
    $($scope.TestList).each(function (index, element) {
        if (element.Selected == true) {
            $scope.SelectedTest = element;
        }
    });
    jQuery.event.trigger("ajaxStop");
},
function (response) { // optional
    // failed

});


    }
    $scope.EnableDisabledMaprole = function () {
        if ($($scope.SelectedUser).length > 0) {
            return false;
        }
        else {

            return true;
        }
    }
    $scope.SaveRolemap = function () {
        if ($($scope.SelectedUser).length > 0) {

            var selectedUser = $scope.SelectedUser.Id;
            var selectedrole = $scope.SelectedRole.Id == undefined ? "" : $scope.SelectedRole.Id;
            $http({
                url: '/RollPrivilege/SaveSelectedRole?selectedUser=' + selectedUser + "&selectedrole=" + selectedrole,
                method: "POST"
            })
    .success(function (data) {
        if (data == true) {
            $('#rolemodel').modal('hide');
            $scope.SelectedUser = [];
            ngNotify.set("User role mapped successfully.", 'success');
        }
        else {
            ngNotify.set("Error Occured!please try again", 'error');
        }
    },
    function (response) { // optional

    });

        }
        else {

            var errormsg = "";
            if ($($scope.SelectedUser).length == 0) {
                errormsg = "Please select atleast one User.";
            }
            if ($($scope.SelectedRole).length == 0) {
                errormsg += "Please select atleast one Role.";
            }
        }
    }
    $scope.ActivateUser = function () {
        var selectedUserid = $scope.SelectedUser.Id;


        $http({
            url: '/RollPrivilege/ActivateUser?Userid=' + selectedUserid,
            method: "POST"
        })
.success(function (data) {
    if (data.result == true) {

        $scope.callmethod();
        $scope.SelectedUser = [];
        ngNotify.set("User activated successfully.", 'success');
    }
    else {
        if (data.msg == "") {
            ngNotify.set("Error Occured!please try again.", 'error');
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

    $scope.SaveTest = function () {

        if ($($scope.SelectedTest).length > 0) {
            var selectedUserid = $scope.SelectedUser.Id;
            var selectedtest = $scope.SelectedTest.Id;

            $http({
                url: '/RollPrivilege/AssignTestUser',
                method: "POST",
                data: { userid: selectedUserid, testid: selectedtest }

            })
    .success(function (data) {
        if (data == true) {
            $scope.callmethod();
            $('#roletestmodel').modal('hide');
            $scope.SelectedUser = [];
            ngNotify.set("Test Assign for selected user successfully.", 'success');
        }
        else {
            ngNotify.set("Error Occured!please try again.", 'error');
        }
    },
    function (response) { // optional
        // failed

    });
        }
        else {

            ngNotify.set("Please select test.", 'error');
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
                //$http.post('Home/GetDataForGird?searchtext=' + ft).success(function (largeLoad) {

                //});
                $http({
                    method: 'POST',
                    url: '/RollPrivilege/GetUserTypeUserList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/RollPrivilege/GetUserTypeUserList").success(function (largeLoad) {

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
                                    { field: 'LoginExist', displayName: 'Login Exist', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'ApplicantOrUser', displayName: 'User Type', cellClass: 'NameClass', headerCellClass: 'ageHeader' }],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-click="SelectUserOnlyone(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End

});