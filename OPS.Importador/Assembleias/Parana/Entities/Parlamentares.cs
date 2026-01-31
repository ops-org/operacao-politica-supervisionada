using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        //private void ImportarCotaParlamentar(int ano, int mes)
        //{
        //    // Use Selenium to open the page and wait for cacpcha solve and click in "Consultar" on https://consultas.assembleia.pr.leg.br/#/ressarcimento
        //    //

        //    var address = $"{config.BaseAddress}public/ressarcimento/ressarcimentos/{mes}/{ano}";

        //    ParlamentaresPR objParlamentaresPR = RestApiGetWithCustomDateConverter<ParlamentaresPR>(address);

        //    foreach (var itemParlamentar in objParlamentaresPR.Parlamentares)
        //    {
        //        string nomePolitico = ImportarParlamentar(itemParlamentar);

        //        var newAddress = address.Replace("ressarcimentos", "despesas-ressarcimento") + "/" + itemParlamentar.Parlamentar.Codigo;
        //        DespesasPR objDespesaPR = RestApiGetWithCustomDateConverter<DespesasPR>(newAddress);
        //        if (!objDespesaPR.Sucesso) continue;

        //        foreach (var parlamentarDespesa in objDespesaPR.Despesas)
        //        {
        //            foreach (var despesa in parlamentarDespesa.DespesasAnuais?[0]?.DespesasMensais?[0]?.Despesas)
        //            {
        //                foreach (var itensDespesa in despesa.ItensDespesa)
        //                {
        //                    var despesaTemp = new CamaraEstadualDespesaTemp()
        //                    {
        //                        Nome = nomePolitico.ToTitleCase(),
        //                        Cpf = parlamentarDespesa.Parlamentar.Codigo.ToString(),
        //                        Ano = (short)itensDespesa.Exercicio,
        //                        TipoDespesa = itensDespesa.TipoDespesa.Descricao,
        //                        Valor = (decimal)(itensDespesa.Valor - itensDespesa.ValorDevolucao),
        //                        DataEmissao = itensDespesa.Data,
        //                        CnpjCpf = itensDespesa.Fornecedor?.Documento,
        //                        Empresa = itensDespesa.Fornecedor?.Nome,
        //                        Documento = $"{itensDespesa.NumeroDocumento} [{itensDespesa.Codigo}/{itensDespesa.Numero}]",
        //                        Observacao = itensDespesa.Descricao
        //                    };

        //                    if (itensDespesa.Transporte != null)
        //                    {
        //                        var t = itensDespesa.Transporte;
        //                        var v = t.Veiculo;

        //                        despesaTemp.Observacao =
        //                            $"{t.Descricao}; Veículo: {v.Placa}/{v.Modelo}; Distância: {t.Distancia:N0)}; Periodo: {t.DataSaida:dd/MM/yyyy} à {t.DataChegada:dd/MM/yyyy}";
        //                    }

        //                    if (itensDespesa.Diaria != null)
        //                    {
        //                        var d = itensDespesa.Diaria;

        //                        despesaTemp.Observacao =
        //                            $"{d.Descricao}; Diárias: {d.NumeroDiarias:N1}; Região: {d.Regiao}";
        //                    }

        //                    InserirDespesaTemp(despesaTemp);
        //                }
        //            }
        //        }
        //    }
        //}

        //private string ImportarParlamentar(Parlamentares itemDespesa)
        //{
        //    var parlamentar = itemDespesa.Parlamentar;
        //    var nomeParlamentar = parlamentar.NomePolitico.Replace("DEPUTADO ", "").Replace("DEPUTADA ", "").Trim().ToTitleCase();

        //    var matricula = (int)parlamentar.Codigo;
        //    var deputado = GetDeputadoByMatriculaOrNew(matricula);

        //    var IdPartido = repositoryService.GetList<Partido>(new { sigla = parlamentar.Partido.Replace("REPUB", "REPUBLICANOS").Replace("CDN", "CIDADANIA") }).FirstOrDefault()?.Id;
        //    if (IdPartido == null && deputado.IdPartido == null)
        //        throw new Exception($"Partido '{parlamentar.Partido}' Inexistenete");

        //    //deputado.UrlPerfil = $"http://www.assembleia.pr.leg.br/deputados/perfil/{parlamentar.Codigo}";
        //    deputado.NomeParlamentar = nomeParlamentar;
        //    deputado.NomeCivil = parlamentar.Nome.ToTitleCase();

        //    deputado.IdPartido ??= IdPartido;
        //    deputado.Sexo = parlamentar.NomePolitico.StartsWith("DEPUTADO") ? "M" : "F";

        //    if (deputado.Id == 0)
        //        repositoryService.Insert(deputado);
        //    else
        //        repositoryService.Update(deputado);

        //    return nomeParlamentar;
        //}

        private class Parlamentares
        {
            [JsonPropertyName("parlamentar")]
            public Parlamentar Parlamentar { get; set; }

            //[JsonPropertyName("despesasAnuais")]
            //public List<DespesasAnuais> DespesasAnuais { get; set; }

            //[JsonPropertyName("tipoDespesa")]
            //public TipoDespesa TipoDespesa { get; set; }

            //[JsonPropertyName("itensDespesa")]
            //public List<ItensDespesa> ItensDespesa { get; set; }
        }
    }
}
