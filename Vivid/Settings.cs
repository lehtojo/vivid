using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

public enum BinaryType
{
	EXECUTABLE,
	SHARED_LIBRARY,
	STATIC_LIBRARY,
	OBJECTS,
	RAW
}

public static class Settings
{
	public static Architecture Architecture { get; set; } = RuntimeInformation.OSArchitecture;
	public static bool IsX64 => Architecture == Architecture.X64;
	public static bool IsArm64 => Architecture == Architecture.Arm64;
	public static Size Size { get; set; } = Size.QWORD;
	public const int Bytes = 8;
	public static Format Format => Size.ToFormat();
	public static Format Signed => Format.INT64;
	public static OSPlatform Target { get; set; } = OSPlatform.Windows;
	public static bool IsOptimizationEnabled { get; set; } = false;
	public static bool IsMathematicalAnalysisEnabled { get; set; } = false;
	public static bool IsGarbageCollectorEnabled { get; set; } = false;
	public static bool IsDebuggingEnabled { get; set; } = false;
	public static bool IsVerboseOutputEnabled { get; set; } = false;
	public static bool IsTargetWindows => Target == OSPlatform.Windows;
	public static bool IsTargetLinux => Target == OSPlatform.Linux;
	public static bool UseIndirectAccessTables { get; set; } = false;
	public static bool IsAssemblyOutputEnabled { get; set; } = false;
	public static bool IsLegacyAssemblyEnabled { get; set; } = false;
	public static Parse? Parse { get; set; } = null;
	public static List<string> Arguments { get; set; } = new();
	public static Dictionary<SourceFile, BinaryObjectFile> ObjectFiles { get; set; } = new Dictionary<SourceFile, BinaryObjectFile>(); // Stores all imported objects (compiler and user)
	public static List<string> UserImportedObjectFiles { get; set; } = new(); // Stores the object files added by the user
	public static List<SourceFile> SourceFiles { get; set; } = new(); // Stores compiler generated information about the source files specified by the user
	public static List<string> Libraries { get; set; } = new(); // Stores the libraries needed to link the program
	public static string OutputName { get; set; } = "v"; // Stores the name of the output file
	public static BinaryType OutputType { get; set; } = BinaryType.EXECUTABLE; // Stores the output type of the program (executable, library, etc.)
	public static bool LinkObjects { get; set; } = false; // Whether to link the object files produced by the compiler (relevant only in textual assembly mode)
	public static bool Time { get; set; } = false; // Whether to print the time taken to execute various parts of the compiler.
	public static bool Rebuild { get; set; } = false; // Whether to rebuild all the specified source files
	public static bool Service { get; set; } = false; // Whether to start a compiler service for code completion
	public static List<string> Filenames { get; set; } = new(); // Stores the user-defined source files to load
	public static bool TextualAssembly { get; set; } = false; // Stores whether textual assembly mode is enabled

	public static FunctionImplementation? AllocationFunction { get; set; }
	public static FunctionImplementation? DeallocationFunction { get; set; }
	public static FunctionImplementation? InheritanceFunction { get; set; }
	public static FunctionImplementation? InitializationFunction { get; set; }
	public static List<string> IncludedFolders { get; set; } = new();

	public const string VERSION = "1.0";
	public const string ASSEMBLY_EXTENSION = ".asm";
	public const string VIVID_EXTENSION = ".v";
	public const string POSITIVE_INFINITY_CONSTANT = "POSITIVE_INFINITY";
	public const string NEGATIVE_INFINITY_CONSTANT = "NEGATIVE_INFINITY";

	public static void Initialize()
	{
		Architecture = RuntimeInformation.OSArchitecture;
		Size = Size.QWORD;
		Target = OSPlatform.Windows;
		IsOptimizationEnabled = false;
		IsMathematicalAnalysisEnabled = false;
		IsGarbageCollectorEnabled = false;
		IsDebuggingEnabled = false;
		IsVerboseOutputEnabled = false;
		UseIndirectAccessTables = false;
		IsAssemblyOutputEnabled = false;
		IsLegacyAssemblyEnabled = false;
		Parse = null;
		Arguments = new();
		ObjectFiles = new Dictionary<SourceFile, BinaryObjectFile>();
		UserImportedObjectFiles = new();
		SourceFiles = new();
		Libraries = new();
		OutputName = "v";
		OutputType = BinaryType.EXECUTABLE;
		LinkObjects = false;
		Time = false;
		Rebuild = false;
		Service = false;
		Filenames = new();
		TextualAssembly = false;
		AllocationFunction = null;
		DeallocationFunction = null;
		InheritanceFunction = null;
		InitializationFunction = null;
		IncludedFolders = new List<string> { Environment.CurrentDirectory };
	}
}