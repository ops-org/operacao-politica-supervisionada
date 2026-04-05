using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPS.Core.DTOs;

namespace OPS.Core.Repositories
{
    public interface IParlamentarRepository
    {
        Task<ParlamentarDetalheDTO> Consultar(int id, CancellationToken ct = default);
        Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO request, CancellationToken ct = default);
        Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro = null, CancellationToken ct = default);
        Task<dynamic> Lancamentos(DataTablesRequest request, CancellationToken ct = default);
        Task<List<TipoDespesaDTO>> TipoDespesa(CancellationToken ct = default);
        Task<DocumentoDetalheDTO> Documento(int id, CancellationToken ct = default);
        Task<List<DocumentoRelacionadoDTO>> DocumentosDoMesmoDia(int id, CancellationToken ct = default);
        Task<List<DocumentoRelacionadoDTO>> DocumentosDaSubcotaMes(int id, CancellationToken ct = default);
        Task<GraficoBarraDTO> GastosPorAno(int id, CancellationToken ct = default);
        Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id, CancellationToken ct = default);
        Task<List<ParlamentarNotaDTO>> MaioresNotas(int id, CancellationToken ct = default);
        Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id, CancellationToken ct = default);
    }
}
