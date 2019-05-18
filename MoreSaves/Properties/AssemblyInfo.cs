using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Permissions;

[assembly:
    Debuggable(DebuggableAttribute.DebuggingModes.Default        | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints |
        DebuggableAttribute.DebuggingModes.EnableEditAndContinue |
        DebuggableAttribute.DebuggingModes.DisableOptimizations)]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]