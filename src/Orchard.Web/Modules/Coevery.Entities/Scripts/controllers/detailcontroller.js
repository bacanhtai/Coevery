﻿'use strict';

define(['core/app/detourService'], function (detour) {
    detour.registerController([
        'EntityDetailCtrl',
        ['$scope', 'logger', '$detour', '$stateParams', '$resource',
            function ($scope, logger, $detour, $stateParams) {

                $scope.exit = function() {
                    $detour.transitionTo('EntityList');
                };
                
                $scope.edit = function () {
                    $detour.transitionTo('EntityEdit', { Id: $stateParams.Id });
                };

                $scope.listViewDesigner = function() {
                    $detour.transitionTo('ProjectionList', { EntityName: $stateParams.Id });
                };
                $scope.formDesigner = function() {
                    location.href = 'FormDesigner/SystemAdmin/Index/' + $stateParams.Id;
                };
            }]
    ]);
});