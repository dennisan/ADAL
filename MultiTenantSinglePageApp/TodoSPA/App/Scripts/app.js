'use strict';
angular.module('todoApp', ['ngRoute','AdalAngular'])
.config(['$routeProvider', '$httpProvider', 'adalAuthenticationServiceProvider', function ($routeProvider, $httpProvider, adalProvider) {

    $routeProvider.when("/Home", {
        controller: "homeCtrl",
        templateUrl: "/App/Views/Home.html",
    }).when("/TodoList", {
        controller: "todoListCtrl",
        templateUrl: "/App/Views/TodoList.html",
        requireADLogin: true,
    }).when("/UserData", {
        controller: "userDataCtrl",
        templateUrl: "/App/Views/UserData.html",
    }).otherwise({ redirectTo: "/Home" });

    adalProvider.init(
        {
        	//tenant: 'denscorp.onmicrosoft.com',
        	clientId: 'b3bc0840-bb76-409c-8fe9-f209070bda97',
        	//extraQueryParameter: 'nux=1',
        	//appKey: 'pBY86YLh4SIOvlEfFFaoALLcDj1k9nKoWyMQAxZWyXo='
        },
        $httpProvider
        );
   
}]);
 