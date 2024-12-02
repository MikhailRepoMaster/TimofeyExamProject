using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Dapper;

public class Event
{
    public int Id { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public int CategoryId { get; set; }
    public Category Categories { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

class Program
{
    public class CalendarDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Categories)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=LocalHost;Database=EventCalendar;Integrated Security=True;Trust Server Certificate=True;");
        }
    }

    public static void Main(string[] args)
    {
        string connectionString = "Server=LocalHost;Integrated Security=True;Trust Server Certificate=True;";
        string useDbCommand = "USE EventCalendar";

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            // CreateDb, AddCategory, AddEvent, после этих действий остальное в любом порядке
            //ShowAllEventsCategory(connection, useDbCommand);
            //CreateDb(connection); 

            //AddEvent(connection, useDbCommand, "Событие", DateTime.Now, 1);
            //UpdateEvent(3, "Fet", 1, DateTime.Now);
            //DeleteEvent(3, useDbCommand, connectionString);

            //AddCategory(connection, useDbCommand, "Покер");
            //ShowAllCategories(useDbCommand, connectionString); 
            //UpdateCategoryName(1, "Категория", useDbCommand, connectionString); 
            //DeleteCategory(3, useDbCommand, connectionString);
        }
    }
    public static void AddCategory(SqlConnection connection, string useDbCommand, string categoryName)
    {
        string addCategoryQuery = @"INSERT INTO Categories(CategoryName) VALUES(@categoryName);";

        connection.Open();
        using (var sqlCommand = new SqlCommand(useDbCommand, connection))
        {
            sqlCommand.ExecuteNonQuery();
        }

        connection.Execute(addCategoryQuery, new { categoryName });
        Console.WriteLine("Категория добавлена");
    }

    public static void ShowAllEventsCategory(SqlConnection connection, string useDbCommand)
    {
        using (var context = new CalendarDbContext())
        {
            var events = context.Events.Include(e => e.Categories).ToList();

            if (events.Count == 0)
            {
                Console.WriteLine("Нет событий в базе данных");
                return;
            }

            foreach (var eventItem in events)
            {
                Console.WriteLine($"ID: {eventItem.Id}");
                Console.WriteLine($"Event Name: {eventItem.EventName}");
                Console.WriteLine($"Date: {eventItem.EventDate}");
                Console.WriteLine($"Category Name: {eventItem.Categories.CategoryName}\n");
            }
        }
    }

    public static void DeleteCategory(int categoryId, string useDbCommand, string connectionString)
    {
        string deleteCategory = @"DELETE FROM Categories WHERE Id = @categoryId";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            connection.Execute(deleteCategory, new { categoryId });
            Console.WriteLine("Категория удалена");
        }
    }

    public static void UpdateCategoryName(int categoryId, string newCategoryName, string useDbCommand, string connectionString)
    {
        string updateCategoryName = @"UPDATE Categories SET CategoryName = @categoryName WHERE Id = @categoryId";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            connection.Execute(updateCategoryName, new { categoryName = newCategoryName, categoryId });
            Console.WriteLine("Имя категории обновлено");
        }
    }

    public static void ShowAllCategories(string useDbCommand, string connectionString)
    {
        string selectAllCategories = "SELECT * FROM Categories";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            var categories = connection.Query<Category>(selectAllCategories).ToList();

            foreach (var category in categories)
            {
                Console.WriteLine($"ID категории: {category.Id}\nНазвание категории: {category.CategoryName}");
            }
        }
    }

    public static void DeleteEvent(int eventId, string useDbCommand, string connectionString)
    {


        string deleteEvent = "DELETE FROM Events WHERE Id = @eventId";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            connection.Execute(deleteEvent, new { eventId });
            Console.WriteLine("Ивент удалён");
        }
    }

    public static void UpdateEvent(int eventId, string newEventName, int newCategoryId, DateTime newEventDate)
    {
        string updateEvent = @"UPDATE Events SET EventName = @eventName, EventDate = @eventDate, CategoryId = @CategoryId WHERE Id = @eventId";

        using (var connection = new SqlConnection("Server=LocalHost;Database=EventCalendar;Integrated Security=True;Trust Server Certificate=True;"))
        {
            connection.Open();

            connection.Execute(updateEvent, new { eventName = newEventName, eventDate = newEventDate, CategoryId = newCategoryId, eventId });
            Console.WriteLine("Ивент обновлён");
        }
    }

    public static void ShowAllEvents(string useDbCommand, string connectionString)
    {
        string selectAllEvents = "SELECT * FROM Events";

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            using (var sqlCommand = new SqlCommand(selectAllEvents, connection))
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string eventName = reader.GetString(1);
                        DateTime eventDate = reader.GetDateTime(2);
                        int categoryId = reader.GetInt32(3);

                        Console.WriteLine($"ID ивента: {id}\nИмя ивента: {eventName}\nДата ивента: {eventDate}\nID категории: {categoryId}");
                    }
                }
            }

            Console.WriteLine("Все ивенты отображены");
        }
    }

    public static void AddEvent(SqlConnection connection, string useDbCommand, string eventName, DateTime eventDate, int categoryId)
    {
        string addEventQuery = @"INSERT INTO Events(EventName, EventDate, CategoryId) VALUES(@eventName, @eventDate, @CategoryId);";

        connection.Open();

        using (var sqlCommand = new SqlCommand(useDbCommand, connection))
        {
            sqlCommand.ExecuteNonQuery();
        }

        connection.Execute(addEventQuery, new { eventName, eventDate, CategoryId = categoryId });
        Console.WriteLine("Ивент добавлен");
    }

    public static void CreateDb(SqlConnection connectionString)
    {
        string createDb = "CREATE DATABASE EventCalendar";
        string useDbCommand = "USE EventCalendar";

        string createCategoriesTable = @"CREATE TABLE Categories (
            Id INT PRIMARY KEY IDENTITY(1,1),
            CategoryName NVARCHAR(30)
        );";

        string createEventsTable = @"CREATE TABLE Events (
            Id INT PRIMARY KEY IDENTITY(1,1),
            EventName NVARCHAR(100),
            EventDate DATETIME,
            CategoryId INT,
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
        );";

        using (var sqlCommand = new SqlCommand(createDb, connectionString))
        {
            connectionString.Open();
            sqlCommand.ExecuteNonQuery();
            Console.WriteLine("БД EventCalendar готова");
        }

        using (var connection = new SqlConnection(connectionString.ConnectionString))
        {
            connection.Open();

            using (var sqlCommand = new SqlCommand(useDbCommand, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            using (var sqlCommand = new SqlCommand(createCategoriesTable, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            using (var sqlCommand = new SqlCommand(createEventsTable, connection))
            {
                sqlCommand.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Таблицы созданы");
    }
}
