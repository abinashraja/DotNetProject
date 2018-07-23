var app = angular.module('EPortal', ['ngNotify', 'ngGrid']);
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
});
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
app.controller('EPortalCont', function ($scope, $http, ngNotify, fileUpload) {



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

        var uploadUrl = "/Organization/fileUpload";
        $scope.uploadFileToUrl(filedata, uploadUrl);

    }
    $scope.uploadFileToUrl = function (file, uploadUrl) {
        var fd = new FormData();
        fd.append('file', file);
        fd.append('code', $scope.NewOrganization.Code)
        $http.post(uploadUrl, fd, {
            transformRequest: angular.identity,
            file: file,
            headers: { 'Content-Type': undefined }
        })

        .success(function (data) {
            if (data.result == true) {
                $("#importModel").modal('hide');
                $scope.GetUserList();
                ngNotify.set("User Imported Successfully.", 'success');
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


    $scope.OrganizationList = [];
    $scope.searchText = "";
    $scope.NewOrganization = new Object();
    $scope.Operation = "Insert";



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

    $scope.SaveOrganization = function () {
        var erormsg = "";
        if ($scope.NewOrganization.Code == undefined || $scope.NewOrganization.Code == "") {
            erormsg += "Please enter code.";
        }
        if ($scope.NewOrganization.Name == undefined || $scope.NewOrganization.Name == "") {
            erormsg += "Please enter Name.";
        }
        if (erormsg == "") {
            if ($scope.NewOrganization.Operation == "Edit") {
                $scope.Operation = "Updated";
            }
            else {
                $scope.Operation = "Created";
            }
            var file = $scope.myFile;
            $http({
                url: '/Organization/SaveOrganization',
                method: "POST",
                data: $scope.NewOrganization
            })
            .success(function (data) {
                if (data.result == true) {
                    $scope.uploadFile();
                    $scope.getPagedDataAsync($scope.pagingOptions.pageSize, $scope.pagingOptions.currentPage, $scope.searchText);
                    $scope.NewOrganization = new Object();
                    $('#OrganizationModel').modal('hide');
                    ngNotify.set('Organization ' + $scope.Operation + ' successfully.', 'success');
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

            ngNotify.set(erormsg, 'error');

        }
    }

    $scope.EditOrganization = function (org) {
        $http({
            url: '/Organization/GetOrganizationInfo',
            method: "POST",
            data: org
        })
        .success(function (data) {
            $scope.NewOrganization = data;
            if ($scope.NewOrganization.ESTDate != null) {
                $scope.NewOrganization.ESTDate = new Date(parseInt($scope.NewOrganization.ESTDate.replace(/\D/g, "")));
                $scope.NewOrganization.ESTDate = $scope.ChangeDateFormat($scope.NewOrganization.ESTDate);
            }
            $('#OrganizationModel').modal('show');

        },
        function (response) { // optional
            // failed

        });
    }
    $scope.DeleteOrganization = function (org) {
        $http({
            url: '/Organization/DeleteOrganization',
            method: "POST",
            data: org
        })
        .success(function (data) {
            if (data == true) {
                $scope.getPagedDataAsync($scope.pagingOptions.pageSize, $scope.pagingOptions.currentPage, $scope.searchText);
                $scope.NewOrganization = new Object();
                ngNotify.set('Organization deleted successfully.', 'success');
            }
            else {

                ngNotify.set('Error ocured!please try again.', 'error');
            }

        },
        function (response) { // optional
            // failed

        });
    }
    $scope.MakeObjectEMpty = function () {
        $scope.NewOrganization = new Object();
        $scope.NewOrganization.Operation = "Create";
    }


    $scope.ChangeDateFormat = function (date) {
        var year = date.getFullYear();
        var month = (1 + date.getMonth()).toString();
        month = month.length > 1 ? month : '0' + month;
        var day = date.getDate().toString();
        day = day.length > 1 ? day : '0' + day;
        return day + '/' + month + '/' + year;
    }


    $scope.Assignprivilege = function () {
        if ($($scope.SelectedRowOrg).length > 0) {

            $http({
                url: '/Organization/GetprivilegeInfo',
                method: "POST",
                data: $scope.SelectedRowOrg
            })
        .success(function (data) {
            $scope.ModuleList = data;
            if ($(data).length > 0) {
                $scope.GetModulePage(data[0]);
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

    $scope.SelectUserOnlyoneRole = function (use,type) {

        if (type == 0) {
            use.Create = true;
        }
        else {
            use.Create = false;
        }
      
    }

    $scope.Saveprivilege = function () {

        $http({
            url: '/Organization/OrganizationSave',
            method: "POST",
            data: { moduleprevlist: $scope.ModuleList, orgid: $scope.SelectedRowOrg.Id }
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
    $scope.GetCLassName = function (module) {
        if ($scope.SelectedModule.Id == module.Id) {
            return "active";
        }
        else {
            return "";
        }
    }

    $scope.SelectedOrganization = function (org) {
        $scope.SelectedRowOrg = org;
    }
    $scope.checkdisabled = function () {
        if ($($scope.SelectedRowOrg).length > 0) {
            return false;
        }
        else
            return true;
    }

    //Grid Code Start 

    //now add the above object to your ngGrid
    $scope.totalServerItems = 0;
    $scope.pagingOptions = {
        pageSizes: ["250", "500", "1000"],
        pageSize: "250",
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
            if (searchText) {
                var ft = searchText.toLowerCase();
                //$http.post('Home/GetDataForGird?searchtext=' + ft).success(function (largeLoad) {

                //});
                $http({
                    method: 'POST',
                    url: '/Organization/GetOrganizationList',
                    data: { searchtext: ft }
                })
             .success(function (largeLoad) {
                 $scope.setPagingData(largeLoad, page, pageSize);
             });
            } else {
                $http.get("/Organization/GetOrganizationList").success(function (largeLoad) {

                    $scope.setPagingData(largeLoad, page, pageSize);

                });


            }
        }, 100);
    };
    $scope.callmethod = function () {
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
                                    { field: 'Code', displayName: 'Code' },
                                    { field: 'Name', cellClass: 'Name', headerCellClass: 'ageHeader' }
                                    //{ field: 'Operation', cellClass: 'Operation', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='ngSelectionCell btnclass'><span style='margin-right: 3%;'data-toggle='modal' data-target='.bs-example-modal-sm' ng-click='EditOrganization(row.entity)' class='label label-default'>Edit</span><span style='margin-right: 3%;' ng-click='DeleteOrganization(row.entity)' class='label label-default'>Delete</span></div>" }
        ],
        //showSelectionCheckbox: true,
        //selectWithCheckboxOnly:true,
        //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
        rowTemplate: '<div ng-dblclick="EditOrganization(row.entity)" ng-click="SelectedOrganization(row.entity)"  ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

    };


    //Grid Code End




});