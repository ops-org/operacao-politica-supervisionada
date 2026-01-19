// using System;
// using System.Globalization;
// using System.IO;
// using System.Text;
// using CsvHelper;
// using OPS.Core;

// namespace OPS.Importador
// {
//     public class Candidatos
//     {
//         private readonly string[] ColunasCandidados = {
//             "DT_GERACAO", "HH_GERACAO", "ANO_ELEICAO", "CD_TIPO_ELEICAO", "NM_TIPO_ELEICAO", "NR_TURNO", "CD_ELEICAO", "DS_ELEICAO", "DT_ELEICAO",
//             "TP_ABRANGENCIA", "SG_UF", "SG_UE", "NM_UE", "CD_CARGO", "DS_CARGO", "SQ_CANDIDATO", "NR_CANDIDATO", "NM_CANDIDATO", "NM_URNA_CANDIDATO",
//             "NM_SOCIAL_CANDIDATO", "NR_CPF_CANDIDATO", "NM_EMAIL", "CD_SITUACAO_CANDIDATURA", "DS_SITUACAO_CANDIDATURA", "CD_DETALHE_SITUACAO_CAND", "DS_DETALHE_SITUACAO_CAND",
//             "TP_AGREMIACAO", "NR_PARTIDO", "SG_PARTIDO", "NM_PARTIDO", "SQ_COLIGACAO", "NM_COLIGACAO", "DS_COMPOSICAO_COLIGACAO", "CD_NACIONALIDADE", "DS_NACIONALIDADE",
//             "SG_UF_NASCIMENTO", "CD_MUNICIPIO_NASCIMENTO", "NM_MUNICIPIO_NASCIMENTO", "DT_NASCIMENTO", "NR_IDADE_DATA_POSSE", "NR_TITULO_ELEITORAL_CANDIDATO", "CD_GENERO",
//             "DS_GENERO", "CD_GRAU_INSTRUCAO", "DS_GRAU_INSTRUCAO", "CD_ESTADO_CIVIL", "DS_ESTADO_CIVIL", "CD_COR_RACA", "DS_COR_RACA", "CD_OCUPACAO", "DS_OCUPACAO",
//             "VR_DESPESA_MAX_CAMPANHA", "CD_SIT_TOT_TURNO", "DS_SIT_TOT_TURNO", "ST_REELEICAO", "ST_DECLARAR_BENS", "NR_PROTOCOLO_CANDIDATURA", "NR_PROCESSO",
//             "CD_SITUACAO_CANDIDATO_PLEITO", "DS_SITUACAO_CANDIDATO_PLEITO", "CD_SITUACAO_CANDIDATO_URNA", "DS_SITUACAO_CANDIDATO_URNA", "ST_CANDIDATO_INSERIDO_URNA"
//         };

//         public void ImportarCandidatos(string file)
//         {
//             var totalColunas = ColunasCandidados.Length;

//             using (var banco = new AppDb())
//             {
//                 var cultureInfo = new CultureInfo("pt-BR");

//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
//                 using (var csv = new CsvReader(reader, cultureInfo))
//                 {
//                     int linha = 0;
//                     while (csv.Read())
//                     {
//                         if (++linha == 1)
//                         {
//                             for (int i = 0; i < totalColunas - 1; i++)
//                             {
//                                 if (csv[i] != ColunasCandidados[i])
//                                 {
//                                     throw new Exception("Mudança de integração detectada para o TSE");
//                                 }
//                             }

//                             // Pular linha de titulo
//                             continue;
//                         }

//                         for (int i = 0; i < totalColunas; i++)
//                         {
//                             string coluna = ColunasCandidados[i];
//                             dynamic valor = csv[i];

//                             if (!string.IsNullOrEmpty(valor))
//                             {
//                                 if (coluna.StartsWith("DT_"))
//                                 {
//                                     valor = Convert.ToDateTime(valor, cultureInfo).ToString("yyyy-MM-dd");
//                                 }
//                                 //else if (coluna.StartsWith("NM_") || coluna.StartsWith("num"))
//                                 //{
//                                 //    valor = Convert.ToDecimal(valor, cultureInfo);
//                                 //}
//                                 else
//                                 {
//                                     valor = valor.ToString().Trim();
//                                 }
//                             }
//                             else
//                             {
//                                 valor = null;
//                             }

//                             banco.AddParameter(coluna, valor);
//                         }

