using System;
using System.Data;
using System.IO;
using System.Text;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;

namespace OPS.Importador;

public class Presidencia : ImportadorCotaParlamentarBase
{
    public Presidencia(ILogger<CamaraCeara> logger, IConfiguration configuration, IDbConnection connection) :
       base("BR", logger, configuration, connection)
    {
    }

    public override void ImportarArquivoDespesas(int ano)
    {
        var caminhoArquivo = @"C:\temp\Planilha12003a2022.csv";
        var cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR");

        int indice = 0;
        int DataPgto = indice++;
        int CpfServidor = indice++;
        int CpfCnpjFornecedor = indice++;
        int NomeFornecedor = indice++;
        int Valor = indice++;
        int Tipo = indice++;
        int SubelementoDespesa = indice++;
        int CdId = indice++;

        using (var banco = new AppDb())
        {
            using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
            {
                short count = 0;

                using (var csv = new CsvReader(reader, cultureInfo))
                {

                    while (csv.Read())
                    {
                        count++;

                        if (count == 1)
                        {
                            if (
                                (csv[DataPgto] != "DATA PGTO") ||
                                (csv[CpfServidor] != "CPF SERVIDOR") ||
                                (csv[CpfCnpjFornecedor] != "CPF/CNPJ FORNECEDOR") ||
                                (csv[NomeFornecedor] != "NOME FORNECEDOR") ||
                                (csv[Valor] != "VALOR") ||
                                (csv[Tipo] != "TIPO") ||
                                (csv[SubelementoDespesa] != "SUBELEMENTO DE DESPESA") ||
                                (csv[CdId] != "CDIC")
                            )
                            {
                                throw new Exception("Mudança de integração detectada para a Presidência da República");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        if (string.IsNullOrEmpty(csv[DataPgto])) continue; //Linha vazia
                        if (csv[Valor].Contains("R$ -")) continue;
                        if (csv[DataPgto].StartsWith("Fonte")) break;

                        var presidenciaDespesaTemp = new PresidenciaDespesaTemp()
                        {
                            DataPagamento = DateTime.Parse(csv[DataPgto]),
                            Cpf = csv[CpfServidor],
                            CnpjCpf = csv[CpfCnpjFornecedor],
                            Empresa = csv[NomeFornecedor],
                            Valor = Convert.ToDecimal(csv[Valor].Replace("R$ ", ""), cultureInfo),
                            TipoVerba = csv[Tipo],
                            TipoDespesa = csv[SubelementoDespesa],
                            Documento = csv[CdId]
                        };

                        connection.Insert(presidenciaDespesaTemp);
                    }
                }
            }

            ProcessarDespesasTemp();

            //if (ano == DateTime.Now.Year)
            //{
            //    AtualizaParlamentarValores();
            //}
        }
    }

    public void ProcessarDespesasTemp()
    {
        InsereFornecedorFaltante();
        InsereTipoDespesaFaltante();
        InserePessoaFaltante();
    }

    public override void InsereFornecedorFaltante()
    {
        var affected = connection.Execute(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				SELECT MAX(dt.empresa), dt.cnpj_cpf
				FROM ops_tmp.pr_despesa_temp dt
				LEFT JOIN fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				WHERE dt.cnpj_cpf is not null
				AND f.id is null
                AND dt.cnpj_cpf <> ''
				GROUP BY dt.cnpj_cpf;

                INSERT INTO fornecedor (nome, cnpj_cpf)
				SELECT dt.empresa, dt.cnpj_cpf
				FROM ops_tmp.pr_despesa_temp dt
				LEFT JOIN fornecedor f ON dt.empresa = f.nome
				WHERE dt.cnpj_cpf is not null
				AND f.id is null
				AND dt.cnpj_cpf = ''
				GROUP BY dt.empresa, dt.cnpj_cpf;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} fornecedores incluidos!", affected);
        }
    }

    public override void InsereTipoDespesaFaltante()
    {
        var affected = connection.Execute(@"
				INSERT INTO pr_despesa_tipo (descricao)
                SELECT dt.despesa_tipo
				FROM ops_tmp.pr_despesa_temp dt
                LEFT JOIN pr_despesa_tipo t on t.descricao = dt.despesa_tipo
                WHERE t.id is null
				GROUP BY dt.despesa_tipo
				ORDER BY dt.despesa_tipo;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesa incluidos!", affected);
        }
    }

    public void InserePessoaFaltante()
    {
        var affected = connection.Execute(@"
				INSERT INTO pessoa (cpf, nome)
                SELECT dt.cpf, ''
				FROM ops_tmp.pr_despesa_temp dt
				LEFT JOIN pessoa p ON p.cpf = dt.cpf
				WHERE dt.cpf IS NOT NULL AND dt.cpf <> ''
				GROUP BY dt.cpf;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesa incluidos!", affected);
        }
    }

    public override void InsereDespesaFinal()
    {
        var affected = connection.Execute(@$"
INSERT INTO pr_despesa (
	id_pessoa_servidor,
    id_pr_despesa_tipo,
    tipo,
	id_fornecedor,
	data_pgto,
	ano,
	numero_documento,
	valor_liquido
)
SELECT 
	p.id AS id_pessoa_servidor,
    dt.id id_pr_despesa_tipo, 
    d.tipo_verba,
    f.id AS id_fornecedor,
    d.data_pgto,
   year(d.data_pgto) AS ano,
    d.documento AS numero_documento,
    d.valor AS valor
FROM ops_tmp.pr_despesa_temp d
LEFT join pessoa p ON p.cpf = d.cpf
left join pr_despesa_tipo dt on dt.descricao = d.despesa_tipo
LEFT join fornecedor f ON f.cnpj_cpf = d.cnpj_cpf
LEFT JOIN pr_despesa ds ON ds.numero_documento = d.documento
WHERE ds.id IS NULL
AND d.cnpj_cpf <> '';

INSERT INTO pr_despesa (
	id_pessoa_servidor,
    id_pr_despesa_tipo,
    tipo,
	id_fornecedor,
	data_pgto,
	ano,
	numero_documento,
	valor_liquido
)
SELECT 
	p.id AS id_pessoa_servidor,
    dt.id id_pr_despesa_tipo, 
    d.tipo_verba,
    f.id AS id_fornecedor,
    d.data_pgto,
   year(d.data_pgto) AS ano,
    d.documento AS numero_documento,
    d.valor AS valor
FROM ops_tmp.pr_despesa_temp d
LEFT join pessoa p ON p.cpf = d.cpf
left join pr_despesa_tipo dt on dt.descricao = d.despesa_tipo
LEFT join fornecedor f ON d.empresa = f.nome
LEFT JOIN pr_despesa ds ON ds.numero_documento = d.documento
WHERE ds.id IS NULL
AND d.cnpj_cpf = '';

			", 3600);

        if (affected > 0)
        {
            logger.LogInformation("{Itens} despesas incluidas!", affected);
        }
    }
}
