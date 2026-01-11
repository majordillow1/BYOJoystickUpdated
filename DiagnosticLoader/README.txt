Run the diagnostic loader to inspect LoaderExceptions when the mod fails to load.

Usage:
  1. Build DiagnosticLoader (dotnet build) or open in Visual Studio and build.
  2. Run from command line:
     DiagnosticLoader.exe "path\to\BYOJoystick.dll"

The program loads the assembly and attempts to enumerate types. If a ReflectionTypeLoadException occurs it prints the LoaderExceptions with details to help find missing dependencies or type load issues.
