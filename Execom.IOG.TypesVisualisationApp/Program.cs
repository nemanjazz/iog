using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace Execom.IOG.TypesVisualisationApp
{
    static class Program
    {
        private static string EXTERNAL_ASSEMBLIES_FOLDER = "external assemblies";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadExternalAssemblies();
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            Application.Run(new MainForm());
        }

        // this resolver works as long as the assembly is already loaded
        // with LoadFile/LoadFrom or Load(string) / Load(byte[])
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
                       where a.GetName().FullName == args.Name
                       select a).FirstOrDefault();

            if (asm == null)
                return null;

            return asm;
        }

        private static void LoadExternalAssemblies()
        {
            string path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + EXTERNAL_ASSEMBLIES_FOLDER;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo d = new DirectoryInfo(path);

            foreach (var file in d.GetFiles("*.dll"))
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                AppDomain.CurrentDomain.Load(assembly.GetName());
            }
        }
    }
}
