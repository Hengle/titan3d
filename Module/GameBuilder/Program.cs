﻿// See https://aka.ms/new-console-template for more information

using EngineNS;
using System.CodeDom.Compiler;
using System.Xml.Linq;

try
{
    var projectFile = args[0];
    var csFilesPath = args[1];
    var dotnet_ver = args[2];
    var projectPath = EngineNS.IO.TtFileManager.GetBaseDirectory(projectFile, 1);
    var projName = EngineNS.IO.TtFileManager.GetPureName(projectFile);
    var assemblyFile = projectPath + $"..\\..\\binaries\\{dotnet_ver}\\" + projName + ".dll";

    System.Console.WriteLine($"TitanEngine GameBuilder: {assemblyFile}");

    EngineNS.Macross.UMacrossModule.CompileGameProject(csFilesPath, projectFile, assemblyFile);
}
catch(System.Exception e)
{
    Console.WriteLine(e.ToString());
}