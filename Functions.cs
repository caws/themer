using System;
using System.Collections.Generic;
using System.IO;

namespace Themer
{
    class Functions
    {
        public static void organizeTheme(string themeFolder, string projectFolder, string themeName)
        {
            //Creating theme folder inside the project's vendor folder 
            Console.WriteLine("Creating theme folder inside the project's vendor folder...");
            System.IO.Directory.CreateDirectory(projectFolder + "\\vendor\\assets\\" + themeName);

            //Creating theme folder inside the projects app\assets\javascripts\
            Console.WriteLine("Creating theme folder inside the projects app\\assets\\javascripts\\...");
            System.IO.Directory.CreateDirectory(projectFolder + "\\app\\assets\\javascripts\\" + themeName);

            string javascriptsFolder = projectFolder + "\\app\\assets\\javascripts\\" + themeName + "\\";

            //Creating theme folder inside the projects app\assets\stylesheets\
            Console.WriteLine("Creating theme folder inside the projects app\\assets\\stylesheets\\...");
            System.IO.Directory.CreateDirectory(projectFolder + "\\app\\assets\\stylesheets\\" + themeName);

            string styleSheetsFolder = projectFolder + "\\app\\assets\\stylesheets\\" + themeName + "\\";

            //Copying theme resources to vendor\assets folder inside rails project
            Console.Write("Copying files to vendor\assets folder inside rails project (might take a while)...");
            DirectoryCopy(themeFolder, projectFolder + "\\vendor\\assets\\" + themeName, true, themeName);
            Console.WriteLine("Done");

            singleFileaddToPipeline(projectFolder, themeName, themeName + ".css");

            singleFileaddToPipeline(projectFolder, themeName, themeName + ".js");

            //Search html files for references to js and css files
            DirectoryInfo dir = new DirectoryInfo(themeFolder);
            FileInfo[] files = dir.GetFiles();

            //Creating css import file inside styleshets folder
            Console.WriteLine("Creating css import file inside styleshets folder.");
            System.IO.File.WriteAllText(styleSheetsFolder.Replace(themeName + "\\", "") + themeName + ".css", "/*\n *= require_tree ./" + themeName + "/.\n */");

            //Creating js import file inside styleshets folder
            Console.WriteLine("Creating js import file inside javascripts folder.");
            System.IO.File.WriteAllText(javascriptsFolder.Replace(themeName + "\\", "") + themeName + ".js", "");

            foreach (FileInfo file in files)
            {
                if (Path.GetExtension(file.ToString()) == ".html")
                {
                    //Now we open the html
                    //Console.WriteLine(themeFolder+"\\"+ file);
                    var styles = returnStylesheetImports(themeFolder + "\\" + file);
                    var javascripts = returnJavascriptImports(themeFolder + "\\" + file);

                    string styleFile = "";
                    string scriptFile = "";

                    string fileCreate = Path.GetFileNameWithoutExtension(file.ToString());

                    foreach (var style in styles)
                    {
                        styleFile = "/*\n";
                        styleFile += "*= require " + style + "\n";
                        styleFile += "*/";
                        if (!File.Exists(styleSheetsFolder + Path.GetFileName(styleSheetsFolder + style + ".css")))
                        {
                            Console.WriteLine("Creating: " + Path.GetFileName(styleSheetsFolder + style + ".css"));
                            System.IO.File.WriteAllText(styleSheetsFolder + Path.GetFileName(styleSheetsFolder + style + ".css"), styleFile);
                        }
                        else
                        {
                            //File already exists.
                        }
                        addToPipeline(projectFolder, themeName, Path.GetFileName(styleSheetsFolder + style + ".css"));
                        styleFile = "";
                    }

                    foreach (var javascript in javascripts)
                    {
                        scriptFile += "//= require " + javascript + "\n";
                        addToFile(javascriptsFolder.Replace(themeName + "\\", "") + themeName + ".js", "//= require " + themeName + "/" + Path.GetFileName(javascriptsFolder + javascript));

                        if (!File.Exists(javascriptsFolder + Path.GetFileName(javascriptsFolder + javascript + ".js")))
                        {
                            Console.WriteLine("Creating: " + Path.GetFileName(javascriptsFolder + javascript + ".js"));
                            System.IO.File.WriteAllText(javascriptsFolder + Path.GetFileName(javascriptsFolder + javascript + ".js"), scriptFile);
                        }
                        else
                        {
                            //File already exists.
                        }
                        addToPipeline(projectFolder, themeName, Path.GetFileName(javascriptsFolder + javascript + ".js"));

                        scriptFile = "";
                    }                   
                }
            }


            //Removing the require_tree . from application.css
            Console.WriteLine("Removing the require_tree . line from app\\assets\\stylesheets\\application.css");
            replaceLine(projectFolder + "\\app\\assets\\stylesheets\\application.css", "*= require_tree .");

            //Removing the require_tree . from applicatin.js
            Console.WriteLine("Removing the require_tree . line from app\\assets\\javascripts\\application.js");
            replaceLine(projectFolder + "\\app\\assets\\javascripts\\application.js", "//= require_tree .");
        }

