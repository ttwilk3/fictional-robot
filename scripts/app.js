var testProject = angular.module('testProject',['ngRoute','ngAnimate','ngResource','ui.bootstrap','common','form']);

angular.module('testProject').config(['$routeProvider',Routes]);

function Routes ($routeProvider) {
 $routeProvider.
    when('/home',{
        templateUrl:'form/partials/testsheet.html',
        controller:'TestController',
        controllerAs:'home'
    }).otherwise('/home')

}