//                         banco.ExecuteNonQuery(@"
// INSERT INTO temp.tse_candidato (
//     DT_GERACAO, HH_GERACAO, ANO_ELEICAO, CD_TIPO_ELEICAO, NM_TIPO_ELEICAO, NR_TURNO, CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, 
//     TP_ABRANGENCIA, SG_UF, SG_UE, NM_UE, CD_CARGO, DS_CARGO, SQ_CANDIDATO, NR_CANDIDATO, NM_CANDIDATO, NM_URNA_CANDIDATO, 
//     NM_SOCIAL_CANDIDATO, NR_CPF_CANDIDATO, NM_EMAIL, CD_SITUACAO_CANDIDATURA, DS_SITUACAO_CANDIDATURA, CD_DETALHE_SITUACAO_CAND, DS_DETALHE_SITUACAO_CAND, 
//     TP_AGREMIACAO, NR_PARTIDO, SG_PARTIDO, NM_PARTIDO, SQ_COLIGACAO, NM_COLIGACAO, DS_COMPOSICAO_COLIGACAO, CD_NACIONALIDADE, DS_NACIONALIDADE, 
//     SG_UF_NASCIMENTO, CD_MUNICIPIO_NASCIMENTO, NM_MUNICIPIO_NASCIMENTO, DT_NASCIMENTO, NR_IDADE_DATA_POSSE, NR_TITULO_ELEITORAL_CANDIDATO, CD_GENERO, 
//     DS_GENERO, CD_GRAU_INSTRUCAO, DS_GRAU_INSTRUCAO, CD_ESTADO_CIVIL, DS_ESTADO_CIVIL, CD_COR_RACA, DS_COR_RACA, CD_OCUPACAO, DS_OCUPACAO, 
//     VR_DESPESA_MAX_CAMPANHA, CD_SIT_TOT_TURNO, DS_SIT_TOT_TURNO, ST_REELEICAO, ST_DECLARAR_BENS, NR_PROTOCOLO_CANDIDATURA, NR_PROCESSO, 
//     CD_SITUACAO_CANDIDATO_PLEITO, DS_SITUACAO_CANDIDATO_PLEITO, CD_SITUACAO_CANDIDATO_URNA, DS_SITUACAO_CANDIDATO_URNA, ST_CANDIDATO_INSERIDO_URNA
// ) VALUES (
//     @DT_GERACAO, @HH_GERACAO, @ANO_ELEICAO, @CD_TIPO_ELEICAO, @NM_TIPO_ELEICAO, @NR_TURNO, @CD_ELEICAO, @DS_ELEICAO, @DT_ELEICAO, 
//     @TP_ABRANGENCIA, @SG_UF, @SG_UE, @NM_UE, @CD_CARGO, @DS_CARGO, @SQ_CANDIDATO, @NR_CANDIDATO, @NM_CANDIDATO, @NM_URNA_CANDIDATO, 
//     @NM_SOCIAL_CANDIDATO, @NR_CPF_CANDIDATO, @NM_EMAIL, @CD_SITUACAO_CANDIDATURA, @DS_SITUACAO_CANDIDATURA, @CD_DETALHE_SITUACAO_CAND, @DS_DETALHE_SITUACAO_CAND, 
//     @TP_AGREMIACAO, @NR_PARTIDO, @SG_PARTIDO, @NM_PARTIDO, @SQ_COLIGACAO, @NM_COLIGACAO, @DS_COMPOSICAO_COLIGACAO, @CD_NACIONALIDADE, @DS_NACIONALIDADE, 
//     @SG_UF_NASCIMENTO, @CD_MUNICIPIO_NASCIMENTO, @NM_MUNICIPIO_NASCIMENTO, @DT_NASCIMENTO, @NR_IDADE_DATA_POSSE, @NR_TITULO_ELEITORAL_CANDIDATO, @CD_GENERO, 
//     @DS_GENERO, @CD_GRAU_INSTRUCAO, @DS_GRAU_INSTRUCAO, @CD_ESTADO_CIVIL, @DS_ESTADO_CIVIL, @CD_COR_RACA, @DS_COR_RACA, @CD_OCUPACAO, @DS_OCUPACAO, 
//     @VR_DESPESA_MAX_CAMPANHA, @CD_SIT_TOT_TURNO, @DS_SIT_TOT_TURNO, @ST_REELEICAO, @ST_DECLARAR_BENS, @NR_PROTOCOLO_CANDIDATURA, @NR_PROCESSO, 
//     @CD_SITUACAO_CANDIDATO_PLEITO, @DS_SITUACAO_CANDIDATO_PLEITO, @CD_SITUACAO_CANDIDATO_URNA, @DS_SITUACAO_CANDIDATO_URNA, @ST_CANDIDATO_INSERIDO_URNA
// )");
//                     }
//                 }
//             }
//         }