        public static void addToPipeline(string projectFolder, string themeName, string fileCreate)
        {
            if (editedAlready(projectFolder + "\\config\\initializers\\assets.rb", "Rails.application.config.assets.precompile += %w( " + themeName + "/" + fileCreate + " )"))
            {
                using (System.IO.StreamWriter assets = new System.IO.StreamWriter(projectFolder + "\\config\\initializers\\assets.rb", true))
                {
                    assets.WriteLine("Rails.application.config.assets.precompile += %w( " + themeName + "/" + fileCreate + " )");
                }
            }
        }

        public static void singleFileaddToPipeline(string projectFolder, string themeName, string fileCreate)
        {
            if (editedAlready(projectFolder + "\\config\\initializers\\assets.rb", "Rails.application.config.assets.precompile += %w( " + fileCreate + " )"))
            {
                using (System.IO.StreamWriter assets = new System.IO.StreamWriter(projectFolder + "\\config\\initializers\\assets.rb", true))
                {
                    assets.WriteLine("Rails.application.config.assets.precompile += %w( " + fileCreate + " )");
                }
            }
        }

        public static void addToFile(string file, string lineToAdd)
        {
            if (editedAlready(file, lineToAdd))
            {
                using (System.IO.StreamWriter assets = new System.IO.StreamWriter(file, true))
                {
                    assets.WriteLine(lineToAdd);
                }
            }
        }

        //The following method returns a list containing all the stylesheets used by a given html file
        public static List<string> returnStylesheetImports(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines(filepath);
            var stylesheets = new List<string>();

            // Display the file contents by using a foreach loop.
            //System.Console.WriteLine("Contents of WriteLines2.txt = ");
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                if (line.Contains("stylesheet"))
                {
                    if (stripStylesheetLine(line.Trim(' ')).Length > 0)
                    {
                        if (!line.Contains("https") & !line.Contains("http") & !line.Contains("<!--") & !line.Contains("-->"))
                        {
                            stylesheets.Add(stripStylesheetLine(line.Trim(' ')));
                        }
                    }
                }
            }

            return stylesheets;
        }

        //The following method returns a list containing all the stylesheets used by a given html file
        public static List<string> returnJavascriptImports(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines(filepath);
            var javascripts = new List<string>();

            // Display the file contents by using a foreach loop.
            //System.Console.WriteLine("Contents of WriteLines2.txt = ");
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                if (line.Contains("script"))
                {
                    if (stripJavascriptLine(line.Trim(' ')).Length > 0)
                    {
                        if (!line.Contains("https") & !line.Contains("http") & !line.Contains("<!--") & !line.Contains("-->"))
                        {
                            javascripts.Add(stripJavascriptLine(line.Trim(' ')));
                        }
                    }
                }
            }

            return javascripts;
        }

        public static string stripStylesheetLine(string line)
        {
            string[] split = line.Split();
            string finalPath = "";

            foreach (string word in split)
            {
                if (word.Contains("href=") & !word.Contains("https") & !word.Contains("http") & !word.Contains("<!--") & !word.Contains("-->"))
                {
                    finalPath = word.Substring(6);
                    if (findString(finalPath, ".css") > 0)
                    {
                        finalPath = finalPath.Remove(findString(finalPath, ".css"));
                    }
                }
            }

            return finalPath;
        }

        public static int findString(string originalString, string seekForThis)
        {
            int temp = originalString.IndexOf(seekForThis);
            return temp;
        }

        public static string stripJavascriptLine(string line)
        {
            string[] split = line.Split();
            string finalPath = "";

            foreach (string word in split)
            {
                if (word.Contains("src=") & !word.Contains("https") & !word.Contains("http") & !word.Contains("<!--") & !word.Contains("-->"))
                {
                    finalPath = word.Substring(5);
                    if (findString(finalPath, ".js") > 0)
                    {
                        finalPath = finalPath.Remove(findString(finalPath, ".js"));
                    }
                }
            }

            return finalPath;
        }

        public static bool editedAlready(string file, string searchline)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            foreach (string line in lines)
            {
                if (line.Contains(searchline))
                {
                    return false;
                }
            }

            return true;
        }

        public static void replaceLine(string file, string lineToBeReplaced)
        {
            string[] lines = System.IO.File.ReadAllLines(file);
            var newLines = new List<string>();
            foreach (string line in lines)
            {
                if (!line.Contains(lineToBeReplaced))
                {
                    newLines.Add(line);
                }
            }

            using (System.IO.StreamWriter existingFile = new System.IO.StreamWriter(file, false))
            {
                foreach (string newline in newLines)
                {
                    existingFile.WriteLine(newline);
                }
            }

        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string themeName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                if (Path.GetFileName(Path.GetDirectoryName(temppath)) != themeName)
                {
                    // Copy the file.
                    Console.Write(".");
                    file.CopyTo(temppath, true);
                }

            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, themeName);
                }
            }
        }
    }
}
