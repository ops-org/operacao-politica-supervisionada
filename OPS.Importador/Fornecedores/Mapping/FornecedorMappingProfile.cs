using System;
using Mapster;
using MinhaReceitaFornecedorInfo = OPS.Importador.Fornecedores.MinhaReceita.FornecedorInfo;
using InfraFornecedorInfo = OPS.Infraestrutura.Entities.Fornecedores.FornecedorInfo;
using InfraFornecedorAtividadeSecundaria = OPS.Infraestrutura.Entities.Fornecedores.FornecedorAtividadeSecundaria;
using OPS.Importador.Fornecedores.MinhaReceita;

namespace OPS.Importador.Fornecedores.Mapping
{
    public static class FornecedorMappingProfile
    {
        public static void Configure()
        {
            // Map MinhaReceita.FornecedorInfo to Infraestrutura.Entities.Fornecedores.FornecedorInfo
            TypeAdapterConfig<MinhaReceitaFornecedorInfo, InfraFornecedorInfo>
                .NewConfig()
                .Map(dest => dest.IdFornecedor, src => (uint)src.Id)
                .Map(dest => dest.Cnpj, src => src.Cnpj)
                .Map(dest => dest.CnpjRadical, src => src.RadicalCnpj)
                .Map(dest => dest.Tipo, src => src.Tipo)
                .Map(dest => dest.Nome, src => src.RazaoSocial)
                .Map(dest => dest.NomeFantasia, src => src.NomeFantasia)
                .Map(dest => dest.IdFornecedorAtividadePrincipal, src => src.IdAtividadePrincipal)
                .Map(dest => dest.IdFornecedorNaturezaJuridica, src => src.IdNaturezaJuridica)
                .Map(dest => dest.Logradouro, src => src.Logradouro)
                .Map(dest => dest.Numero, src => src.Numero)
                .Map(dest => dest.Complemento, src => src.Complemento)
                .Map(dest => dest.Cep, src => src.Cep)
                .Map(dest => dest.Bairro, src => src.Bairro)
                .Map(dest => dest.Municipio, src => src.Municipio)
                .Map(dest => dest.Estado, src => src.UF)
                .Map(dest => dest.EnderecoEletronico, src => src.Email)
                .Map(dest => dest.Telefone1, src => src.Telefone1)
                .Map(dest => dest.EnteFederativoResponsavel, src => src.EnteFederativoResponsavel)
                .Map(dest => dest.SituacaoCadastral, src => src.SituacaoCadastral)
                .Map(dest => dest.DataDaSituacaoCadastral, src => src.DataSituacaoCadastral)
                .Map(dest => dest.MotivoSituacaoCadastral, src => src.MotivoSituacaoCadastral)
                .Map(dest => dest.SituacaoEspecial, src => src.SituacaoEspecial)
                .Map(dest => dest.DataSituacaoEspecial, src => src.DataSituacaoEspecial)
                .Map(dest => dest.CapitalSocial, src => src.CapitalSocial)
                .Map(dest => dest.Porte, src => src.Porte)
                .Map(dest => dest.OpcaoPeloMei, src => src.OpcaoPeloMEI)
                .Map(dest => dest.DataOpcaoPeloMei, src => src.DataOpcaoPeloMEI)
                .Map(dest => dest.DataExclusaoDoMei, src => src.DataExclusaoMEI)
                .Map(dest => dest.OpcaoPeloSimples, src => src.OpcaoPeloSimples)
                .Map(dest => dest.DataOpcaoPeloSimples, src => src.DataOpcaoPeloSimples)
                .Map(dest => dest.DataExclusaoDoSimples, src => src.DataExclusaoSimples)
                .Map(dest => dest.NomeCidadeNoExterior, src => src.NomeCidadeExterior)
                .Map(dest => dest.Pais, src => src.Pais)
                .Map(dest => dest.ObtidoEm, src => src.ObtidoEm);

            // Map QuadroSocietario to FornecedorSocio
            TypeAdapterConfig<QuadroSocietario, Infraestrutura.Entities.Fornecedores.FornecedorSocio>
                .NewConfig()
                .Map(dest => dest.IdFornecedor, src => (uint)src.IdFornecedor)
                .Map(dest => dest.CnpjCpf, src => src.CnpjCpf)
                .Map(dest => dest.Nome, src => src.Nome)
                .Map(dest => dest.PaisOrigem, src => src.PaisOrigem)
                .Map(dest => dest.NomeRepresentante, src => src.NomeRepresentante)
                .Map(dest => dest.CpfRepresentante, src => src.CpfRepresentante)
                .Map(dest => dest.IdFornecedorSocioQualificacao, src => src.IdSocioQualificacao > 0 ? (uint?)src.IdSocioQualificacao : null)
                .Map(dest => dest.IdFornecedorSocioRepresentanteQualificacao, src => src.IdSocioRepresentanteQualificacao > 0 ? (uint?)src.IdSocioRepresentanteQualificacao : null)
                .Map(dest => dest.IdFornecedorFaixaEtaria, src => src.IdFaixaEtaria)
                .Map(dest => dest.DataEntradaSociedade, src => !string.IsNullOrEmpty(src.DataEntradaSociedade) ? (DateTime?)DateTime.Parse(src.DataEntradaSociedade) : null);

            // Map MinhaReceita.FornecedorAtividade to FornecedorAtividadeSecundaria
            TypeAdapterConfig<FornecedorAtividade, InfraFornecedorAtividadeSecundaria>
                .NewConfig()
                .Map(dest => dest.IdAtividade, src => (uint)src.Id)
                .Ignore(dest => dest.IdFornecedor); // This will be set manually in the processing logic

            // Global configuration
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.RequireDestinationMemberSource(true);
        }
    }
}
