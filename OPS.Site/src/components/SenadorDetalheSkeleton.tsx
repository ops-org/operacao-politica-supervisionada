import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export const SenadorDetalheSkeleton = () => {
    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb Skeleton */}
                <div className="flex items-center gap-2 mb-8">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-3 w-3" />
                    <Skeleton className="h-4 w-32" />
                </div>

                <div className="grid gap-8">
                    {/* Profile Card Skeleton */}
                    <Card className="shadow-xl border-0 bg-card/80 backdrop-blur-md overflow-hidden">
                        <div className="p-8">
                            <div className="flex flex-col md:flex-row gap-8 items-center md:items-start text-center md:text-left">
                                {/* Avatar Skeleton */}
                                <div className="relative group">
                                    <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-20"></div>
                                    <Skeleton className="h-40 w-32 rounded-2xl border-2 border-background shadow-2xl relative z-10" />
                                </div>

                                {/* Main Info Skeleton */}
                                <div className="flex-1 space-y-6 pt-2">
                                    <div className="space-y-2">
                                        <Skeleton className="h-10 w-64 mx-auto md:mx-0" />
                                        <Skeleton className="h-4 w-48 mx-auto md:mx-0 opacity-60" />
                                    </div>

                                    <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                        <Skeleton className="h-7 w-20 rounded-full" />
                                        <Skeleton className="h-7 w-16 rounded-full" />
                                        <Skeleton className="h-7 w-24 rounded-full" />
                                    </div>

                                    <div className="flex flex-wrap justify-center md:justify-start gap-4">
                                        <Skeleton className="h-5 w-32" />
                                        <Skeleton className="h-5 w-40" />
                                    </div>
                                </div>

                                {/* Action Buttons Skeleton */}
                                <div className="flex md:flex-col gap-3">
                                    <Skeleton className="h-10 w-32 rounded-lg" />
                                    <Skeleton className="h-10 w-40 rounded-lg" />
                                </div>
                            </div>
                        </div>
                    </Card>

                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Summary Chart Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-10 w-10 rounded-xl" />
                                    <Skeleton className="h-6 w-40" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <Skeleton className="h-[300px] w-full rounded-lg" />
                            </CardContent>
                        </Card>

                        {/* Recent Activity or Info Skeleton */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <Skeleton className="h-10 w-10 rounded-xl" />
                                    <Skeleton className="h-6 w-32" />
                                </div>
                            </CardHeader>
                            <CardContent className="p-6 space-y-6">
                                {[1, 2, 3, 4, 5].map((i) => (
                                    <div key={i} className="space-y-2">
                                        <Skeleton className="h-3 w-20 opacity-60" />
                                        <Skeleton className="h-5 w-full" />
                                    </div>
                                ))}
                            </CardContent>
                        </Card>
                    </div>

                    {/* Tables Card Skeleton */}
                    <div className="grid gap-8 lg:grid-cols-2">
                        {[1, 2].map((i) => (
                            <Card key={i} className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <Skeleton className="h-10 w-10 rounded-xl" />
                                            <Skeleton className="h-6 w-48" />
                                        </div>
                                        <Skeleton className="h-8 w-24 rounded-lg" />
                                    </div>
                                </CardHeader>
                                <CardContent className="p-0">
                                    <div className="divide-y divide-border/50">
                                        {[1, 2, 3, 4, 5].map((j) => (
                                            <div key={j} className="p-4 flex justify-between items-center gap-4">
                                                <div className="space-y-2">
                                                    <Skeleton className="h-4 w-48" />
                                                    <Skeleton className="h-3 w-32 opacity-60" />
                                                </div>
                                                <Skeleton className="h-5 w-24" />
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
