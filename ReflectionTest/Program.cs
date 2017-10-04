using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace csTestApp
{
    public class Program
    {
        private static string fileLocation = ".\\extensions";

        static void Main(string[] args)
        => new Program().RealMain(args);

        private async void RealMain(string[] args)
        {
            //create new dir if it doesnt exist
            if (!Directory.Exists(fileLocation))
                Directory.CreateDirectory(fileLocation);

            var files = Directory.EnumerateFiles(fileLocation, "*.dll");

            List<Assembly> assemblies = new List<Assembly>();

            foreach (var file in files)
            {
                assemblies.Add(Assembly.LoadFrom(file));
            }

            foreach (var assembly in assemblies)
            {
                using (var resourceStream = assembly.GetManifestResourceStream(assembly.GetName().Name + (".info.json")))
                {
                    using (var streamReader = new StreamReader(resourceStream))
                    {
                        var json = JObject.Parse(await streamReader.ReadToEndAsync());
                        var mainClass = (string) json["mainClass"];
                        var mainMethod = (string) json["mainMethod"];

                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.Name == mainClass)
                            {
                                Console.WriteLine("Running extension: " + type.Name);
                                
                                var mainInst = Activator.CreateInstance(type);
                       
                                var aMain = type.GetMethod(mainMethod);

                                aMain.Invoke(mainInst, null);
                            }
                        }
                    }
                }
            }
        }
    }
}
