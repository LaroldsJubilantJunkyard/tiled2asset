using System;
using System.Collections.Generic;

namespace tiled2Asset
{
    public static class Utils
    {
        /// <summary>
        /// Runs an external process with the given arguments and returns the standard output
        /// </summary>
        /// <param name="path">The ful path of the external process</param>
        /// <param name="arguments">Arguments to pass to the external process.</param>
        /// <returns>Standard output from the external process.</returns>
        public static string RunProcess(string path, string arguments)
        {

            string output;

            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = path;
                pProcess.StartInfo.Arguments = arguments; //argument
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                output = pProcess.StandardOutput.ReadToEnd(); //The output result
                pProcess.WaitForExit();
            }

            Console.WriteLine(output);

            return output;
        }

        /// <summary>
        /// Converts a dictionary to a string that can be used for declaring constant structs in c
        /// </summary>
        /// <param name="dict"></param>
        /// <returns>All of the key value pairs, comma-delimited, surrounded by squiggly brackets</returns>
        public static String GetStructDefinition(Dictionary<string, Object> dict)
        {

            string structString = "{";
            List<string> keys = new List<string>(dict.Keys);

            // For each item in the dictionary
            for (int i = 0; i < dict.Count; i++)
            {
                // Concatenate the key and value
                structString += "." + keys[i] + "=" + dict[keys[i]];

                // Append a comma
                // Except on the last item
                if (i + 1 < dict.Count) structString += ", ";

            }

            structString += "}";

            return structString;
        }
    }


}
