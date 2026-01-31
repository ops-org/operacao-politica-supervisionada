import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface ParlamentarDetalheSkeletonProps {
    type: "deputado-federal" | "deputado-estadual" | "senador";
}

export const ParlamentarDetalheSkeleton = ({ type }: ParlamentarDetalheSkeletonProps) => {
    const isState = type === "deputado-estadual";
    const isFederal = type === "deputado-federal";

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb Skeleton */}
                <div className="flex items-center gap-2 mb-8">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-3 w-3" />
                    <Skeleton className="h-4 w-32" />
                </div>

                <div className="space-y-8">
                    {/* Profile Card Skeleton */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden border-t-4 border-t-primary/20">
                        <div className="p-8">
                            <div className="flex flex-col md:flex-row gap-8 items-center md:items-start text-center md:text-left">
                                {/* Avatar Skeleton */}
                                <div className="relative group">
                                    <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-10"></div>
                                    <Skeleton className="h-40 w-32 rounded-2xl border-2 border-background shadow-2xl relative z-10" />
                                </div>

                                {/* Main Info Skeleton */}
                                <div className="flex-1 space-y-6 pt-2">
                                    <div className="space-y-3">
                                        <Skeleton className="h-10 w-72 mx-auto md:mx-0" />
                                        <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                            <Skeleton className="h-6 w-16 rounded-lg opacity-50" />
                                            <Skeleton className="h-6 w-12 rounded-lg opacity-50" />
                                            <Skeleton className="h-6 w-20 rounded-lg opacity-50" />
                                        </div>
                                        <Skeleton className="h-4 w-48 mx-auto md:mx-0 opacity-40" />
                                    </div>
                                </div>

                                {/* Total Value Skeleton */}
                                <div className="text-center md:text-right lg:min-w-[280px]">
                                    <div className="bg-primary/5 rounded-2xl p-6 border border-primary/10">
                                        <Skeleton className="h-3 w-32 ml-auto mb-2 opacity-40" />
                                        <Skeleton className="h-10 w-48 ml-auto" />
                                        <Skeleton className="h-2 w-40 ml-auto mt-2 opacity-20" />
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar Skeleton */}
                        <div className="border-t border-border/50 bg-muted/20 px-8 py-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                                {[1, 2, 3, 4].map((i) => (
                                    <div key={i} className="flex items-center gap-3">
                                        <Skeleton className="h-8 w-8 rounded-lg opacity-50" />
                                        <div className="space-y-1">
                                            <Skeleton className="h-2 w-16 opacity-30" />
                                            <Skeleton className="h-3 w-32 opacity-60" />
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </Card>

                    {/* Summary Value Cards Skeleton (Federal and Senator) */}
                    {!isState && (
                        <div className={`grid grid-cols-1 md:grid-cols-2 ${isFederal ? "lg:grid-cols-4" : ""} gap-6`}>
                            {[1, 2, 3, 4].slice(0, isFederal ? 4 : 2).map((i) => (
                                <Card key={i} className="shadow-lg border-0 bg-card/50 backdrop-blur-sm overflow-hidden">
                                    <CardContent className="p-6">
                                        <div className="flex items-center justify-between">
                                            <div className="space-y-2">
                                                <Skeleton className="h-2 w-20 opacity-40" />
                                                <Skeleton className="h-6 w-32" />
                                            </div>
                                            <Skeleton className="h-12 w-12 rounded-2xl opacity-10" />
                                        </div>
                                    </CardContent>
                                </Card>
                            ))}
                        </div>
                    )}

                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Personal Information Card Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-10 w-10 rounded-xl opacity-50" />
                                    <Skeleton className="h-6 w-40" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    {[1, 2, 3, 4, 5, 6].map((i) => (
                                        <div key={i} className="flex items-start gap-2">
                                            <Skeleton className="h-4 w-4 rounded opacity-40 mt-1" />
                                            <div className="space-y-2">
                                                <Skeleton className="h-2 w-20 opacity-30" />
                                                <Skeleton className="h-4 w-32 opacity-70" />
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>

                        {/* Chart Card Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-10 w-10 rounded-xl opacity-50" />
                                    <Skeleton className="h-6 w-32" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <Skeleton className="h-[300px] w-full rounded-lg opacity-40" />
                                <div className="flex justify-center gap-6 mt-6">
                                    {[1, 2, 3, 4].map((i) => (
                                        <div key={i} className="flex items-center gap-2">
                                            <Skeleton className="h-3 w-3 rounded-full opacity-30" />
                                            <Skeleton className="h-2 w-20 opacity-30" />
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Table Card Skeletons */}
                    <div className="grid gap-8 lg:grid-cols-2 mb-8">
                        {[1, 2].map((i) => (
                            <Card key={i} className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <Skeleton className="h-10 w-10 rounded-xl opacity-50" />
                                            <Skeleton className="h-6 w-48" />
                                        </div>
                                        <Skeleton className="h-8 w-24 rounded-lg opacity-80" />
                                    </div>
                                </CardHeader>
                                <CardContent className="p-0">
                                    <div className="divide-y divide-border/50">
                                        {[1, 2, 3, 4, 5].map((j) => (
                                            <div key={j} className="p-4 flex justify-between items-center gap-4">
                                                <div className="space-y-2 flex-1">
                                                    <Skeleton className="h-4 w-[80%] opacity-80" />
                                                    <Skeleton className="h-3 w-32 opacity-30" />
                                                </div>
                                                <Skeleton className="h-5 w-24 opacity-80" />
                                            </div>
                                        ))}
                                    </div>
                                </CardContent>
                            </Card>
                        ))}
                    </div>
                </div>
            </main>
        </div>
    );
};
