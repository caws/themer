using System;

//ToDo: Delete require_tree . line from application.js and application.js

namespace Themer
{
    class Themer
    {
        static void Main(string[] args)
        {
            if ((args.Length == 0))
            {
                Console.WriteLine("I can't help you without files!");
                Console.WriteLine("This is how you use this: themer themeName themeFolder projectFolder");
            }
            else
            {
               if ((args[0] != null) && (args[1] != null) && (args[2] != null))
                {
                    string themeName = args[0];
                    string themeFolder = args[1];
                    string projectFolder = args[2];

                    Functions.organizeTheme(themeFolder, projectFolder, themeName);
                    Console.WriteLine("\nDone. Now go develop amazing stuff !");
                } else
                {
                    if (args[0] == null)
                    {
                        Console.WriteLine("You must provide the theme name.");
                    }
                    if (args[1] == null)
                    {
                        Console.WriteLine("You must provide the theme folder path.");
                    }
                    if (args[2] == null)
                    {
                        Console.WriteLine("You must provide the Rails project folder path.");
                    }
                }
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();  
        }        

    }

}
