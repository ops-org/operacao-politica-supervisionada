import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

export const DeputadoDetalheSkeleton = () => {
    return (
        <div className="min-h-screen bg-background">
            <main className="container mx-auto px-4 py-8">
                <div className="space-y-8">
                    {/* Header Skeleton */}
                    <div className="text-center space-y-4">
                        <div className="flex items-center justify-center gap-3 mb-4">
                            <div className="h-8 w-8 bg-muted rounded-lg animate-pulse"></div>
                            <div className="h-10 w-64 bg-muted rounded-lg animate-pulse"></div>
                        </div>
                        <div className="h-5 w-96 bg-muted rounded-lg animate-pulse mx-auto"></div>
                    </div>

                    {/* Profile Card Skeleton */}
                    <Card className="shadow-md border-0 bg-white overflow-hidden">
                        {/* Header Section Skeleton */}
                        <div className="bg-gradient-to-r from-blue-600 to-blue-700 p-6">
                            <div className="flex flex-col md:flex-row gap-6 items-center md:items-start">
                                {/* Avatar Skeleton */}
                                <div className="flex-shrink-0">
                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white/30">
                                        <AvatarFallback className="rounded-xl bg-white/30 animate-pulse"></AvatarFallback>
                                    </Avatar>
                                </div>

                                {/* Main Info Skeleton */}
                                <div className="flex-1 text-center md:text-left space-y-3">
                                    <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                                        <div className="h-8 w-48 bg-white/30 rounded-lg animate-pulse"></div>
                                        <div className="h-6 w-32 bg-white/30 rounded-lg animate-pulse"></div>
                                    </div>
                                    <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                        <div className="h-6 w-32 bg-white/30 rounded-lg animate-pulse"></div>
                                        <div className="h-6 w-24 bg-white/30 rounded-lg animate-pulse"></div>
                                        <div className="h-6 w-20 bg-white/30 rounded-lg animate-pulse"></div>
                                    </div>
                                    <div className="h-4 w-48 bg-white/30 rounded-lg animate-pulse"></div>
                                </div>

                                {/* Total CEAP Skeleton */}
                                <div className="text-center md:text-right space-y-2">
                                    <div className="bg-white/30 rounded-lg p-4 backdrop-blur-sm">
                                        <div className="h-4 w-24 bg-white/30 rounded-lg animate-pulse mb-2"></div>
                                        <div className="h-8 w-32 bg-white/30 rounded-lg animate-pulse"></div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar Skeleton */}
                        <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                <div className="flex items-center gap-2">
                                    <div className="h-4 w-4 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-16 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-24 bg-muted rounded animate-pulse"></div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="h-4 w-4 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-20 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-28 bg-muted rounded animate-pulse"></div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="h-4 w-4 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-12 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-40 bg-muted rounded animate-pulse"></div>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="h-4 w-4 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-16 bg-muted rounded animate-pulse"></div>
                                    <div className="h-4 w-32 bg-muted rounded animate-pulse"></div>
                                </div>
                            </div>
                        </div>
                    </Card>

                    {/* Charts Section Skeleton */}
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                        <Card className="shadow-md border-0 bg-white">
                            <CardHeader>
                                <CardTitle>
                                    <div className="h-6 w-40 bg-muted rounded-lg animate-pulse"></div>
                                </CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="h-64 bg-muted rounded-lg animate-pulse"></div>
                            </CardContent>
                        </Card>
                        <Card className="shadow-md border-0 bg-white">
                            <CardHeader>
                                <CardTitle>
                                    <div className="h-6 w-40 bg-muted rounded-lg animate-pulse"></div>
                                </CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="space-y-3">
                                    {[1, 2, 3, 4, 5].map((i) => (
                                        <div key={i} className="flex items-center justify-between">
                                            <div className="h-4 w-32 bg-muted rounded-lg animate-pulse"></div>
                                            <div className="h-4 w-24 bg-muted rounded-lg animate-pulse"></div>
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Fornecedores Table Skeleton */}
                    <Card className="shadow-md border-0 bg-white">
                        <CardHeader>
                            <CardTitle>
                                <div className="h-6 w-48 bg-muted rounded-lg animate-pulse"></div>
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="overflow-x-auto">
                                <table className="w-full">
                                    <thead>
                                        <tr className="border-b">
                                            <th className="text-left p-2">
                                                <div className="h-4 w-24 bg-muted rounded animate-pulse"></div>
                                            </th>
                                            <th className="text-left p-2">
                                                <div className="h-4 w-32 bg-muted rounded animate-pulse"></div>
                                            </th>
                                            <th className="text-left p-2">
                                                <div className="h-4 w-20 bg-muted rounded animate-pulse"></div>
                                            </th>
                                            <th className="text-left p-2">
                                                <div className="h-4 w-28 bg-muted rounded animate-pulse"></div>
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {[1, 2, 3, 4, 5].map((i) => (
                                            <tr key={i} className="border-b">
                                                <td className="p-2">
                                                    <div className="h-4 w-32 bg-muted rounded animate-pulse"></div>
                                                </td>
                                                <td className="p-2">
                                                    <div className="h-4 w-40 bg-muted rounded animate-pulse"></div>
                                                </td>
                                                <td className="p-2">
                                                    <div className="h-4 w-24 bg-muted rounded animate-pulse"></div>
                                                </td>
                                                <td className="p-2">
                                                    <div className="h-4 w-28 bg-muted rounded animate-pulse"></div>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </main>
        </div>
    );
};