//         private readonly string[] ColunasDespesasPagas = {
//             "DT_GERACAO", "HH_GERACAO", "ANO_ELEICAO", "CD_TIPO_ELEICAO", "NM_TIPO_ELEICAO", "CD_ELEICAO", "DS_ELEICAO", "DT_ELEICAO", "ST_TURNO",
//             "TP_PRESTACAO_CONTAS", "DT_PRESTACAO_CONTAS", "SQ_PRESTADOR_CONTAS", "SG_UF", "DS_TIPO_DOCUMENTO", "NR_DOCUMENTO", "CD_FONTE_DESPESA", "DS_FONTE_DESPESA",
//             "CD_ORIGEM_DESPESA", "DS_ORIGEM_DESPESA", "CD_NATUREZA_DESPESA", "DS_NATUREZA_DESPESA", "CD_ESPECIE_RECURSO", "DS_ESPECIE_RECURSO",
//             "SQ_DESPESA", "SQ_PARCELAMENTO_DESPESA", "DT_PAGTO_DESPESA", "DS_DESPESA", "VR_PAGTO_DESPESA",
//         };

//         public void ImportarDespesasPagas(string file)
//         {
//             var totalColunas = ColunasDespesasPagas.Length;

//             using (var banco = new AppDb())
//             {
//                 var cultureInfoBR = new CultureInfo("pt-BR");
//                 var cultureInfoUS = new CultureInfo("en-US");

//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
//                 using (var csv = new CsvReader(reader, cultureInfoBR))
//                 {
//                     int linha = 0;
//                     while (csv.Read())
//                     {
//                         if (++linha == 1)
//                         {
//                             for (int i = 0; i < totalColunas - 1; i++)
//                             {
//                                 if (csv[i] != ColunasDespesasPagas[i])
//                                 {
//                                     throw new Exception("Mudança de integração detectada para o TSE");
//                                 }
//                             }

//                             // Pular linha de titulo
//                             continue;
//                         }

//                         for (int i = 0; i < totalColunas; i++)
//                         {
//                             string coluna = ColunasDespesasPagas[i];
//                             dynamic valor = csv[i];

//                             if (!string.IsNullOrEmpty(valor))
//                             {
//                                 if (coluna.StartsWith("DT_"))
//                                 {
//                                     valor = Convert.ToDateTime(valor, cultureInfoBR).ToString("yyyy-MM-dd");
//                                 }
//                                 else if (coluna.StartsWith("VR_"))
//                                 {
//                                     valor = Convert.ToDecimal(valor, cultureInfoBR).ToString(cultureInfoUS);
//                                 }
//                                 else
//                                 {
//                                     valor = valor.ToString().Trim();
//                                 }
//                             }
//                             else
//                             {
//                                 valor = null;
//                             }

//                             banco.AddParameter(coluna, valor);
//                         }

