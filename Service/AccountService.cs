using System;
using System.IO;
using System.Text.RegularExpressions;

namespace bankaccount.Service
{
    using System.Globalization;
    using System.Net.Mail;

    /// <summary>
    /// This is the Account service class used for account operationss
    /// </summary>
    public class AccountService
    {
        // Regex for phone number
        private const string phoneRegexPattern = @"^\({0,1}((0|\+61)(2|4|3|7|8)){0,1}\){0,1}(\ |-){0,1}[0-9]{2}(\ |-){0,1}[0-9]{2}(\ |-){0,1}[0-9]{1}(\ |-){0,1}[0-9]{3}$";
        // Regex for email
        private const string emailRegexPattern = @"(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";

        /// <summary>
        /// Create an account
        /// </summary>
        public void CreateAccount()
        {
            var account = new Account();
            var proceed = 'N';

            // Repeat input until user confirms the information is correct
            do
            {
                account = GetAccountInput(account);
                Console.Clear();
                DisplayInputDetails(account);
                ConsoleKeyInfo key;

                do
                {
                    Console.Write("      Is the information correct (Y/N)? : ");
                    key = Console.ReadKey(true);
                    Console.WriteLine(key.KeyChar);
                }
                while (!(key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N));

                proceed = key.KeyChar;
            }
            while (proceed == 'N');

            var rnd = new Random();

            // Generate a random account numbers
            account.AccountNumber = rnd.Next(100000, 99999999);
            SaveAccountToFile(account);

            Console.WriteLine("\n\n\nAccount Created! details will be provided via email");
            Console.WriteLine($"\nAccount number is: {account.AccountNumber}");

            SendEmail(account);

            Console.ReadKey();
        }

        /// <summary>
        /// Search accounts
        /// </summary>
        public void SearchAccount()
        {
            Console.Write("Account number: ");
            var accountNumber = Console.ReadLine();

            GetAccountDetailsByAccountNumber(accountNumber);
            ConsoleKeyInfo key;

            // Repeat search if user enters Y. Exit to main menu if user enters N
            do
            {
                Console.Write("Do you want to search for another account(Y/N)? : ");
                key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);
            }
            while (!(key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N));

            /// if the character is Y or y.
            if (key.Key == ConsoleKey.Y)
            {
                SearchAccount();
            }
        }

        public void GetAccountStatement()
        {
            Console.Write("Account number: ");
            var accountNumber = Console.ReadLine();

            var account = GetAccountDetailsByAccountNumber(accountNumber);
            ConsoleKeyInfo key;

            //Should account details be emailed?
            Console.Write("Do you want to email the account details(Y/N)? : ");

            // Repeat search if user enters Y. Exit to main menu if user enters N
            do
            {
                key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);
            }
            while (!(key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N));

