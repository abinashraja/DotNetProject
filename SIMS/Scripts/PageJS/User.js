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

//app.controller('myCtrl', ['$scope', 'fileUpload', function ($scope, fileUpload) {
//    $scope.uploadFile = function () {
//        var file = $scope.myFile;

//        console.log('file is ');
//        console.dir(file);

//        var uploadUrl = "/User/fileUpload";
//        fileUpload.uploadFileToUrl(file, uploadUrl);
//    };
//}]);
app.directive('datepicker', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModelCtrl) {
            $(function () {
                element.datepicker({
                    changeMonth: true,
                    changeYear: true,
                    maxDate: "0D"
                });
            });
        }
    }
})
 .controller('EPortalCont', function ($scope, $http, ngNotify, fileUpload, directiveName) {

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
     $scope.uploadFileToUrl = function (file, uploadUrl) {
         var fd = new FormData();
         fd.append('file', file);

         $http.post(uploadUrl, fd, {
             transformRequest: angular.identity,
             file: file,
             headers: { 'Content-Type': undefined }
         })

         .success(function (data) {
             if (data.result == true) {
                 $("#importModel").modal('hide');
                 $scope.callmethod();
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

         var uploadUrl = "/User/fileUpload";
         $scope.uploadFileToUrl(filedata, uploadUrl);

     }

     $scope.uploadFileImage = function (userid) {

         jQuery.event.trigger("ajaxStart");
         if ($scope.myFile == undefined) {
             ngNotify.set("Please select the file.", 'error');
             jQuery.event.trigger("ajaxStop");
             return false;
         }
         var filedata = $scope.myFile;

         console.log('file is ');
         console.dir(filedata);

         var uploadUrl = "/User/fileUploadImage";
         $scope.uploadFileToUrlImage(filedata, uploadUrl, userid);

     }
     $scope.uploadFileToUrlImage = function (file, uploadUrl, userid) {
         var fd = new FormData();
         fd.append('file', file);
         fd.append('userid', userid)
         $http.post(uploadUrl, fd, {
             transformRequest: angular.identity,
             file: file,
             headers: { 'Content-Type': undefined }
         })

         .success(function (data) {
             if (data.result == true) {
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

     $scope.NewUser = new Object();
     $scope.Operation = "Insert";



     $scope.SaveUser = function () {
         var erormsg = "";
         if ($scope.NewUser.Code == undefined || $scope.NewUser.Code == "") {
             erormsg = "Error";
         }
         if ($scope.NewUser.Name == undefined || $scope.NewUser.Name == "") {
             erormsg = "Error";
         }
         if ($scope.NewUser.LogInId == undefined || $scope.NewUser.LogInId == "") {
             erormsg = "Error";
         }
         //if ($scope.NewUser.Email == undefined || $scope.NewUser.Email == "") {
         //    erormsg = "Error";
         //}
         if ($scope.NewUser.DateOfBirth == undefined || $scope.NewUser.DateOfBirth == "") {
             erormsg = "Error";
         }
         if ($scope.NewUser.MobileNo != undefined && $scope.NewUser.MobileNo.length != 10) {
             if ($scope.NewUser.MobileNo != "") {
                 erormsg = "Error";
                 ngNotify.set("Please enter valid Mobile No.", 'error');
                 return false;
             }
         }
         if ($scope.NewUser.Email != undefined && !($scope.validateEmail($scope.NewUser.Email))) {
             if ($scope.NewUser.Email != "") {
                 erormsg = "Error";
                 ngNotify.set("Please enter valid Email.", 'error');
                 return false;
             }
         }
         if (erormsg == "") {
             jQuery.event.trigger("ajaxStart");
             if ($scope.NewUser.Operation == "Edit") {
                 $scope.Operation = "Updated";
             }
             else {
                 $scope.Operation = "Created";
             }
             $http({
                 url: '/User/SaveUser',
                 method: "POST",
                 data: { UserInfo: $scope.NewUser }
             })
             .success(function (data) {
                 jQuery.event.trigger("ajaxStop");
                 if (data.result == true) {
                     $scope.uploadFileImage(data.id);
                     $scope.callmethod();
                     $scope.NewUser = new Object();
                     $('#UserModel').modal('hide');                     
                     ngNotify.set('User ' + $scope.Operation + ' successfully.', 'success');
                 }
                 else {
                     if (data.errormsg != "") {
                         ngNotify.set(data.errormsg, 'error');
                     }
                     else {

                         ngNotify.set('Error ocured!please try again.', 'error');
                     }

                 }


             }).error(function (xhr, status, error) {
                 console.log(xhr);
                 if (xhr == 'undefined' || xhr == undefined) {
                     alert('undefined');
                 } else {
                     alert('object is there');
                 }
                 alert(status);
                 alert(error);
             },
             function (response) { // optional
                 // failed
                 alert(response);

             });
         }
         else {
             if (erormsg == "Error") {
                 ngNotify.set("One or more mendatory field is laft blank.", 'error');
             }

         }
     }
     $scope.EditUser = function (org) {
       
         jQuery.event.trigger("ajaxStart");
         $http({
             url: '/User/GetUserInfo',
             method: "POST",
             data: org
         })
         .success(function (data) {
             $scope.NewUser = data;
             $('#UserModel').modal('show');
             $scope.NewUser.DateOfBirth = new Date(parseInt($scope.NewUser.DateOfBirth.replace(/\D/g, "")));
             $scope.NewUser.DateOfBirth = $scope.ChangeDateFormat($scope.NewUser.DateOfBirth);
             jQuery.event.trigger("ajaxStop");
         },
         function (response) { // optional
             // failed

         });
     }
     $scope.DeleteUser = function (org) {
         jQuery.event.trigger("ajaxStart");
         $http({
             url: '/User/DeleteUser',
             method: "POST",
             data: org
         })
         .success(function (data) {
             if (data.result == true) {
                 $scope.NewUser = new Object();
                 ngNotify.set('User deleted successfully.', 'success');
                 $scope.callmethod();
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
     $scope.ShowGird = true;    
     $scope.MakeObjectEMpty = function () {
         $scope.NewUser = new Object();
         $scope.NewUser.IsApplicant = false;
         $scope.NewUser.Operation = "Create";
         $scope.ShowGird = false;
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
     $scope.isNumber = function (evt) {
         evt = (evt) ? evt : window.event;
         var charCode = (evt.which) ? evt.which : evt.keyCode;
         if (charCode > 31 && (charCode < 48 || charCode > 57)) {
             evt.preventDefault();
         }
     }
     $scope.validateEmail = function (email) {
         var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
         return re.test(email);
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
                     url: '/User/GetUserList',
                     data: { searchtext: ft }
                 })
              .success(function (largeLoad) {

                  $scope.setPagingData(largeLoad.org, page, pageSize);
              });
             } else {
                 $http.get("/User/GetUserList").success(function (largeLoad) {

                     $scope.setPagingData(largeLoad.org, page, pageSize);

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
                                     { field: 'Operation', width: 100, cellClass: 'OperationClass', sortable: false, headerCellClass: 'ageHeader', cellTemplate: "<div class='btnclass'><span style='margin-right: 3%;' ng-click='DeleteUser(row.entity)' class='label label-danger'>Delete</span></div>" }
         ],
         //showSelectionCheckbox: true,
         //selectWithCheckboxOnly:true,
         //checkboxCellTemplate: '<div class="ngSelectionCell"><input tabindex="-1" class="ngSelectionCheckbox" type="checkbox" ng-checked="row.selected" ng-click="SelectedRowData(row)" /></div>',
         rowTemplate: '<div ng-dblclick="EditUser(row.entity)"   ng-repeat="col in renderedColumns"    ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div class="ngVerticalBar" ng-style="{height: rowHeight}" ng-class="{ ngVerticalBarVisible: !$last }">&nbsp;</div><div ng-cell></div></div>'

     };


     //Grid Code End




 });