//                         banco.ExecuteNonQuery(@"
// INSERT INTO temp.tse_despesa_paga (
//     DT_GERACAO, HH_GERACAO, ANO_ELEICAO, CD_TIPO_ELEICAO, NM_TIPO_ELEICAO, CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, ST_TURNO, 
//     TP_PRESTACAO_CONTAS, DT_PRESTACAO_CONTAS, SQ_PRESTADOR_CONTAS, SG_UF, DS_TIPO_DOCUMENTO, NR_DOCUMENTO, CD_FONTE_DESPESA, DS_FONTE_DESPESA, 
//     CD_ORIGEM_DESPESA, DS_ORIGEM_DESPESA, CD_NATUREZA_DESPESA, DS_NATUREZA_DESPESA, CD_ESPECIE_RECURSO, DS_ESPECIE_RECURSO, SQ_DESPESA, 
//     SQ_PARCELAMENTO_DESPESA, DT_PAGTO_DESPESA, DS_DESPESA, VR_PAGTO_DESPESA
// ) VALUES (
//     @DT_GERACAO, @HH_GERACAO, @ANO_ELEICAO, @CD_TIPO_ELEICAO, @NM_TIPO_ELEICAO, @CD_ELEICAO, @DS_ELEICAO, @DT_ELEICAO, @ST_TURNO, 
//     @TP_PRESTACAO_CONTAS, @DT_PRESTACAO_CONTAS, @SQ_PRESTADOR_CONTAS, @SG_UF, @DS_TIPO_DOCUMENTO, @NR_DOCUMENTO, @CD_FONTE_DESPESA, @DS_FONTE_DESPESA, 
//     @CD_ORIGEM_DESPESA, @DS_ORIGEM_DESPESA, @CD_NATUREZA_DESPESA, @DS_NATUREZA_DESPESA, @CD_ESPECIE_RECURSO, @DS_ESPECIE_RECURSO, @SQ_DESPESA, 
//     @SQ_PARCELAMENTO_DESPESA, @DT_PAGTO_DESPESA, @DS_DESPESA, @VR_PAGTO_DESPESA
// )");
//                     }
//                 }
//             }
//         }


//         private readonly string[] ColunasDespesasContratadas = {
//             "DT_GERACAO","HH_GERACAO","ANO_ELEICAO","CD_TIPO_ELEICAO","NM_TIPO_ELEICAO","CD_ELEICAO","DS_ELEICAO","DT_ELEICAO","ST_TURNO",
//             "TP_PRESTACAO_CONTAS","DT_PRESTACAO_CONTAS", "SQ_PRESTADOR_CONTAS", "SG_UF","SG_UE","NM_UE","NR_CNPJ_PRESTADOR_CONTA","CD_CARGO","DS_CARGO",
//             "SQ_CANDIDATO","NR_CANDIDATO","NM_CANDIDATO","NR_CPF_CANDIDATO","NR_CPF_VICE_CANDIDATO", "NR_PARTIDO", "SG_PARTIDO","NM_PARTIDO",
//             "CD_TIPO_FORNECEDOR","DS_TIPO_FORNECEDOR","CD_CNAE_FORNECEDOR","DS_CNAE_FORNECEDOR","NR_CPF_CNPJ_FORNECEDOR","NM_FORNECEDOR","NM_FORNECEDOR_RFB",
//             "CD_ESFERA_PART_FORNECEDOR","DS_ESFERA_PART_FORNECEDOR", "SG_UF_FORNECEDOR","CD_MUNICIPIO_FORNECEDOR","NM_MUNICIPIO_FORNECEDOR",
//             "SQ_CANDIDATO_FORNECEDOR","NR_CANDIDATO_FORNECEDOR", "CD_CARGO_FORNECEDOR","DS_CARGO_FORNECEDOR","NR_PARTIDO_FORNECEDOR","SG_PARTIDO_FORNECEDOR","NM_PARTIDO_FORNECEDOR",
//             "DS_TIPO_DOCUMENTO","NR_DOCUMENTO","CD_ORIGEM_DESPESA","DS_ORIGEM_DESPESA",  "SQ_DESPESA","DT_DESPESA","DS_DESPESA","VR_DESPESA_CONTRATADA"
//         };

//         public void ImportarDespesasContratadas(string file)
//         {
//             var totalColunas = ColunasDespesasContratadas.Length;

//             using (var banco = new AppDb())
//             {
//                 var cultureInfoBR = new CultureInfo("pt-BR");
//                 var cultureInfoUS = new CultureInfo("en-US");

//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
//                 using (var csv = new CsvReader(reader, cultureInfoBR))
//                 {
//                     int linha = 0;
//                     while (csv.Read())
//                     {
//                         if (++linha == 1)
//                         {
//                             for (int i = 0; i < totalColunas - 1; i++)
//                             {
//                                 if (csv[i] != ColunasDespesasContratadas[i])
//                                 {
//                                     throw new Exception("Mudança de integração detectada para o TSE");
//                                 }
//                             }

//                             // Pular linha de titulo
//                             continue;
//                         }

//                         for (int i = 0; i < totalColunas; i++)
//                         {
//                             string coluna = ColunasDespesasContratadas[i];
//                             dynamic valor = csv[i];

