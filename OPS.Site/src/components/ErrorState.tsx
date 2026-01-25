import { AlertCircle, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Header } from "./Header";
import { Footer } from "./Footer";

interface ErrorStateProps {
    title?: string;
    message?: string;
    onRetry?: () => void;
    showRetryButton?: boolean;
}

export const ErrorState = ({
    title = "Erro ao carregar dados",
    message = "Não foi possível carregar as informações solicitadas. Tente novamente mais tarde.",
    onRetry,
    showRetryButton = false
}: ErrorStateProps) => {
    return (
        <div className="min-h-screen bg-background">
            <Header />
            <main className="container mx-auto px-4 py-8">
                <div className="flex items-center justify-center min-h-[400px] px-4">
                    <Card className="max-w-md w-full border-0 shadow-lg">
                        <CardContent className="p-8 text-center space-y-6">
                            <div className="flex justify-center">
                                <div className="w-16 h-16 bg-destructive/10 rounded-full flex items-center justify-center">
                                    <AlertCircle className="w-8 h-8 text-destructive" />
                                </div>
                            </div>

                            <div className="space-y-2">
                                <h2 className="text-xl font-semibold text-foreground">
                                    {title}
                                </h2>
                                <p className="text-muted-foreground leading-relaxed">
                                    {message}
                                </p>
                            </div>

                            {showRetryButton && onRetry && (
                                <Button
                                    onClick={onRetry}
                                    className="w-full sm:w-auto"
                                    variant="outline"
                                >
                                    <RefreshCw className="w-4 h-4 mr-2" />
                                    Tentar novamente
                                </Button>
                            )}
                        </CardContent>
                    </Card>
                </div>
            </main>
            <Footer />
        </div >
    );
};
