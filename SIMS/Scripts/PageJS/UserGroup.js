var app = angular.module('EPortal', ['ngNotify', 'ngGrid', 'EAssessmentModule']);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datepicker({ dateFormat: 'dd-mm-yy' });
            });
        }
    }
}).controller('EPortalCont', function ($scope, $http, ngNotify, directiveName) {


    //$scope.months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    $scope.TestSectionList = [];
    $scope.GridItemList = [];
    $scope.searchText = "";
    $scope.NewTestSection = new Object();
    $scope.Operation = "Insert";
    $scope.currentPage = 0;
    $scope.disabledNext = false;
    $scope.disabledPrevious = false;

    $scope.DestApplicantdataList = [];


    $scope.GetTestSectionList = function () {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/TestSection/GetTestSectionList',
            method: "POST"
        })
.success(function (data) {
    $scope.GridItemList = data;
    $scope.GetPageRequestData();
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

    $scope.EnableDisabledMaprole = function () {
        if ($($scope.SelectedUser).length > 0) {
            return false;
        }
        else {

            return true;
        }
    }
    $scope.SelectUserOnlyone = function (use) {
        use.Selected = true;
        $scope.SelectedUser = use;
    }
    $scope.AssignTest = function () {
        var selectedUser = $scope.SelectedUser;
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/UserGroup/GetTestList',
            method: "POST",
            data: { usergroup: selectedUser }

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
    $scope.SaveTest = function () {
        if ($($scope.SelectedTest).length > 0) {
            var selectedUserid = $scope.SelectedUser.Id;
            var selectedtest = $scope.SelectedTest.Id;

            $http({
                url: '/UserGroup/AssignTestUser',
                method: "POST",
                data: { groupid: selectedUserid, testid: selectedtest }

            })
    .success(function (data) {
        if (data == true) {
            $scope.callmethod();
            $('#roletestmodel').modal('hide');
            $scope.SelectedUser = [];
            ngNotify.set("Test Assign for selected User Group.", 'success');
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
    $scope.MakeObjectEMpty = function () {
        $scope.NewUserGroup.Operation = "Create";
        $scope.NewUserGroup.UserGroupCode = "";
        $scope.NewUserGroup.UserGroupName = "";
        $scope.DestApplicantdataList = [];
        $scope.GetSourceApplicant();
    }

    $scope.GetSourceApplicant = function () {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/UserGroup/GetSourceApplicantList',
            method: "POST"
        })
.success(function (data) {
    $scope.SourceApplicantDataList = data;
    jQuery.event.trigger("ajaxStop");
},
function (response) { // optional
    // failed

});
    }

    $scope.enableright = false;
    $scope.MoveRight = function () {
        if ($scope.sourceselectedItems[0] != undefined) {
            $scope.DestApplicantdataList.push($scope.sourceselectedItems[0]);
            $($scope.DestApplicantdataList).each(function (index, elemnt) {
                var index = $scope.SourceApplicantDataList.indexOf(elemnt);
                if (index != -1) {
                    $scope.SourceApplicantDataList.splice(index, 1);
                }
            });
            if ($($scope.SourceApplicantDataList).length == 0) {
                $scope.enableright = true;
            }
            if ($($scope.DestApplicantdataList).length > 0) {
                $scope.enableleft = false;
            }
        }

    }
    $scope.enableleft = false;
    $scope.MoveLeft = function () {
        if ($scope.DestSelectedItems[0] != undefined) {
            $scope.SourceApplicantDataList.push($scope.DestSelectedItems[0]);
            $($scope.SourceApplicantDataList).each(function (index, elemnt) {
                var index = $scope.DestApplicantdataList.indexOf(elemnt);
                if (index != -1) {
                    $scope.DestApplicantdataList.splice(index, 1);
                }
            });
            if ($($scope.DestApplicantdataList).length == 0) {
                $scope.enableleft = true;
            }
            if ($($scope.SourceApplicantDataList).length > 0) {
                $scope.enableright = false;
            }
        }

    }

    $scope.NewUserGroup = new Object();

    $scope.SaveUserGroup = function () {
        $scope.NewUserGroup;
        $scope.DestApplicantdataList;
        $scope.NewUserGroup.ApplicantList = $scope.DestApplicantdataList;
        var datavalidation = 0;
        if ($scope.NewUserGroup.UserGroupCode == undefined || $scope.NewUserGroup.UserGroupCode == "") {
            ngNotify.set("Please enter Code.", 'error');
            return false;
        }
        if ($scope.NewUserGroup.UserGroupName == undefined || $scope.NewUserGroup.UserGroupName == "") {
            ngNotify.set("Please enter Name.", 'error');
            return false;
        }
        if ($($scope.DestApplicantdataList).length == 0) {
            datavalidation = 1;
            ngNotify.set("Please select atleast one User.", 'error');
            return false;

        }

        if (datavalidation == 0) {
            jQuery.event.trigger("ajaxStart");
            if ($scope.NewUserGroup.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            $http({
                url: '/UserGroup/SaveUserGroup',
                method: "POST",
                data: { usergroup: $scope.NewUserGroup, applicantlist: $scope.DestApplicantdataList }
            })
    .success(function (data) {
        jQuery.event.trigger("ajaxStop");
        if (data == true) {
            $('#UsergroupModel').modal('hide');
            $scope.callmethod();
            ngNotify.set("User Group " + $scope.Operation + " successfully.", 'success');

        }
        else {
            ngNotify.set("Some Error Occured!Please try again.", 'error');
        }

    },
    function (response) { // optional
        // failed

    });
        }


    }

    $scope.EditUserGroup = function (selecteduser) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/UserGroup/EditUserGroup',
            method: "POST",
            data: { usergroupid: selecteduser.Id }
        })
.success(function (data) {
    jQuery.event.trigger("ajaxStop");
    selecteduser.Operation = "Edit";
    selecteduser.DeleteConformation = false,
    $scope.NewUserGroup = selecteduser;
    $scope.DestApplicantdataList = data.dest;
    $scope.SourceApplicantDataList = data.source;
    $('#UsergroupModel').modal('show');
},
function (response) { // optional
    // failed

});
    }

    $scope.DeleteUserGroup = function (selectedrow) {
        jQuery.event.trigger("ajaxStart");
        $http({
            url: '/UserGroup/DeleteUserGroup',
            method: "POST",
            data: { usergroupid: selectedrow.Id }
        })
.success(function (data) {
    if (data.result == true) {
        ngNotify.set("Record Deleted successfully.", 'success');
    }
    else {
        if (data.errormsg == "") {

            ngNotify.set("Error occured!please try again.", 'error');
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





    $scope.GetClassForRole = function (operation) {
        if (operation == "Create") {

            return "";
        }
        else {

            return "UpdateButton";
        }
    }
    $scope.ChangeDateFormat = function (date) {
        var year = date.getFullYear();
        var month = (1 + date.getMonth()).toString();
        month = month.length > 1 ? month : '0' + month;
        var day = date.getDate().toString();
        day = day.length > 1 ? day : '0' + day;
        return day + '/' + month + '/' + year;
    }
    $scope.SelctTest = function (test) {
        $($scope.AllTest).each(function (index, element) {
            element.Selected = false;
        });
        test.Selected = true;
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
                    url: '/UserGroup/GetUSerGroupList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/UserGroup/GetUSerGroupList").success(function (largeLoad) {

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
                                    { field: 'UserGroupCode', displayName: 'Code', cellClass: 'CodeClass' },
                                    { field: 'UserGroupName', displayName: 'Name', cellClass: 'NameClass', headerCellClass: 'ageHeader' },
                                    { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteUserGroup(row.entity)' class='label label-danger'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditUserGroup(row.entity)" ng-click="SelectUserOnlyone(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End








});