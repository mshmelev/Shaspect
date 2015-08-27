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

                // TODO: on class level
                // TODO: nesting (with overwriting) on assembly/class/method/(get/set property) levels
                // TODO: passing arguments, return value
                // TODO: specifying targets (properties, methods). Now done only for ctor.
                // TODO: specifying targets by name (Namespace1.Namespace2.Class.*).
                // TODO: inheritance
                // TODO: interfaces
                // TODO: aspect in a referenced assembly (assembly resolver)
                // TODO: handle signed assemblies
                // TODO: aspect on parameters (e.g. check parameter is not null)
                // TODO: optimize performance of the Builder
                // TODO: process only once (check if there's Shaspect.Implementation namespace there

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