            /// If the character is Y or y.
            if (key.Key == ConsoleKey.Y)
            {
                SendEmail(account);
            }
        }

        /// <summary>
        /// Deposit to account
        /// </summary>
        public void DepositAccount()
        {
            Console.Write("Enter account number: ");
            var accountNumber = Console.ReadLine();

            Console.Write("Amount: ");
            var depositAmount = Console.ReadLine();
            decimal amount;
            while (!decimal.TryParse(depositAmount, out amount))
            {
                Console.WriteLine("Enter a valid deposit amount");
                depositAmount = Console.ReadLine();
            }

            string filePath = GetFilePath(accountNumber);
            var accountDetails = File.ReadAllLines(filePath);

            decimal.TryParse(accountDetails[accountDetails.Length - 1], out var balance);

            try
            {
                balance = balance + amount;
                accountDetails[accountDetails.Length - 1] = balance.ToString(CultureInfo.InvariantCulture);
                File.WriteAllLines(filePath, accountDetails);

                Console.WriteLine("Updated account details are shown below");
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        Console.WriteLine(reader.ReadLine());
                    }
                }
                Console.WriteLine($"Deposit Successful!!!");
            }

            catch (FileNotFoundException e)
            {
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Withdraw amount from account
        /// </summary>
        public void WithdrawAmount()
        {
            Console.Write("Account number: ");
            string accountNumber = Console.ReadLine();

            Console.Write("Amount: ");
            var withdraw = Console.ReadLine();

            decimal withdrawalAmount;
            decimal balance;

            while (!decimal.TryParse(withdraw, out withdrawalAmount))
            {
                Console.WriteLine("Enter a valid withdrawal amount");
                withdraw = Console.ReadLine();
            }

            string filePath = GetFilePath(accountNumber);
            var accountDetails = File.ReadAllLines(filePath);
            decimal.TryParse(accountDetails[accountDetails.Length - 1], out balance);

            if (balance < withdrawalAmount)
            {
                Console.WriteLine("insufficient balance");
                return;
            }

            balance = balance - withdrawalAmount;
            accountDetails[accountDetails.Length - 1] = balance.ToString();
            File.WriteAllLines(filePath, accountDetails);

            Console.WriteLine($"Withdrawal Successful!!!");
        }

        /// <summary>
        /// Delete an account
        /// </summary>
        public void DeleteAccount()
        {
            Console.WriteLine("Account number:");
            string accountNumber = Console.ReadLine();
            string filePath = GetFilePath(accountNumber);
            GetAccountDetailsByAccountNumber(accountNumber);

            //Should account details be emailed?
            Console.Write("\n\nDo you want to delete this account(Y/N)? : ");
            ConsoleKeyInfo key;
            // Repeat search if user enters Y. Exit to main menu if user enters N
            do
            {
                key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);
            }
            while (!(key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N));

            /// If the character is Y or y.
            if (key.Key == ConsoleKey.Y)
            {
                File.Delete(filePath);
            }            
        }

        /// <summary>
        /// Gets account details given the account number
        /// </summary>
        /// <param name="accountNumber">account number</param>
        private static Account GetAccountDetailsByAccountNumber(string accountNumber)
        {
            // Generate the file path, given account number
            string filePath = GetFilePath(accountNumber);
            var account = new Account();
            if (File.Exists(filePath))
            {
                var accountDetails = File.ReadAllLines(filePath);
                account.AccountNumber = int.Parse(accountDetails[0]);
                account.Balance = accountDetails[6];
                account.FirstName = accountDetails[1];
                account.LastName = accountDetails[2];
                account.Address = accountDetails[3];
                account.Phone = accountDetails[5];
                account.Email = accountDetails[4];

                Console.WriteLine($"Account number: {account.AccountNumber}");
                Console.WriteLine($"Account Balance: {account.Balance}");
                Console.WriteLine($"First name: {account.FirstName}");
                Console.WriteLine($"Last Name: {account.LastName}");
                Console.WriteLine($"Address: {account.Address}");
                Console.WriteLine($"Phone: {account.Phone}");
                Console.WriteLine($"Email: {account.Email}");
                return account;
            }
            else
            {
                Console.WriteLine("Entered acount does not exist");
                return null;
            }
        }

        /// <summary>
        /// Generates a file path given an account number
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns>The file path</returns>
        private static string GetFilePath(string accountNumber)
        {
            var workingDirectory = Environment.CurrentDirectory;
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var filePath = Path.Combine(projectDirectory, "AccountFiles", accountNumber + ".txt");
            return filePath;
        }

        /// <summary>
        /// Save to file.
        /// </summary>
        /// <param name="account">Account object to save</param>
        private void SaveAccountToFile(Account account)
        {

            //This will get the current WORKING directory (i.e. \bin\Debug)

            var workingDirectory = Environment.CurrentDirectory;
            //or: Directory.GetCurrentDirectory() gives the same result

            //This will get the current PROJECT directory
            var projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            if (string.IsNullOrEmpty(account.Balance))
                account.Balance = "0";

            using (var fileWriter = new StreamWriter(Path.Combine(projectDirectory, "AccountFiles", account.AccountNumber.ToString() + ".txt"), true))
            {
                fileWriter.WriteLine(account.AccountNumber);
                fileWriter.WriteLine(account.Address);
                fileWriter.WriteLine(account.FirstName);
                fileWriter.WriteLine(account.LastName);
                fileWriter.WriteLine(account.Email);
                fileWriter.WriteLine(account.Phone);
                fileWriter.Write(account.Balance);
            }
        }

        /// <summary>
        /// This is to send an email to an account.
        /// </summary>
        private void SendEmail(Account account)
        {
            // Configure smtp client with google smtp server
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            // This is a email address created dedicated to send emails
            smtpClient.Credentials = new System.Net.NetworkCredential("utsemailer@gmail.com", "UTSIT@2019");

            // Form the email message
            MailMessage message = new MailMessage("utsemailer@gmail.com", account.Email);

            //Subject
            var subject = "Your account details";
            var body = $"<p>Your account details are given below </p> <br/> <p> Account No : {account.AccountNumber} <br/>";
            body += $"Account Balance : {account.Balance} <br/>";
            body += $"First Name : {account.FirstName} <br/>";
            body += $"Last Name : {account.LastName} <br/>";
            body += $"Address : {account.Address} <br/>";
            body += $"Phone : {account.Phone} <br/>";
            body += $"Email : {account.Email} <br/>";
            message.IsBodyHtml = true;
            message.Subject = subject;
            message.Body = body;

            smtpClient.Send(message);
        }

        /// <summary>
        /// Get the account details to create an account
        /// </summary>
        /// <param name="account">account object</param>
        /// <returns>Filled account object</returns>
        private Account GetAccountInput(Account account)
        {
            Console.WriteLine("     ___________________________________________________________");
            Console.WriteLine("     |                    CREATE A NEW ACCOUNT                  |");
            Console.WriteLine("     ============================================================");
            Console.Write("            FIRST NAME: ");
            account.FirstName = Console.ReadLine();
            Console.Write("            LAST NAME:  ");
            account.LastName = Console.ReadLine();
            Console.Write("            ADDRESS:    ");
            account.Address = Console.ReadLine();
            string email;

            // Repeat until valid email is entered
            do
            {
                Console.Write("        EMAIL:   ");
                account.Email = Console.ReadLine();
                email = account.Email;
            }
            while (!ValidateEmailId(email));


            // Repeat until valid phone number is entered
            var phone = string.Empty;
            do
            {
                Console.Write("        PHONE:   ");

                phone = Console.ReadLine();
                account.Phone = phone;
            }
            while (!ValidatePhoneNumber(phone));
            Console.WriteLine("         ==============================================================");

            return account;
        }

        /// <summary>
        /// Validate email address
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True/False</returns>
        private bool ValidateEmailId(string email)
        {
            if (!Regex.Match(email, emailRegexPattern).Success)
            {
                Console.WriteLine("Incorrect Email Id. Please enter a valid Email ID");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate phone number
        /// </summary>
        /// <param name="phone">Phone number to validate</param>
        /// <returns>True/False</returns>
        private bool ValidatePhoneNumber(string phone)
        {
            if (!Regex.Match(phone.Trim(), phoneRegexPattern).Success)
            {
                Console.WriteLine("Incorrect Phone Number. Please enter a valid phone number");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Display account details
        /// </summary>
        /// <param name="account">account object to display</param>
        private void DisplayInputDetails(Account account)
        {

            Console.WriteLine("     ---------------------------------------------------------------");
            Console.WriteLine("     |                    CREATE A NEW ACCOUNT                     |");
            Console.WriteLine("     ---------------------------------------------------------------");
            Console.WriteLine("     | FIRST NAME: {0}", account.FirstName, "                       |");
            Console.WriteLine("     | LAST NAME: {0}", account.LastName, "                         |");
            Console.WriteLine("     | ADDRESS: {0}", account.Address, "                            |");
            Console.WriteLine("     | PHONE: {0}", account.Phone, "                                |");
            Console.WriteLine("     | EMAIL: {0}", account.Email, "                                |");
            Console.WriteLine("     |---------------------------------------------------------------");
        }
    }

}
