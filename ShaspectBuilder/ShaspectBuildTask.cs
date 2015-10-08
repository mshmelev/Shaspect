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