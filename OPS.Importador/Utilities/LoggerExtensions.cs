//using System.Runtime.CompilerServices;
//using Microsoft.Extensions.Logging;

//namespace OPS.Importador.Utilities;

//public static class LoggerExtensions
//{
//    public static ILogger<T> Here<T>(this ILogger<T> logger,
//        [CallerMemberName] string memberName = "",
//        [CallerFilePath] string sourceFilePath = "",
//        [CallerLineNumber] int sourceLineNumber = 0) where T : class
//    {
//        return logger
//            .ForContext("MemberName", memberName)
//            .ForContext("FilePath", sourceFilePath)
//            .ForContext("LineNumber", sourceLineNumber);
//    }
//}
