import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { ChevronDown, Search, Trash } from "lucide-react";
import { useState, useMemo, useEffect } from "react";

interface MultiSelectDropdownProps {
  items: Array<{ 
    id: number | string; 
    text: string;
    image?: string;
    help_text?: string;
  }>;
  placeholder?: string;
  selectedItems?: string[];
  onSelectionChange?: (selectedIds: string[]) => void;
}

export const MultiSelectDropdown = ({
  items,
  placeholder = "Selecione itens",
  selectedItems = [],
  onSelectionChange,
}: MultiSelectDropdownProps) => {
  const [selectedIds, setSelectedIds] = useState<string[]>(selectedItems);
  const [searchTerm, setSearchTerm] = useState("");

  // Sync internal state with prop changes
  useEffect(() => {
    setSelectedIds(selectedItems);
  }, [selectedItems]);

  const filteredItems = useMemo(() => {
    if (!searchTerm) return items;
    
    const normalizeText = (text: string) => {
      return text
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');
    };
    
    const searchNormalized = normalizeText(searchTerm);
    
    return items.filter(item => {
      const textNormalized = normalizeText(item.text);
      const helpTextNormalized = item.help_text ? normalizeText(item.help_text) : '';
      
      return textNormalized.includes(searchNormalized) || 
             helpTextNormalized.includes(searchNormalized);
    });
  }, [items, searchTerm]);

  const handleCheckedChange = (itemId: string, checked: boolean) => {
    const newSelectedIds = checked
      ? [...selectedIds, itemId]
      : selectedIds.filter(id => id !== itemId);
    
    setSelectedIds(newSelectedIds);
    onSelectionChange?.(newSelectedIds);
  };

  const handleClearAll = (e: React.MouseEvent) => {
    e.stopPropagation();
    setSelectedIds([]);
    onSelectionChange?.([]);
  };

  const getDisplayText = () => {
    if (selectedIds.length === 0) return placeholder;
    return `${selectedIds.length} item(ns) selecionado(s)`;
  };

  return (
    <Popover>
      <PopoverTrigger asChild>
        <div className="flex items-center space-x-2">
          <Button variant="outline" className="p-3 flex-1 justify-between">
            <span className="truncate">{getDisplayText()}</span>
            <ChevronDown className="h-4 w-4 opacity-50" />
          </Button>
          {selectedIds.length > 0 && (
            <Button
              variant="outline"
              size="sm"
              onClick={handleClearAll}
              className="h-10 w-10 p-0"
              title="Limpar seleção"
            >
              <Trash className="h-4 w-4" />
            </Button>
          )}
        </div>
      </PopoverTrigger>
      <PopoverContent className="w-[500px] max-w-[calc(100vw-2rem)] p-0" align="start">
        <div className="p-3 border-b">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Buscar..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>
        <div className="max-h-[30vh] overflow-auto p-3 space-y-2">
          {filteredItems.length === 0 ? (
            <div className="text-center text-muted-foreground py-4">
              Nenhum item encontrado
            </div>
          ) : (
            filteredItems.map((item) => (
              <div 
                key={item.id} 
                className="flex items-start space-x-3 p-2 rounded hover:bg-muted/50 cursor-pointer"
                onClick={() => {
                  const isChecked = selectedIds.includes(item.id.toString());
                  handleCheckedChange(item.id.toString(), !isChecked);
                }}
              >
                <Checkbox
                  id={`item-${item.id}`}
                  checked={selectedIds.includes(item.id.toString())}
                  className="mt-0.5"
                />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      {item.image && (
                        <img 
                          src={item.image.replace("/img/", "//static.ops.org.br/")} 
                          alt={item.text}
                          className="w-8 h-8 rounded object-scale-down flex-shrink-0"
                        />
                      )}
                      <label 
                        htmlFor={`item-${item.id}`}
                        className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {item.text}
                      </label>
                    </div>
                    {item.help_text && (
                      <p className="text-xs text-muted-foreground leading-relaxed text-right ml-4">
                        {item.help_text}
                      </p>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </PopoverContent>
    </Popover>
  );
};
