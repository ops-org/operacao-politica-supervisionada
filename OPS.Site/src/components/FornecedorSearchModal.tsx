import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Search, X } from "lucide-react";
import { fetchFornecedores, FornecedorResult } from "@/lib/api";

interface FornecedorSearchModalProps {
  selectedFornecedores: string[];
  onSelectionChange: (fornecedores: string[]) => void;
}

export const FornecedorSearchModal = ({ 
  selectedFornecedores, 
  onSelectionChange 
}: FornecedorSearchModalProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [nome, setNome] = useState("");
  const [cnpj, setCnpj] = useState("");

  const { data: fornecedores = [], isLoading, refetch } = useQuery({
    queryKey: ["fornecedores-search"],
    queryFn: () => fetchFornecedores({ nome, cnpj }),
    enabled: false,
  });

  const handleSearch = () => {
    refetch();
  };

  const handleClear = () => {
    setNome("");
    setCnpj("");
  };

  const handleFinalizeSelection = () => {
    if (selectedFornecedores.length > 0) {
      setIsOpen(false);
    }
  };

  const handleSelectFornecedor = (fornecedor: FornecedorResult) => {
    if (selectedFornecedores.includes(fornecedor.id_fornecedor)) {
      onSelectionChange(selectedFornecedores.filter(id => id !== fornecedor.id_fornecedor));
    } else {
      onSelectionChange([...selectedFornecedores, fornecedor.id_fornecedor]);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" className="w-full justify-start text-muted-foreground">
          {selectedFornecedores.length > 0 
            ? `${selectedFornecedores.length} fornecedor(es) selecionado(s)`
            : "Selecionar fornecedores"
          }
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-4xl h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Buscar Fornecedor</DialogTitle>
        </DialogHeader>
        
        <div className="space-y-4 flex-1 flex flex-col min-h-0">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Nome</label>
              <Input
                placeholder="Digite o nome do fornecedor"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">CPF / CNPJ</label>
              <Input
                placeholder="Digite o CPF ou CNPJ"
                value={cnpj}
                onChange={(e) => setCnpj(e.target.value)}
              />
            </div>
          </div>

          <div className="flex gap-2">
            <Button onClick={handleSearch} disabled={isLoading}>
              <Search className="h-4 w-4 mr-2" />
              Pesquisar
            </Button>
            <Button variant="outline" onClick={handleClear}>
              <X className="h-4 w-4 mr-2" />
              Limpar
            </Button>
            <Button 
              variant="default" 
              onClick={handleFinalizeSelection}
              disabled={selectedFornecedores.length === 0}
              className="ml-auto"
            >
              Finalizar seleção
            </Button>
          </div>

          {isLoading && (
            <div className="text-center py-4">
              <p className="text-muted-foreground">Carregando...</p>
            </div>
          )}

          {!isLoading && fornecedores.length > 0 && (
            <div className="space-y-2 flex-1 flex flex-col min-h-0">
              <h3 className="text-sm font-medium">Resultados ({fornecedores.length})</h3>
              <div className="border rounded-md flex-1 overflow-y-auto min-h-0">
                {fornecedores.map((fornecedor) => (
                  <div
                    key={fornecedor.id_fornecedor}
                    className="p-3 border-b last:border-b-0 hover:bg-accent transition-colors"
                  >
                    <div className="flex items-center space-x-3">
                      <Checkbox
                        checked={selectedFornecedores.includes(fornecedor.id_fornecedor)}
                        onCheckedChange={() => handleSelectFornecedor(fornecedor)}
                      />
                      <div className="flex-1 cursor-pointer" onClick={() => handleSelectFornecedor(fornecedor)}>
                        <div className="font-medium">{fornecedor.nome}</div>
                        <div className="text-sm text-muted-foreground">
                          {fornecedor.cnpj_cpf}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {!isLoading && !isLoading && fornecedores.length === 0 && (nome || cnpj) && (
            <div className="text-center py-4">
              <p className="text-muted-foreground">Nenhum fornecedor encontrado</p>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
