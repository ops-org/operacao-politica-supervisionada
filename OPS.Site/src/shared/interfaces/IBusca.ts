import { IDeputadoFederal } from "./IDeputadoFederal"
import { IFornecedor } from "./IFornecedor"
import { ISenador } from "./ISenador"

export interface IBusca{
  deputado_federal: IDeputadoFederal;
  senador: ISenador;
  fornecedor: IFornecedor;
}
