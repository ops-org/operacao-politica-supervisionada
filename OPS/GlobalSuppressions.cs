// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance", 
    "CA1822:Mark members as static", 
    Justification = "<Pending>", 
    Scope = "NamespaceAndDescendants", 
    Target = "OPS.WebApi"
)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance", 
    "CA1822:Mark members as static", 
    Justification = "<Pending>", 
    Scope = "member", 
    Target = "~M:OPS.Startup.Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder,Microsoft.Extensions.Hosting.IHostEnvironment)"
)]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1052:Static holder types should be Static or NotInheritable",
    Justification = "<Pending>",
    Scope = "type",
    Target = "~T:OPS.Program"
)]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Reliability", 
    "CA2007:Consider calling ConfigureAwait on the awaited task", 
    Justification = "ASP.NET Core doesn't have a SynchronizationContext. https://blog.stephencleary.com/2017/03/aspnetcore-synchronization-context.html", 
    Scope = "NamespaceAndDescendants", 
    Target = "OPS.WebApi"
)]

