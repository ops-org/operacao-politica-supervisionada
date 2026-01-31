import { Button } from "@/components/ui/button";
import { AlertTriangle, RefreshCw } from "lucide-react";

interface ErrorMessageProps {
    title?: string;
    message?: string;
    onRetry?: () => void;
    retryText?: string;
    className?: string;
    icon?: React.ReactNode;
}

export function ErrorMessage({
    title = "Erro ao carregar dados",
    message = "Por favor, tente novamente.",
    onRetry,
    retryText = "Tentar novamente",
    className = "",
    icon
}: ErrorMessageProps) {
    return (
        <div className={`min-h-screen flex flex-col bg-gray-50 ${className}`}>
            <div className="flex-1 container mx-auto px-4 py-8">
                <div className="max-w-2xl mx-auto">
                    <div className="relative overflow-hidden rounded-2xl bg-red-50 border border-red-200 shadow-lg">
                        <div className="relative p-8 text-center">
                            <div className="w-16 h-16 bg-red-500 rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg">
                                {icon || <AlertTriangle className="w-8 h-8 text-white" />}
                            </div>
                            <h3 className="text-xl font-bold text-red-800 mb-2">{title}</h3>
                            <p className="text-red-600 mb-6">{message}</p>
                            {onRetry && (
                                <Button
                                    onClick={onRetry}
                                    className="bg-red-500 hover:bg-red-600 text-white font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all duration-300"
                                >
                                    <RefreshCw className="w-4 h-4 mr-2" />
                                    {retryText}
                                </Button>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
