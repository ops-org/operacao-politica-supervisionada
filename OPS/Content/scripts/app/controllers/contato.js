'use strict';

app.controller('ContatoController', ['$scope', '$api', '$timeout',
    function ($scope, $api, $timeout) {
        document.title = "OPS :: Contato";

    	$scope.disqusConfig = {
    		disqus_identifier: 'Contato',
    	};

    	$scope.submit = function (form) {
    		// If form is invalid, return and let AngularJS show validation errors.
    		if (form.$invalid) {
    			return;
    		}

    		// Trigger validation flag.
    		$scope.submitted = true;

    		// Default values for the request.
    		var params = {
    			'name': $scope.name,
    			'email': $scope.email,
    			'comments': $scope.comments
    		};

			$api.post('Contato', params).then(function (data, status, headers, config) {
    			$scope.name = null;
    			$scope.email = null;
    			$scope.comments = null;
    			$scope.messages = 'Sua mensagem foi enviada com sucesso!';
    			$scope.submitted = false;

    			// Hide status messages after three seconds.
    			$timeout(function () {
    				$scope.messages = null;
    			}, 10000);
			}, function (data, status, headers, config) {
			    $scope.submitted = false;

			    if (ga.q) {
			        ga('send', 'event', 'Email', 'envio', 'Lúcio', {
			            'transport': 'beacon'
			        });
			    }
    		});
    	};
    }]);