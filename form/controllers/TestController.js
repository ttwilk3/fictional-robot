angular.module('form.controllers').controller('TestController',['$routeParams','FormService','$uibModal','AdminService', TestController]);

function TestController($routeParams,FormService,$uibModal,AdminService) {

    var vm = this;
    vm.person = {};

    /*functions*/

    vm.confirmResponse = function (size)
    {
        var modalInstance = $uibModal.open({
            templateUrl: 'common/modals/DeleteModal.html',
            controller: 'DeleteController',
            controllerAs: 'home',
            size:size
        })
        modalInstance.result.then(function (testFunction) {
        if (testFunction != undefined) {
            return;
        }
        });
    }
}