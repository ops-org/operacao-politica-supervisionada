'use strict';

app.controller('DeputadoFederalListaController', ["$rootScope", "$scope", "$tabela", "$api", "$queryString",
    function ($rootScope, $scope, $tabela, $api, $queryString) {

    	var init = function () {
    		document.title = "OPS :: Deputado Federal";
    		$scope.filtro = {};

    		var qs = $queryString.search();
    		if (qs.page) {
    			$tabela.params.page = parseInt(qs.page);
    		}
    		if (qs.sorting) {
    			var lstSorting = qs.sorting.split(' ');
    			$tabela.params.sorting = {};
    			$tabela.params.sorting[lstSorting[0]] = lstSorting[1];
    		}

    		OPS.select("#lstParlamentar", "./Api/Deputado/Pesquisa", qs.IdParlamentar);
    		$scope.filtro.IdParlamentar = qs.IdParlamentar;

    		OPS.select("#lstDespesa", "./Api/Deputado/TipoDespesa", qs.Despesa);
    		$scope.filtro.Despesa = qs.Despesa;

    		//OPS.select("#lstFornecedor", "./Api/Fornecedor/Pesquisa", qs.Fornecedor);
    		$('#txtBeneficiario').val(qs.Fornecedor);
    		$scope.filtro.Fornecedor = qs.Fornecedor;

    		OPS.select("#lstUF", "./Api/Estado", qs.Uf);
    		$scope.filtro.Uf = qs.Uf;

    		OPS.select("#lstPartido", "./Api/Partido", qs.Partido);
    		$scope.filtro.Partido = qs.Partido;

    		$("#txtDocumento").val(qs.Documento);
            $scope.filtro.Documento = qs.Documento || null;

            if (qs.PeriodoCustom && qs.PeriodoCustom.length > 1) {
                var periodo = qs.PeriodoCustom.split('-');
                if (periodo[0].length == 6) {
                    $("#lstPeridoAnoInicio").val(periodo[0].substr(0, 4));
                    $("#lstPeridoMesInicio").val(periodo[0].substr(4, 2));
                }
                if (periodo[1].length == 6) {
                    $("#lstPeridoAnoFinal").val(periodo[1].substr(0, 4));
                    $("#lstPeridoMesFinal").val(periodo[1].substr(4, 2));
                }
            }
            $scope.filtro.PeriodoCustom = qs.PeriodoCustom || null;

    		$scope.filtro.Periodo = $("#lstPerido").val(qs.Periodo || "9").trigger('change').val();
    		$scope.TrocaAba(null, parseInt(qs.Agrupamento || '1'));

    		$scope.Pesquisar(true);

    		$('#lstPerido').selectpicker({
    			width: '100%',
    			actionsBox: true,
    			liveSearch: true,
    			liveSearchNormalize: true
    		});

            $('#lblDeputadoFederalUltimaAtualizacao').text(window.DeputadoFederalUltimaAtualizacao);

	        $("#lstPeridoMesInicio,#lstPeridoAnoInicio,#lstPeridoMesFinal,#lstPeridoAnoFinal").change(function() {
	            $("#lstPerido").val('0').selectpicker('refresh');
	        });
	    }

        $scope.Pesquisar = function (page_load) {
            var lstParlamentar = $("#lstParlamentar").val();
            if (lstParlamentar && lstParlamentar.length > 50) {
                alert('No maximo 50 deputados / lideranças podem ser selecionados por vez. Redefina sua busca e tente novamente.');
                return;
            }

    		if (!page_load) {
                $scope.filtro.IdParlamentar = (lstParlamentar || []).join(',') || null;
    		    $scope.filtro.Despesa = ($("#lstDespesa").val() || []).join(',') || null;
    		    $scope.filtro.Uf = ($("#lstUF").val() || []).join(',') || null;
    		    $scope.filtro.Partido = ($("#lstPartido").val() || []).join(',') || null;
    		    $scope.filtro.Fornecedor = $("#txtBeneficiario").val() || null;
    			$scope.filtro.Documento = $("#txtDocumento").val() || null;
    			$scope.filtro.Periodo = $("#lstPerido").val();
                $scope.filtro.Agrupamento = $("#lstAgrupamento").val();
                var competencia = $("#lstPeridoAnoInicio").val() + $("#lstPeridoMesInicio").val() + '-' + $("#lstPeridoAnoFinal").val() + $("#lstPeridoMesFinal").val();
                $scope.filtro.PeriodoCustom = competencia !== '-' ? competencia : null;

    			delete $tabela.params.sorting;
    			$tabela.params.page = 1;
    		}
    		$scope.BuscaGrid();
    	};

    	$scope.BuscaGrid = function () {
    		$scope.tableParams = $tabela.databind('Deputado/Lancamentos', $scope.filtro);
    	}

        $scope.LimparFiltros = function () {
            document.forms[0].reset();

    		$("#lstParlamentar, #lstDespesa, #lstUF, #lstPartido").selectpicker('deselectAll');
    		$("#txtBeneficiario, #txtDocumento").val('');
    		$("#lstPerido").val('8').selectpicker('refresh');
    		$scope.TrocaAba(null, 1);
    	}

    	$scope.TrocaAba = function ($event, id) {
    		if ($event) {
    			$event.preventDefault();
    		}

    		$rootScope.valor_total = null;
    		$scope.tableParams = null;

            $('#dvDocumento,#dvPeriodoCustom').hide();
	        $('.nav-tabs li').removeClass('active');
    		$('.aba-' + id).addClass('active');

    		switch (id) {
    			case 1:
    				$scope.visao = 'parlamentar.html';
    				break;
    			case 2:
    				$scope.visao = 'despesa.html';
    				break;
    			case 3:
    				$scope.visao = 'fornecedor.html';
    				break;
    			case 4:
    				$scope.visao = 'partido.html';
    				break;
    			case 5:
    				$scope.visao = 'uf.html';
    				break;
    			case 6:
    				$scope.visao = 'documento.html';
                    $('#dvDocumento,#dvPeriodoCustom').show();
    				break;
    		}

            $scope.filtro.Agrupamento = id;

	        if (id !== 6) {
	            $("#lstPeridoMesInicio,#lstPeridoAnoInicio,#lstPeridoMesFinal,#lstPeridoAnoFinal").val('');
	        }
	    }

    	$scope.AbreModalConsultaFornecedor = function() {
    	    $('#dvConsultaFornecedor').modal('show');
    	}

    	$scope.ConsultaFornecedor = function () {
	        if ($scope.formFornecedor && ($scope.formFornecedor.nome || $scope.formFornecedor.cnpj)) {
	            $api.post('Fornecedor/Consulta', $scope.formFornecedor)
	                .success(function(data) {
	                    if (!data || data.length === 0) {
	                        $scope.fornecedores = null;
	                        alert('Nenhum Beneficiário Encontrado com o Filtro Informado.');
	                    } else {
	                        $scope.fornecedores = data;
	                    }
	                });
	        } else {
	            alert('Favor Informe o Nome ou CPF/CNPJ do Beneficiário.');
	        }
    	}

    	$scope.LimparFornecedor = function () {
	        $scope.formFornecedor = null;
    	    $scope.fornecedores = null;
    	}

    	$scope.LimparFiltroFornecedor = function() {
	        $('#txtBeneficiario').val('');
	    }

    	$scope.SelecionarFornecedor = function (fornecedor) {
    	    $('#txtBeneficiario').val(fornecedor.cnpj_cpf);

    	    $('#dvConsultaFornecedor').modal('hide');
	        $scope.LimparFornecedor();
	    }

    	init();
    }]);
