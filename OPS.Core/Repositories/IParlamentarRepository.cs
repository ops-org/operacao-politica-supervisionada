using System.Collections.Generic;
using System.Threading.Tasks;
using OPS.Core.DTOs;

namespace OPS.Core.Repositories
{
    public interface IParlamentarRepository
    {
        Task<ParlamentarDetalheDTO> Consultar(int id);
        Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO request);
        Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro = null);
        Task<dynamic> Lancamentos(DataTablesRequest request);
        Task<List<TipoDespesaDTO>> TipoDespesa();
        Task<DocumentoDetalheDTO> Documento(int id);
        Task<List<DocumentoRelacionadoDTO>> DocumentosDoMesmoDia(int id);
        Task<List<DocumentoRelacionadoDTO>> DocumentosDaSubcotaMes(int id);
        Task<GraficoBarraDTO> GastosPorAno(int id);
        Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id);
        Task<List<ParlamentarNotaDTO>> MaioresNotas(int id);
        Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id);
    }
}
