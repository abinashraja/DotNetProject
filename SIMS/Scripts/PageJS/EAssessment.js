// 1. define the module and the other module dependencies (if any)
angular.module('EAssessmentModule', [])

// 2. set a constant
    .constant('MODULE_VERSION', '0.0.3')

// 3. maybe set some defaults
    .value('defaults', {
        foo: 'bar'
    })

// 4. define a module component
    .factory('factoryName', function () {/* stuff here */ })

// 5. define another module component
    .service('directiveName', function () {

        this.GetMessageList = function ($http,$scope)
        {
            $http({
                url: '/Home/GetMessageList',
                method: "POST"
            })
    .success(function (data) {
        $scope.GetMessageList = data.messagelist;
        $scope.MegCount = data.msgcount;
       
       
    },
    function (response) { // optional
        // failed

    });
        }
    })
;// and so on