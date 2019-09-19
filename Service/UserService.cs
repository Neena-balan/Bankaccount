using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bankaccount
{ 
       
    public class UserService
    {
        public bool LoginDetails()
        {
            Console.WriteLine("==============================================================");
            Console.WriteLine("|               WELCOME TO THE SIMPLE BANKING SYSTEM         |");
            Console.WriteLine("______________________________________________________________");

            Console.WriteLine("|                      LOGIN TO START                        |");
            Console.Write("         USERNAME:  ");
            var userName = Console.ReadLine();
            var password = "";
            Console.Write("         PASSWORD: ");
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    Console.Write("\b");
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            var workingDirectory = Environment.CurrentDirectory;
            //or: Directory.GetCurrentDirectory() gives the same result

            //This will get the current PROJECT directory
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            
            using (var reader = new StreamReader(Path.Combine(projectDirectory, "login.txt"), false))
            {
                while(!reader.EndOfStream)
                {
                    var loginPair = reader.ReadLine().Split("|");
                    if (userName.Trim() == loginPair[0].Trim() && password.Trim() == loginPair[1].Trim())
                    {
                        return true;
                    }
                        
                }
            }
            return false;
        }
    }

    
    
}
