using bankaccount.Service;
using System;

namespace bankaccount
{

    class Program
    {
        static void Main(string[] args)
        {           
            var accountService = new AccountService();  //function calling 
            UserService user = new UserService(); // function calling of User

            //Repeat user screen until user id and password are correct
            while (!user.LoginDetails()) // while the login username and password is correct, the loop should repeatedly ask for username and password.
            {
                Console.WriteLine("\n Invalid Credentials!..., Please try again... "); 
            }

            Console.WriteLine("\n Valid Credentials!..., Please enter");

            while (true) 
            {
                // banking system options declaration.
                Console.WriteLine("      ___________________________________________________________");
                Console.WriteLine("      |             WELCOME TO SIMPLE BANKING SYSTEM             |");
                Console.WriteLine("      ============================================================");
                Console.WriteLine("      |  1. CREATE A NEW ACCOUNT                                 |");
                Console.WriteLine("      |  2. SEARCH FOR AN ACCOUNT                                |");
                Console.WriteLine("      |  3. DEPOSIT                                              |");
                Console.WriteLine("      |  4. WITHDRAWAL                                           |");
                Console.WriteLine("      |  5. A/C STATEMENT                                        |");
                Console.WriteLine("      |  6. DELETE AN ACCOUNT                                    |");
                Console.WriteLine("      |  7. EXIT                                                 |");
                Console.WriteLine("      ____________________________________________________________");
                Console.Write("       |       ENTER YOUR CHOICE FROM 1-7: ");
                var case_choice = Console.ReadLine(); // enter the one of the mentioned choice.
                Console.WriteLine("      ------------------------------------------------------------");

                switch (case_choice) // switch case fro each options.
                {
                    case "1":
                        Console.WriteLine("Creating Account...");
                        accountService.CreateAccount(); // if user pressed 1, this execute the function create account for entering details

                        break;
                    case "2":
                        Console.WriteLine("Search an Account...");
                        accountService.SearchAccount(); //if user pressed 2, this execute the function search account for searching the account
                         break;
                    case "3":
                        Console.WriteLine("Deposit to an Account...");
                        accountService.DepositAccount(); //if user pressed 3, this execute the function DepositAccount for depositing money into an accout.
                         break;
                    case "4":
                        accountService.WithdrawAmount();
                        break;
                    case "5":
                        accountService.GetAccountStatement();
                        break;
                    case "6":
                        accountService.DeleteAccount();
                        break;
                    case "7":
                        return;

                    default:
                        Console.WriteLine("Please enter a valid choice");
                        break;

                }
                Console.ReadKey();
                Console.Clear();
            }

        }
    }
}
