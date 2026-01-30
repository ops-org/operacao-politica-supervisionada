import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export const FornecedorDetalheSkeleton = () => {
    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb Skeleton */}
                <div className="flex items-center gap-2 mb-8">
                    <Skeleton className="h-4 w-16" />
                    <Skeleton className="h-4 w-4" />
                    <Skeleton className="h-4 w-32" />
                </div>

                <div className="space-y-8">
                    {/* Profile Card Skeleton */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden border-t-4 border-t-muted">
                        <div className="p-8">
                            <div className="flex flex-col md:flex-row gap-8 items-center md:items-start">
                                {/* Icon Section Skeleton */}
                                <div className="flex-shrink-0">
                                    <div className="relative">
                                        <div className="absolute -inset-1 bg-muted rounded-2xl blur opacity-25"></div>
                                        <Skeleton className="relative w-28 h-28 rounded-2xl shadow-inner" />
                                    </div>
                                </div>

                                {/* Main Info Section Skeleton */}
                                <div className="flex-1 space-y-4 text-center md:text-left">
                                    <div className="space-y-2">
                                        <Skeleton className="h-10 w-96 mx-auto md:mx-0" />
                                        <div className="flex flex-wrap justify-center md:justify-start gap-3 items-center">
                                            <Skeleton className="h-6 w-40" />
                                            <Skeleton className="h-6 w-24" />
                                            <Skeleton className="h-6 w-32" />
                                        </div>
                                    </div>
                                    <Skeleton className="h-4 w-48 mx-auto md:mx-0 opacity-60" />
                                </div>

                                {/* Total Recebimentos Skeleton */}
                                <div className="lg:min-w-[280px]">
                                    <Skeleton className="h-28 w-full rounded-2xl shadow-xl" />
                                </div>
                            </div>

                            {/* Detailed Info Grid Skeleton */}
                            <div className="mt-8 pt-8 border-t border-border/50 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                                {[1, 2, 3, 4].map((i) => (
                                    <div key={i} className="flex items-center gap-2">
                                        <Skeleton className="h-4 w-4 rounded-full" />
                                        <Skeleton className="h-4 w-full" />
                                    </div>
                                ))}
                                <div className="lg:col-span-4 flex items-center gap-2 mt-2">
                                    <Skeleton className="h-4 w-4 rounded-full" />
                                    <Skeleton className="h-4 w-3/4" />
                                </div>
                            </div>
                        </div>
                    </Card>

                    {/* Detailed Info Card Skeleton */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                        <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b p-6">
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-12 w-12 rounded-xl" />
                                    <Skeleton className="h-6 w-48" />
                                </div>
                                <Skeleton className="h-8 w-24 rounded-lg" />
                            </div>
                        </CardHeader>
                    </Card>

                    {/* Charts and Tables Skeleton */}
                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Chart Card Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b p-6">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-12 w-12 rounded-xl" />
                                    <Skeleton className="h-6 w-40" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <Skeleton className="h-[300px] w-full" />
                            </CardContent>
                        </Card>

                        {/* Table Card Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b p-6">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-12 w-12 rounded-xl" />
                                    <Skeleton className="h-6 w-64" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-0">
                                <div className="divide-y divide-border/50">
                                    {[1, 2, 3, 4, 5].map((i) => (
                                        <div key={i} className="p-4 flex justify-between items-center gap-4">
                                            <div className="space-y-2 flex-1">
                                                <Skeleton className="h-4 w-3/4" />
                                                <Skeleton className="h-3 w-1/2 opacity-60" />
                                            </div>
                                            <Skeleton className="h-5 w-24" />
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </main>
        </div>
    );
};
