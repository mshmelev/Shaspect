using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


namespace Shaspect.Builder
{
    public class ShaspectBuildTask : Task
    {
        [Required]
        public string AssemblyFile { get; set; }

        [Required]
        public string References { get; set; }

        public string KeyFile { get; set; }


        public override bool Execute()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var injector = new AspectsInjector (AssemblyFile, References);
                if (injector.ProcessAssembly() == 0)
                {
                    Log.LogWarning (
                        "No aspects detected in {0}. You can uninstall Shaspect package for this assembly to speed up the build.",
                        AssemblyFile);
                }

                // TODO: return value with yield
                // TODO: return value changing (args.SetReturnValue(...))
                // TODO: generics as arguments and returning values
                // TODO: test aspect on static and instance constructors (they're compiled differently)
                // TODO: OnInit for aspects
                // TODO: implement OnException, OnExit
                // TODO: specifying targets (properties, methods). Now done only for ctor. Don't forget about Exclude.
                // TODO: specifying targets by name (Namespace1.Namespace2.Class.*). Don't forget about Exclude.
                // TODO: Implement priorities (cut-through across all the nesting levels). Don't forget about Exclude.
                // TODO: inheritance
                // TODO: aspect on interfaces
                // TODO: aspect in a referenced assembly (assembly resolver); resolving using configuration from app.config
                // TODO: handle signed assemblies
                // TODO: aspect on parameters (e.g. check parameter is not null)
                // TODO: optimize performance of the Builder
                // TODO: process only once (check if there's Shaspect.Implementation namespace there
                // TODO: support changing of method arguments (something like args.SetArgument (0, "12345") )

                stopwatch.Stop();
                Log.LogMessage ("ShaspectBuildTask took {0}ms", stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (ApplicationException ex)
            {
                Log.LogError (ex.Message);
            }

            return false;
        }


    }
}