//                             if (!string.IsNullOrEmpty(valor))
//                             {
//                                 if (coluna.StartsWith("DT_"))
//                                 {
//                                     valor = Convert.ToDateTime(valor, cultureInfoBR).ToString("yyyy-MM-dd");
//                                 }
//                                 else if (coluna.StartsWith("VR_"))
//                                 {
//                                     valor = Convert.ToDecimal(valor, cultureInfoBR).ToString(cultureInfoUS);
//                                 }
//                                 else
//                                 {
//                                     valor = valor.ToString().Trim();
//                                 }
//                             }
//                             else
//                             {
//                                 valor = null;
//                             }

//                             banco.AddParameter(coluna, valor);
//                         }

//                         banco.ExecuteNonQuery(@"
// INSERT INTO temp.tse_despesa_contratada (
//     DT_GERACAO, HH_GERACAO, ANO_ELEICAO, CD_TIPO_ELEICAO, NM_TIPO_ELEICAO, CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, ST_TURNO, TP_PRESTACAO_CONTAS, DT_PRESTACAO_CONTAS, SQ_PRESTADOR_CONTAS, 
//     SG_UF, SG_UE, NM_UE, NR_CNPJ_PRESTADOR_CONTA, CD_CARGO, DS_CARGO, SQ_CANDIDATO, NR_CANDIDATO, NM_CANDIDATO, NR_CPF_CANDIDATO, NR_CPF_VICE_CANDIDATO, NR_PARTIDO, SG_PARTIDO, NM_PARTIDO, 
//     CD_TIPO_FORNECEDOR, DS_TIPO_FORNECEDOR, CD_CNAE_FORNECEDOR, DS_CNAE_FORNECEDOR, NR_CPF_CNPJ_FORNECEDOR, NM_FORNECEDOR, NM_FORNECEDOR_RFB, CD_ESFERA_PART_FORNECEDOR, DS_ESFERA_PART_FORNECEDOR, 
//     SG_UF_FORNECEDOR, CD_MUNICIPIO_FORNECEDOR, NM_MUNICIPIO_FORNECEDOR, SQ_CANDIDATO_FORNECEDOR, NR_CANDIDATO_FORNECEDOR, CD_CARGO_FORNECEDOR, DS_CARGO_FORNECEDOR, 
//     NR_PARTIDO_FORNECEDOR, SG_PARTIDO_FORNECEDOR, NM_PARTIDO_FORNECEDOR, DS_TIPO_DOCUMENTO, NR_DOCUMENTO, CD_ORIGEM_DESPESA, DS_ORIGEM_DESPESA, SQ_DESPESA, DT_DESPESA, DS_DESPESA, VR_DESPESA_CONTRATADA
// ) VALUES (
//     @DT_GERACAO, @HH_GERACAO, @ANO_ELEICAO, @CD_TIPO_ELEICAO, @NM_TIPO_ELEICAO, @CD_ELEICAO, @DS_ELEICAO, @DT_ELEICAO, @ST_TURNO, @TP_PRESTACAO_CONTAS, @DT_PRESTACAO_CONTAS, @SQ_PRESTADOR_CONTAS, 
//     @SG_UF, @SG_UE, @NM_UE, @NR_CNPJ_PRESTADOR_CONTA, @CD_CARGO, @DS_CARGO, @SQ_CANDIDATO, @NR_CANDIDATO, @NM_CANDIDATO, @NR_CPF_CANDIDATO, @NR_CPF_VICE_CANDIDATO, @NR_PARTIDO, @SG_PARTIDO, @NM_PARTIDO, 
//     @CD_TIPO_FORNECEDOR, @DS_TIPO_FORNECEDOR, @CD_CNAE_FORNECEDOR, @DS_CNAE_FORNECEDOR, @NR_CPF_CNPJ_FORNECEDOR, @NM_FORNECEDOR, @NM_FORNECEDOR_RFB, @CD_ESFERA_PART_FORNECEDOR, @DS_ESFERA_PART_FORNECEDOR, 
//     @SG_UF_FORNECEDOR, @CD_MUNICIPIO_FORNECEDOR, @NM_MUNICIPIO_FORNECEDOR, @SQ_CANDIDATO_FORNECEDOR, @NR_CANDIDATO_FORNECEDOR, @CD_CARGO_FORNECEDOR, @DS_CARGO_FORNECEDOR, 
//     @NR_PARTIDO_FORNECEDOR, @SG_PARTIDO_FORNECEDOR, @NM_PARTIDO_FORNECEDOR, @DS_TIPO_DOCUMENTO, @NR_DOCUMENTO, @CD_ORIGEM_DESPESA, @DS_ORIGEM_DESPESA, @SQ_DESPESA, @DT_DESPESA, @DS_DESPESA, @VR_DESPESA_CONTRATADA
// )");
//                     }
//                 }
//             }
//         }


