using System;
using Mono.Cecil;
using System.Collections.Generic;

namespace ForwardersExport {
	class MainClass {
		public static void Main (string [] args)
		{
			var existing = GetForwarders ("/Users/marek/mono/lib/mono/4.5/Facades/System.Runtime.Extensions.dll");

			var asm = AssemblyDefinition.ReadAssembly ("/Users/marek/Downloads/00/System.Runtime.Extensions.dll");

			foreach (var et in asm.MainModule.ExportedTypes)
			{
				if (existing.Contains (et.FullName))
					continue;

				if (et.FullName.Contains ("/"))
					continue;

				Console.WriteLine ($"[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute (typeof ({Format(et.FullName)}))]");
			}

			foreach (var et in asm.MainModule.Types) {
				if (existing.Contains (et.FullName))
					continue;

				if (et.FullName.Contains ("/"))
					continue;

				if (!et.IsPublic)
					continue;

				Console.WriteLine ($"[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute (typeof ({Format (et.FullName)}))]");
			}
		}

		static string Format (string input)
		{
			int arity_start = input.IndexOf ('`');
			if (arity_start > 0) {

				var arity = int.Parse (input.Substring (arity_start + 1, 1));
				return input.Replace ("`" + arity.ToString (), $"<{string.Join(",", new string[arity])}>");
			}

			return input;
		}

		static HashSet<string> GetForwarders (string file)
		{
			var hs = new HashSet<string> ();

			var asm = AssemblyDefinition.ReadAssembly (file);

			foreach (var et in asm.MainModule.ExportedTypes) {
				hs.Add (et.FullName);
			}

			return hs;
		}
	}
}
