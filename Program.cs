using System;
using System.Data;
using System.Data.SqlClient;

public class UserManager
{
    private readonly string _connectionString;

    public UserManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters); 
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }
    }


    private int ExecuteCommand(string query, params SqlParameter[] parameters)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }

    public void ManageUsers()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Работа с таблицей Users.");
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1) Посмотреть все записи");
            Console.WriteLine("2) Добавить нового пользователя");
            Console.WriteLine("3) Обновить существующего пользователя");
            Console.WriteLine("4) Удалить существующего пользователя");
            Console.WriteLine("5) Авторизоваться в системе");
            Console.WriteLine("0) Выход");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ViewUsers(); break;
                case "2": AddUser(); break;
                case "3": UpdateUser(); break;
                case "4": DeleteUser(); break;
                case "5": Login(); break;
                case "0": return;
                default: Console.WriteLine("Неверный выбор."); Console.ReadKey(); break;
            }
        }
    }

    private void ViewUsers()
    {
        Console.Clear();
        DataTable users = ExecuteQuery("SELECT user_id, first_name, email, password_hash FROM Users");

        if (users.Rows.Count == 0)
        {
            Console.WriteLine("Список пользователей пуст.");
        }
        else
        {
            Console.WriteLine("Список пользователей:");
            Console.WriteLine("{0,-5} {1,-15} {2,-20} {3,-10}", "ID", "Firstname", "Email", "Password");
            foreach (DataRow row in users.Rows)
            {
                Console.WriteLine("{0,-5} {1,-15} {2,-20} {3,-10} {4}", row["user_id"], row["first_name"], row["email"], "********" );
            }
        }
        Console.ReadKey();
    }

    private void AddUser()
    {
        Console.Clear();
        Console.WriteLine("Добавление пользователя:");
        Console.WriteLine("Введите данные через запятую (Firstname,Email,Password):");

        while (true)
        {
            string input = Console.ReadLine();
            string[] parts = input.Split(',');
            if (parts.Length == 3)
            {
                string firstname = parts[0].Trim();
                string email = parts[1].Trim();
                string password = parts[2].Trim(); //Insecure - needs hashing!

                string query = "INSERT INTO Users (first_name, email, password_hash) VALUES (@firstname, @email, @password)";
                int rowsAffected = ExecuteCommand(query,
                    new SqlParameter("@firstname", firstname),
                    new SqlParameter("@email", email),
                    new SqlParameter("@password", password));

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Пользователь успешно добавлен!");
                    break;
                }
                else
                {
                    Console.WriteLine("Ошибка добавления пользователя. Возможно, email уже существует."); //More specific error message
                }
            }
            else
            {
                Console.WriteLine("Неверный формат. Повторите попытку (Firstname,Email,Password).");
            }
        }
        Console.ReadKey();
    }


    private void UpdateUser()
    {
        Console.Clear();
        Console.WriteLine("Обновление пользователя:");
        Console.WriteLine("Введите ID пользователя для обновления:");

        if (int.TryParse(Console.ReadLine(), out int userId))
        {
            string selectQuery = "SELECT first_name, email, password_hash FROM Users WHERE user_id = @userId";
            DataTable existingUserData = ExecuteQuery(selectQuery, new SqlParameter("@userId", userId));

            if (existingUserData.Rows.Count > 0)
            {
                DataRow userRow = existingUserData.Rows[0];
                string existingFirstName = userRow["first_name"].ToString();
                string existingEmail = userRow["email"].ToString();
                string existingPassword = userRow["password_hash"].ToString();


                Console.WriteLine($"Текущие данные: ID: {userId}, First Name: {existingFirstName}, Email: {existingEmail}, Password: ********");
                Console.WriteLine("Введите новые данные через запятую (Firstname,Email,Password, leave empty to skip):");

                while (true)
                {
                    string input = Console.ReadLine();
                    string[] parts = input.Split(',');
                    if (parts.Length == 4)
                    {
                        string firstname = parts[0].Trim().Length > 0 ? parts[0].Trim() : existingFirstName;
                        string email = parts[1].Trim().Length > 0 ? parts[1].Trim() : existingEmail;
                        string password = parts[2].Trim().Length > 0 ? parts[2].Trim() : existingPassword;

                        string updateQuery = "UPDATE Users SET first_name = @firstname, email = @email, password_hash = @password WHERE user_id = @userId";
                        int rowsAffected = ExecuteCommand(updateQuery,
                            new SqlParameter("@firstname", firstname),
                            new SqlParameter("@email", email),
                            new SqlParameter("@password", password), 
                            new SqlParameter("@userId", userId));


                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Пользователь успешно обновлен!");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Ошибка обновления пользователя.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверный формат. Повторите попытку.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Пользователь с ID {userId} не найден.");
            }
        }
        else
        {
            Console.WriteLine("Неверный формат ID.");
        }
        Console.ReadKey();
    }


    private void DeleteUser()
    {
        Console.Clear();
        Console.WriteLine("Удаление пользователя:");
        Console.WriteLine("Введите ID пользователя для удаления:");

        if (int.TryParse(Console.ReadLine(), out int userId))
        {
            string deleteQuery = "DELETE FROM Users WHERE user_id = @userId"; 
            int rowsAffected = ExecuteCommand(deleteQuery, new SqlParameter("@userId", userId));

            if (rowsAffected > 0)
            {
                Console.WriteLine("Пользователь успешно удален!");
            }
            else
            {
                Console.WriteLine($"Пользователь с ID {userId} не найден.");
            }
        }
        else
        {
            Console.WriteLine("Неверный формат ID.");
        }
        Console.ReadKey();
    }


    private void Login()
    {
        Console.WriteLine("Введите имя пользователя:");
        string username = Console.ReadLine();
        Console.WriteLine("Введите пароль:");
        string password = Console.ReadLine();

        if (username == "admin" && password == "password")
        {
            Console.WriteLine("Авторизация успешна!");
        }
        else
        {
            Console.WriteLine("Неверный логин или пароль.");
        }
        Console.ReadKey();
    }

    public static void Main(string[] args)
    {
        string connectionString = "Server=KOMPUTER\\MSSQLSERVER02;Database=family_content;Trusted_Connection=True;TrustServerCertificate=True;";
        UserManager userManager = new UserManager(connectionString);
        userManager.ManageUsers();
    }
}