//         private readonly string[] ColunasReceitasDoadorOriginario = {
//             "DT_GERACAO", "HH_GERACAO", "ANO_ELEICAO", "CD_TIPO_ELEICAO", "NM_TIPO_ELEICAO", "CD_ELEICAO", "DS_ELEICAO", "DT_ELEICAO", "ST_TURNO", "TP_PRESTACAO_CONTAS",
//             "DT_PRESTACAO_CONTAS", "SQ_PRESTADOR_CONTAS", "SG_UF", "NR_CPF_CNPJ_DOADOR_ORIGINARIO", "NM_DOADOR_ORIGINARIO", "NM_DOADOR_ORIGINARIO_RFB",
//             "TP_DOADOR_ORIGINARIO", "CD_CNAE_DOADOR_ORIGINARIO", "DS_CNAE_DOADOR_ORIGINARIO", "SQ_RECEITA", "DT_RECEITA", "DS_RECEITA", "VR_RECEITA"
//         };

//         public void ImportarReceitasDoadorOriginario(string file)
//         {
//             var totalColunas = ColunasReceitasDoadorOriginario.Length;

//             using (var banco = new AppDb())
//             {
//                 var cultureInfoBR = new CultureInfo("pt-BR");
//                 var cultureInfoUS = new CultureInfo("en-US");

//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
//                 using (var csv = new CsvReader(reader, cultureInfoBR))
//                 {
//                     int linha = 0;
//                     while (csv.Read())
//                     {
//                         if (++linha == 1)
//                         {
//                             for (int i = 0; i < totalColunas - 1; i++)
//                             {
//                                 if (csv[i] != ColunasReceitasDoadorOriginario[i])
//                                 {
//                                     throw new Exception("Mudança de integração detectada para o TSE");
//                                 }
//                             }

//                             // Pular linha de titulo
//                             continue;
//                         }

//                         for (int i = 0; i < totalColunas; i++)
//                         {
//                             string coluna = ColunasReceitasDoadorOriginario[i];
//                             dynamic valor = csv[i];

//                             if (!string.IsNullOrEmpty(valor))
//                             {
//                                 if (coluna.StartsWith("DT_"))
//                                 {
//                                     valor = Convert.ToDateTime(valor, cultureInfoBR).ToString("yyyy-MM-dd");
//                                 }
//                                 else if (coluna.StartsWith("VR_"))
//                                 {
//                                     valor = Convert.ToDecimal(valor, cultureInfoBR).ToString(cultureInfoUS);
//                                 }
//                                 else
//                                 {
//                                     valor = valor.ToString().Trim();
//                                 }
//                             }
//                             else
//                             {
//                                 valor = null;
//                             }

//                             banco.AddParameter(coluna, valor);
//                         }

//                         banco.ExecuteNonQuery(@"
// INSERT INTO temp.tse_receita_doador_originario (
//     DT_GERACAO, HH_GERACAO, ANO_ELEICAO, CD_TIPO_ELEICAO, NM_TIPO_ELEICAO, CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, ST_TURNO, TP_PRESTACAO_CONTAS, 
//     DT_PRESTACAO_CONTAS, SQ_PRESTADOR_CONTAS, SG_UF, NR_CPF_CNPJ_DOADOR_ORIGINARIO, NM_DOADOR_ORIGINARIO, NM_DOADOR_ORIGINARIO_RFB, 
//     TP_DOADOR_ORIGINARIO, CD_CNAE_DOADOR_ORIGINARIO, DS_CNAE_DOADOR_ORIGINARIO, SQ_RECEITA, DT_RECEITA, DS_RECEITA, VR_RECEITA
// ) VALUES (
//     @DT_GERACAO, @HH_GERACAO, @ANO_ELEICAO, @CD_TIPO_ELEICAO, @NM_TIPO_ELEICAO, @CD_ELEICAO, @DS_ELEICAO, @DT_ELEICAO, @ST_TURNO, @TP_PRESTACAO_CONTAS, 
//     @DT_PRESTACAO_CONTAS, @SQ_PRESTADOR_CONTAS, @SG_UF, @NR_CPF_CNPJ_DOADOR_ORIGINARIO, @NM_DOADOR_ORIGINARIO, @NM_DOADOR_ORIGINARIO_RFB, 
//     @TP_DOADOR_ORIGINARIO, @CD_CNAE_DOADOR_ORIGINARIO, @DS_CNAE_DOADOR_ORIGINARIO, @SQ_RECEITA, @DT_RECEITA, @DS_RECEITA, @VR_RECEITA
// )");
//                     }
//                 }
//             }
//         }


