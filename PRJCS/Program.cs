using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

class Program
{
    static void Main()
    {
        // Establish database connection
        string connectionString = @"Data Source = localhost; Initial Catalog = infor; Integrated Security = True";
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        // Perform login
        bool loginSuccess = false;
        int count = 0;
        do
        {
            Console.Clear();
            if (count > 0)
            {
                Console.WriteLine("Sai tai khoan hoac mat khau. Vui long thu lai.");
            }
            Console.Write("Tai khoan: ");
            string username = Console.ReadLine();
            Console.Write("Mat khau: ");
            string password = Console.ReadLine();

            if (Login(connection, username, password))
            {
                // User is logged in as admin
                if (IsAdmin(connection, username))
                {
                    Console.Clear();
                    AdminMenu(connection);
                }
                // User is logged in as student
                else
                {
                    Console.Clear();
                    StudentMenu(connection, username);
                }

                loginSuccess = true;
            }
            count++;
        } while (!loginSuccess);




        connection.Close();
    }

    static bool Login(SqlConnection connection, string username, string password)
    {
        // Use parameters to avoid SQL injection
        string query = "SELECT * FROM taikhoan WHERE username = @username AND password = @password";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                // If there is a match in the database, the user is logged in
                return reader.HasRows;
            }
        }
    }


    static bool IsAdmin(SqlConnection connection, string username)
    {
        // Use parameters to avoid SQL injection
        string query = "SELECT username FROM taikhoan WHERE username = @username";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@username", username);

            object result = command.ExecuteScalar();

            // If the result is not null and the role is "admin", the user is an admin
            return result != null && result.ToString().ToLower() == "admin";
        }
    }




    static void AdminMenu(SqlConnection connection)
    {
        int choice;
        do
        {
            Console.WriteLine("1. Hien thi danh sach sinh vien");
            Console.WriteLine("2. Them thong tin sinh vien");
            Console.WriteLine("3. Sua thong tin sinh vien");
            Console.WriteLine("4. Xoa thong tin sinh vien");
            Console.WriteLine("5. Tim kiem sinh vien");
            Console.WriteLine("6. Kiem tra dieu kien tot nghiep");
            Console.WriteLine("7. Hien thi cac sinh vien bi canh cao");
            Console.WriteLine("0. Thoat");

            Console.Write("Nhap lua chon cua ban: ");
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        // Display student list
                        DisplayStudentList(connection);
                        break;
                    case 2:
                        // Add a student
                        AddStudent(connection);
                        break;
                    case 3:
                        // Edit a student
                        EditInformation(connection);
                        break;
                    case 4:
                        // Delete a student
                        DeleteStudent(connection);
                        break;
                    case 5:
                        // Search and display student information
                        SearchStudent(connection);
                        break;
                    case 6:
                        // Check graduation conditions for a student
                        CheckGraduationConditions(connection);
                        break;
                    case 7:
                        // Display students with muccanhcao > 0
                        DisplayStudentsWithMuccanhcao(connection);
                        break;
                    case 0:
                        // Exit
                        break;
                    default:
                        Console.WriteLine("Moi nhap lai.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Moi nhap lai");
            }

        } while (choice != 0);
    }

    static void StudentMenu(SqlConnection connection, string username)
    {
        int choice;
        do
        {
            Console.WriteLine("1. Hien thi danh sach sinh vien");
            Console.WriteLine("2. Doi mat khau");
            Console.WriteLine("3. Sua thong tin ca nhan");
            Console.WriteLine("4. Kiem tra dieu kien tot nghiep");
            Console.WriteLine("0. Thoat");

            Console.Write("Nhap lua chon cua ban: ");
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                switch (choice)
                {
                    case 1:
                        // Display student list
                        DisplayStudentList(connection);
                        break;
                    case 2:
                        // Change password
                        ChangePassword(connection, username);
                        break;
                    case 3:
                        // Edit personal information
                        EditPersonalInformation(connection, username);
                        break;
                    case 4:
                        // Check graduation conditions for a student
                        ViewGraduationConditions(connection, username);
                        break;
                    case 0:
                        // Exit
                        break;
                    default:
                        Console.WriteLine("Moi nhap lai.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Moi nhap lai.");
            }

        } while (choice != 0);
    }

    static void DisplayStudentList(SqlConnection connection)
    {
        Console.Clear();
        string query = "SELECT * FROM sinhvien";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Student List:");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                    "Full Name", "MSSV", "Gender", "Email", "SoTC", "CPA", "TOEIC", "Muccanhcao");
                Console.WriteLine("-----------------------------------------------------");

                while (reader.Read())
                {
                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        reader["Hoten"], reader["Mssv"], reader["Gioiting"],
                        reader["Email"], reader["SoTC"], reader["CPA"], reader["Toeic"], reader["Muccanhcao"]);
                }
            }
        }
    }

    static void AddStudent(SqlConnection connection)
    {
        Console.Clear();
        // Generate a random password
        string randomPassword = GenerateRandomPassword();

        // Collect student information
        Console.Write("Nhap ho va ten: ");
        string fullName = Console.ReadLine();
        Console.Write("Nhap MSSV: ");
        int MSSV = int.Parse(Console.ReadLine());
        Console.Write("Nhap gioi tinh: ");
        string gender = Console.ReadLine();
        Console.Write("Nhap email ");
        string email = Console.ReadLine();
        Console.Write("Enter so tin chi: ");
        int SoTC = int.Parse(Console.ReadLine());
        Console.Write("Nhap CPA: ");
        double GPA = double.Parse(Console.ReadLine());
        Console.Write("Nhap diem TOEIC (chua thi thi nhap 0): ");
        double TOEIC = double.Parse(Console.ReadLine());
        Console.Write("Enter muccanhcao: ");
        int muccanhcao = int.Parse(Console.ReadLine());

        // Add student to sinhvien table
        string insertStudentQuery = "INSERT INTO sinhvien (Hoten, Mssv, Gioiting, Email, SoTC, CPA, Toeic, Muccanhcao) " +
                                    "VALUES (@fullName, @MSSV, @gender, @email, @SoTC, @GPA, @TOEIC, @muccanhcao)";

        using (SqlCommand command = new SqlCommand(insertStudentQuery, connection))
        {
            command.Parameters.AddWithValue("@fullName", fullName);
            command.Parameters.AddWithValue("@MSSV", MSSV);
            command.Parameters.AddWithValue("@gender", gender);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@SoTC", SoTC);
            command.Parameters.AddWithValue("@GPA", GPA);
            command.Parameters.AddWithValue("@TOEIC", TOEIC);
            command.Parameters.AddWithValue("@muccanhcao", muccanhcao);

            command.ExecuteNonQuery();
        }

        // Add student to taikhoan table with random password
        string insertAccountQuery = "INSERT INTO taikhoan (username, password) VALUES (@MSSV, @randomPassword)";

        using (SqlCommand command = new SqlCommand(insertAccountQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);
            command.Parameters.AddWithValue("@randomPassword", randomPassword);

            command.ExecuteNonQuery();
        }

        Console.WriteLine("Student added successfully.");
    }

    static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    static void DeleteStudent(SqlConnection connection)
    {
        Console.Clear();
        // Get student ID to delete
        Console.Write("Nhap MSSV can xoa: ");
        int MSSV;
        while (!int.TryParse(Console.ReadLine(), out MSSV))
        {
            Console.WriteLine("Moi nhap lai MSSV.");
        }

        // Check if the student exists
        if (IsStudentExist(connection, MSSV))
        {
            // Delete student from sinhvien table
            string deleteStudentQuery = "DELETE FROM sinhvien WHERE Mssv = @MSSV";

            using (SqlCommand command = new SqlCommand(deleteStudentQuery, connection))
            {
                command.Parameters.AddWithValue("@MSSV", MSSV);

                command.ExecuteNonQuery();
            }

            // Delete student from taikhoan table
            string deleteAccountQuery = "DELETE FROM taikhoan WHERE username = @MSSV";

            using (SqlCommand command = new SqlCommand(deleteAccountQuery, connection))
            {
                command.Parameters.AddWithValue("@MSSV", MSSV);

                command.ExecuteNonQuery();
            }

            Console.WriteLine("Xoa thanh cong.");
        }
        else
        {
            Console.WriteLine("Sinh vien co mssv {0} khong ton tai.", MSSV);
        }
    }

    static bool IsStudentExist(SqlConnection connection, int MSSV)
    {
        // Check if the student exists in sinhvien table
        string checkStudentQuery = "SELECT COUNT(*) FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(checkStudentQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
        }
    }

    static void SearchStudent(SqlConnection connection)
    {
        Console.Clear();
        // Get student ID to search
        Console.Write("Nhap MSSV: ");
        int MSSV;
        while (!int.TryParse(Console.ReadLine(), out MSSV))
        {
            Console.WriteLine("Moi nhap lai mssv.");
        }

        // Retrieve student information
        string searchStudentQuery = "SELECT * FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(searchStudentQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine("Student Information:");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        "Full Name", "MSSV", "Gender", "Email", "SoTC", "CPA", "TOEIC", "Muccanhcao");
                    Console.WriteLine("-----------------------------------------------------");

                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        reader["Hoten"], reader["Mssv"], reader["Gioiting"],
                        reader["Email"], reader["SoTC"], reader["CPA"], reader["Toeic"], reader["Muccanhcao"]);
                }
                else
                {
                    Console.WriteLine("Sinh vien co mssv {0} khong ton tai.", MSSV);
                }
            }
        }
    }

    static void CheckGraduationConditions(SqlConnection connection)
    {
        Console.Clear();
        // Get student ID to check graduation conditions
        Console.Write("Nhap MSSV: ");
        int MSSV;
        while (!int.TryParse(Console.ReadLine(), out MSSV))
        {
            Console.WriteLine("Moi nhap lai.");
        }

        // Retrieve student information
        string checkConditionsQuery = "SELECT * FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(checkConditionsQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Retrieve relevant information
                    int SoTC = Convert.ToInt32(reader["SoTC"]);
                    double GPA = Convert.ToDouble(reader["CPA"]);
                    double TOEIC = Convert.ToDouble(reader["Toeic"]);

                    // Check graduation conditions
                    if (SoTC >= 160 && GPA >= 2.6 && TOEIC >= 550)
                    {
                        Console.WriteLine("Sinh vien co mssv {0} da du dieu kien tot nghiep.", MSSV);
                    }
                    else
                    {
                        Console.WriteLine("Sinh vien co mssv {0} chua du dieu kien tot nghiep.", MSSV);
                    }
                }
                else
                {
                    Console.WriteLine("Sinh vien co mssv {0} khong ton tai.", MSSV);
                }
            }
        }
    }

    static void DisplayStudentsWithMuccanhcao(SqlConnection connection)
    {
        Console.Clear();
        // Retrieve students with muccanhcao > 0
        string query = "SELECT * FROM sinhvien WHERE Muccanhcao > 0";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Students with Muccanhcao > 0:");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                    "Full Name", "MSSV", "Gender", "Email", "SoTC", "CPA", "TOEIC", "Muccanhcao");
                Console.WriteLine("-----------------------------------------------------");

                while (reader.Read())
                {
                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        reader["Hoten"], reader["Mssv"], reader["Gioiting"],
                        reader["Email"], reader["SoTC"], reader["CPA"], reader["Toeic"], reader["Muccanhcao"]);
                }
            }
        }
    }

    static void ChangePassword(SqlConnection connection, string username)
    {
        Console.Clear();
        // Get new password
        Console.Write("Nhap mat khau moi: ");
        string newPassword = Console.ReadLine();

        // Update password in taikhoan table
        string updatePasswordQuery = "UPDATE taikhoan SET password = @newPassword WHERE username = @username";

        using (SqlCommand command = new SqlCommand(updatePasswordQuery, connection))
        {
            command.Parameters.AddWithValue("@newPassword", newPassword);
            command.Parameters.AddWithValue("@username", username);

            command.ExecuteNonQuery();
        }

        Console.WriteLine("Doi mat khau thanh cong.");
    }

    static void EditInformation(SqlConnection connection)
    {
        Console.Clear();
        // Get student ID
        // Get student ID to search
        Console.Write("Nhap MSSV: ");
        int MSSV;
        while (!int.TryParse(Console.ReadLine(), out MSSV))
        {
            Console.WriteLine("Moi nhap lai mssv.");
        }

        // Retrieve current student information
        string getCurrentInfoQuery = "SELECT * FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(getCurrentInfoQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Display current information
                    Console.WriteLine("Current Information:");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        "Full Name", "MSSV", "Gender", "Email", "SoTC", "CPA", "TOEIC", "Muccanhcao");
                    Console.WriteLine("-----------------------------------------------------");

                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        reader["Hoten"], reader["Mssv"], reader["Gioiting"],
                        reader["Email"], reader["SoTC"], reader["CPA"], reader["Toeic"], reader["Muccanhcao"]);
                }
                else
                {
                    Console.WriteLine("Student with ID {0} not found.", MSSV);
                    return;
                }
            }
        }

        // Get new information
        Console.WriteLine("Enter new information (leave blank to keep the current value):");
        Console.Write("Enter full name: ");
        string newFullName = Console.ReadLine();
        Console.Write("Enter gender: ");
        string newGender = Console.ReadLine();
        Console.Write("Enter email: ");
        string newEmail = Console.ReadLine();

        Console.Write("Enter SoTC (enter 'null' to set to null): ");
        string newSoTCInput = Console.ReadLine();
        int? newSoTC = (newSoTCInput.ToLower() == "null") ? (int?)null : int.Parse(newSoTCInput);

        Console.Write("Enter CPA (enter 'null' to set to null): ");
        string newCPAInput = Console.ReadLine();
        double? newCPA = (newCPAInput.ToLower() == "null") ? (double?)null : double.Parse(newCPAInput);

        Console.Write("Enter TOEIC (enter 'null' to set to null): ");
        string newTOEICInput = Console.ReadLine();
        double? newTOEIC = (newTOEICInput.ToLower() == "null") ? (double?)null : double.Parse(newTOEICInput);

        Console.Write("Enter Muccanhcao (enter 'null' to set to null): ");
        string newMuccanhcaoInput = Console.ReadLine();
        int? newMuccanhcao = (newMuccanhcaoInput.ToLower() == "null") ? (int?)null : int.Parse(newMuccanhcaoInput);

        // Update information in sinhvien table
        string updateInfoQuery = "UPDATE sinhvien " +
                                 "SET Hoten = @newFullName, " +
                                 "    Gioiting = @newGender, " +
                                 "    Email = @newEmail, " +
                                 "    SoTC = @newSoTC, " +
                                 "    CPA = @newCPA, " +
                                 "    Toeic = @newTOEIC, " +
                                 "    Muccanhcao = @newMuccanhcao " +
                                 "WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(updateInfoQuery, connection))
        {
            command.Parameters.AddWithValue("@newFullName", string.IsNullOrWhiteSpace(newFullName) ? DBNull.Value : (object)newFullName);
            command.Parameters.AddWithValue("@newGender", string.IsNullOrWhiteSpace(newGender) ? DBNull.Value : (object)newGender);
            command.Parameters.AddWithValue("@newEmail", string.IsNullOrWhiteSpace(newEmail) ? DBNull.Value : (object)newEmail);
            command.Parameters.AddWithValue("@newSoTC", newSoTC.HasValue ? (object)newSoTC.Value : DBNull.Value);
            command.Parameters.AddWithValue("@newCPA", newCPA.HasValue ? (object)newCPA.Value : DBNull.Value);
            command.Parameters.AddWithValue("@newTOEIC", newTOEIC.HasValue ? (object)newTOEIC.Value : DBNull.Value);
            command.Parameters.AddWithValue("@newMuccanhcao", newMuccanhcao.HasValue ? (object)newMuccanhcao.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MSSV", MSSV);

            command.ExecuteNonQuery();
        }

        Console.WriteLine("Personal information updated successfully.");
    }

    static void EditPersonalInformation(SqlConnection connection, string username)
    {
        Console.Clear();
        // Get student ID
        int MSSV = Convert.ToInt32(username); // Assuming the username is the student ID

        // Retrieve current student information
        string getCurrentInfoQuery = "SELECT * FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(getCurrentInfoQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Display current information
                    Console.WriteLine("Current Information:");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        "Full Name", "MSSV", "Gender", "Email", "SoTC", "CPA", "TOEIC", "Muccanhcao");
                    Console.WriteLine("-----------------------------------------------------");

                    Console.WriteLine("{0,-20} {1,-10} {2,-5} {3,-30} {4,-5} {5,-5} {6,-5} {7,-5}",
                        reader["Hoten"], reader["Mssv"], reader["Gioiting"],
                        reader["Email"], reader["SoTC"], reader["CPA"], reader["Toeic"], reader["Muccanhcao"]);
                }
                else
                {
                    return;
                }
            }
        }

        // Get new information
        Console.WriteLine("Nhap thong tin moi (de trong neu giu nguyen thong tin cu)");
        Console.Write("Nhap ho ten: ");
        string newFullName = Console.ReadLine();
        Console.Write("Nhap gioi tinh: ");
        string newGender = Console.ReadLine();
        Console.Write("Nhap email: ");
        string newEmail = Console.ReadLine();

        // Update information in sinhvien table
        string updateInfoQuery = "UPDATE sinhvien " +
                                 "SET Hoten = ISNULL(@newFullName, Hoten), " +
                                 "    Gioiting = ISNULL(@newGender, Gioiting), " +
                                 "    Email = ISNULL(@newEmail, Email) " +
                                 "WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(updateInfoQuery, connection))
        {
            command.Parameters.AddWithValue("@newFullName", string.IsNullOrEmpty(newFullName) ? DBNull.Value : (object)newFullName);
            command.Parameters.AddWithValue("@newGender", string.IsNullOrEmpty(newGender) ? DBNull.Value : (object)newGender);
            command.Parameters.AddWithValue("@newEmail", string.IsNullOrEmpty(newEmail) ? DBNull.Value : (object)newEmail);
            command.Parameters.AddWithValue("@MSSV", MSSV);

            command.ExecuteNonQuery();
        }

        Console.WriteLine("Personal information updated successfully.");
    }

    static void ViewGraduationConditions(SqlConnection connection, string username)
    {
        Console.Clear();
        // Get student ID
        int MSSV = Convert.ToInt32(username); // Assuming the username is the student ID

        // Retrieve student information
        string checkConditionsQuery = "SELECT * FROM sinhvien WHERE Mssv = @MSSV";

        using (SqlCommand command = new SqlCommand(checkConditionsQuery, connection))
        {
            command.Parameters.AddWithValue("@MSSV", MSSV);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Retrieve relevant information
                    int SoTC = Convert.ToInt32(reader["SoTC"]);
                    double GPA = Convert.ToDouble(reader["CPA"]);
                    double TOEIC = Convert.ToDouble(reader["Toeic"]);

                    // Display graduation conditions
                    Console.WriteLine("Dieu kien tot nghiep:");
                    Console.WriteLine("-----------------------------------------------------");
                    Console.WriteLine("SoTC >= 160: {0}", SoTC >= 160 ? "Thoa man" : "Khong thoa man");
                    Console.WriteLine("CPA >= 2.6: {0}", GPA >= 2.6 ? "Thoa man" : "Khong thoa man");
                    Console.WriteLine("TOEIC >= 550: {0}", TOEIC >= 550 ? "Thoa man" : "Khong thoa man");
                    Console.WriteLine("-----------------------------------------------------");
                }
            }
        }
    }
}
