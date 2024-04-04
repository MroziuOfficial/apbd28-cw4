using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository = new();
        private readonly UserCreditService _userCreditService = new();

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!IsEmailValid(email))
            {
                return false;
            }
            
            int age = AgeCalculator(dateOfBirth);
            if (age < 21)
            {
                return false;
            }
            
            var client = _clientRepository.GetById(clientId);

            var user = UserCreator(firstName, lastName, email, dateOfBirth, client);
            
            SetCrLimit(user, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsEmailValid(string mail)
        {
            return mail.Contains('@') && mail.Contains('.');
        }

        private int AgeCalculator(DateTime date)
        {
            var now = DateTime.Now;
            int age = now.Year - date.Year;
            if (
                now.Month < date.Month ||
                (now.Month == date.Month && now.Day < date.Day)
                )
            {
                age--;
            }

            return age;
        }

        private User UserCreator(string firstName, string lastName, string mail, DateTime date, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = date,
                EmailAddress = mail,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private void SetCrLimit(User user, Client client)
        {
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                int crLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (client.Type == "ImportantClient")
                {
                    crLimit *= 2;
                }

                user.CreditLimit = crLimit;
            }
        }
    }
}