//         private readonly string[] ColunasReceitas = {
//             "DT_GERACAO","HH_GERACAO","ANO_ELEICAO","CD_TIPO_ELEICAO","NM_TIPO_ELEICAO","CD_ELEICAO","DS_ELEICAO","DT_ELEICAO","ST_TURNO",
//             "TP_PRESTACAO_CONTAS","DT_PRESTACAO_CONTAS","SQ_PRESTADOR_CONTAS","SG_UF","SG_UE","NM_UE","NR_CNPJ_PRESTADOR_CONTA","CD_CARGO","DS_CARGO",
//             "SQ_CANDIDATO","NR_CANDIDATO","NM_CANDIDATO","NR_CPF_CANDIDATO","NR_CPF_VICE_CANDIDATO","NR_PARTIDO","SG_PARTIDO","NM_PARTIDO",
//             "CD_FONTE_RECEITA","DS_FONTE_RECEITA","CD_ORIGEM_RECEITA","DS_ORIGEM_RECEITA","CD_NATUREZA_RECEITA","DS_NATUREZA_RECEITA",
//             "CD_ESPECIE_RECEITA","DS_ESPECIE_RECEITA","CD_CNAE_DOADOR","DS_CNAE_DOADOR","NR_CPF_CNPJ_DOADOR","NM_DOADOR","NM_DOADOR_RFB",
//             "CD_ESFERA_PARTIDARIA_DOADOR","DS_ESFERA_PARTIDARIA_DOADOR","SG_UF_DOADOR","CD_MUNICIPIO_DOADOR","NM_MUNICIPIO_DOADOR","SQ_CANDIDATO_DOADOR",
//             "NR_CANDIDATO_DOADOR","CD_CARGO_CANDIDATO_DOADOR","DS_CARGO_CANDIDATO_DOADOR","NR_PARTIDO_DOADOR","SG_PARTIDO_DOADOR","NM_PARTIDO_DOADOR",
//             "NR_RECIBO_DOACAO","NR_DOCUMENTO_DOACAO","SQ_RECEITA","DT_RECEITA","DS_RECEITA","VR_RECEITA"
//         };

//         public void ImportarReceitas(string file)
//         {
//             var totalColunas = ColunasReceitas.Length;

//             using (var banco = new AppDb())
//             {
//                 var cultureInfoBR = new CultureInfo("pt-BR");
//                 var cultureInfoUS = new CultureInfo("en-US");

//                 using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
//                 using (var csv = new CsvReader(reader, cultureInfoBR))
//                 {
//                     int linha = 0;
//                     while (csv.Read())
//                     {
//                         if (++linha == 1)
//                         {
//                             for (int i = 0; i < totalColunas - 1; i++)
//                             {
//                                 if (csv[i] != ColunasReceitas[i])
//                                 {
//                                     throw new Exception("Mudança de integração detectada para o TSE");
//                                 }
//                             }

//                             // Pular linha de titulo
//                             continue;
//                         }

//                         for (int i = 0; i < totalColunas; i++)
//                         {
//                             string coluna = ColunasReceitas[i];
//                             dynamic valor = csv[i];

//                             if (!string.IsNullOrEmpty(valor))
//                             {
//                                 if (coluna.StartsWith("DT_"))
//                                 {
//                                     valor = Convert.ToDateTime(valor, cultureInfoBR).ToString("yyyy-MM-dd");
//                                 }
//                                 else if (coluna.StartsWith("VR_"))
//                                 {
//                                     valor = Convert.ToDecimal(valor, cultureInfoBR).ToString(cultureInfoUS);
//                                 }
//                                 else
//                                 {
//                                     valor = valor.ToString().Trim();
//                                 }
//                             }
//                             else
//                             {
//                                 valor = null;
//                             }

//                             banco.AddParameter(coluna, valor);
//                         }

//                         banco.ExecuteNonQuery(@"
// INSERT INTO temp.tse_receita (
//     DT_GERACAO, HH_GERACAO, ANO_ELEICAO, CD_TIPO_ELEICAO, NM_TIPO_ELEICAO, CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, ST_TURNO, 
//     TP_PRESTACAO_CONTAS, DT_PRESTACAO_CONTAS, SQ_PRESTADOR_CONTAS, SG_UF, SG_UE, NM_UE, NR_CNPJ_PRESTADOR_CONTA, CD_CARGO, DS_CARGO, 
//     SQ_CANDIDATO, NR_CANDIDATO, NM_CANDIDATO, NR_CPF_CANDIDATO, NR_CPF_VICE_CANDIDATO, NR_PARTIDO, SG_PARTIDO, NM_PARTIDO, 
//     CD_FONTE_RECEITA, DS_FONTE_RECEITA, CD_ORIGEM_RECEITA, DS_ORIGEM_RECEITA, CD_NATUREZA_RECEITA, DS_NATUREZA_RECEITA, 
//     CD_ESPECIE_RECEITA, DS_ESPECIE_RECEITA, CD_CNAE_DOADOR, DS_CNAE_DOADOR, NR_CPF_CNPJ_DOADOR, NM_DOADOR, NM_DOADOR_RFB, 
//     CD_ESFERA_PARTIDARIA_DOADOR, DS_ESFERA_PARTIDARIA_DOADOR, SG_UF_DOADOR, CD_MUNICIPIO_DOADOR, NM_MUNICIPIO_DOADOR, SQ_CANDIDATO_DOADOR, 
//     NR_CANDIDATO_DOADOR, CD_CARGO_CANDIDATO_DOADOR, DS_CARGO_CANDIDATO_DOADOR, NR_PARTIDO_DOADOR, SG_PARTIDO_DOADOR, NM_PARTIDO_DOADOR, 
//     NR_RECIBO_DOACAO, NR_DOCUMENTO_DOACAO, SQ_RECEITA, DT_RECEITA, DS_RECEITA, VR_RECEITA
// ) VALUES (
//     @DT_GERACAO, @HH_GERACAO, @ANO_ELEICAO, @CD_TIPO_ELEICAO, @NM_TIPO_ELEICAO, @CD_ELEICAO, @DS_ELEICAO, @DT_ELEICAO, @ST_TURNO, 
//     @TP_PRESTACAO_CONTAS, @DT_PRESTACAO_CONTAS, @SQ_PRESTADOR_CONTAS, @SG_UF, @SG_UE, @NM_UE, @NR_CNPJ_PRESTADOR_CONTA, @CD_CARGO, @DS_CARGO, 
//     @SQ_CANDIDATO, @NR_CANDIDATO, @NM_CANDIDATO, @NR_CPF_CANDIDATO, @NR_CPF_VICE_CANDIDATO, @NR_PARTIDO, @SG_PARTIDO, @NM_PARTIDO,
//     @CD_FONTE_RECEITA, @DS_FONTE_RECEITA, @CD_ORIGEM_RECEITA, @DS_ORIGEM_RECEITA, @CD_NATUREZA_RECEITA, @DS_NATUREZA_RECEITA,
//     @CD_ESPECIE_RECEITA, @DS_ESPECIE_RECEITA, @CD_CNAE_DOADOR, @DS_CNAE_DOADOR, @NR_CPF_CNPJ_DOADOR, @NM_DOADOR, @NM_DOADOR_RFB, 
//     @CD_ESFERA_PARTIDARIA_DOADOR, @DS_ESFERA_PARTIDARIA_DOADOR, @SG_UF_DOADOR, @CD_MUNICIPIO_DOADOR, @NM_MUNICIPIO_DOADOR, @SQ_CANDIDATO_DOADOR, 
//     @NR_CANDIDATO_DOADOR, @CD_CARGO_CANDIDATO_DOADOR, @DS_CARGO_CANDIDATO_DOADOR, @NR_PARTIDO_DOADOR, @SG_PARTIDO_DOADOR, @NM_PARTIDO_DOADOR, 
//     @NR_RECIBO_DOACAO, @NR_DOCUMENTO_DOACAO, @SQ_RECEITA, @DT_RECEITA, @DS_RECEITA, @VR_RECEITA
// )");
//                     }
//                 }
//             }
//         }
//     }
// }
