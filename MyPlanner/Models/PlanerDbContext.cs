using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPlanner.Models
{
    public class PlanerDbContext : DbContext
    {

        public DbSet <User> Users {  get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet <TaskClass> Tasks { get; set; }
        public DbSet <Note> Notes { get; set; }

        public PlanerDbContext()
        {

        }

        public PlanerDbContext(DbContextOptions<PlanerDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            optionsBuilder.UseNpgsql(connectionString); //использоваие строки подключения 
            optionsBuilder.UseLazyLoadingProxies(); //использование ленивой загрузки
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // Настройка первичного ключа
                entity.HasKey(u => u.Id);

                // Уникальность поля Login
                entity.HasIndex(u => u.Login).IsUnique();

                // Ограничение на максимальную длину для имени
                entity.Property(u => u.Name)
                      .HasMaxLength(100)
                      .IsRequired();  // Поле обязательное

                // Ограничение на максимальную длину для логина
                entity.Property(u => u.Login)
                      .HasMaxLength(50)
                      .IsRequired();  // Поле обязательное

                // Связь с таблицей Project (один ко многим: у пользователя может быть много проектов)
                entity.HasMany(u => u.Projects)
                      .WithOne(p => p.User)   // Один пользователь связан с проектами
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);  // Если удалить пользователя, проекты также удалятся
            });

            // Конфигурация сущности Project
            modelBuilder.Entity<Project>(entity =>
            {
                // Настройка первичного ключа
                entity.HasKey(p => p.Id);

                // Поле Name обязательно, ограничение на длину
                entity.Property(p => p.Name)
                      .HasMaxLength(200)
                      .IsRequired();  // Поле обязательно

                // Поле Description необязательно
                entity.Property(p => p.Description)
                      .HasMaxLength(500)
                       .IsRequired(false);  // Поле не обязательно;

                // Конфигурация связи один ко многим: один проект связан с множеством задач
                entity.HasMany(p => p.Tasks)
                      .WithOne(t => t.Project)   // Одна задача относится к одному проекту
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);  // Если проект удален, задачи тоже удаляются

                // Поле Deadline — дата выполнения
                entity.Property(p => p.Deadline)
                      .IsRequired();  // Поле обязательно
            });

            // Конфигурация сущности TaskClass
            modelBuilder.Entity<TaskClass>(entity =>
            {
                // Настройка первичного ключа
                entity.HasKey(t => t.Id);

                // Поле Title обязательно
                entity.Property(t => t.Title)
                      .HasMaxLength(150)
                      .IsRequired();  // Поле обязательно

                // Поле Description необязательно
                entity.Property(t => t.Description)
                      .HasMaxLength(500)
                       .IsRequired(false);  // Поле не обязательно;

                // Поле Deadline — дата выполнения задачи
                entity.Property(t => t.Deadline)
                      .IsRequired();  // Поле обязательно

                // Поле CreationDate должно быть записано при создании задачи
                entity.Property(t => t.CreationDate)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");  // Дата по умолчанию — текущее время

                // Настройка связи один ко многим с заметками (Note)
                entity.HasMany(t => t.Notes)
                      .WithOne(n => n.Task)  // Заметка принадлежит одной задаче
                      .HasForeignKey(n => n.TaskId)  // Связь через TaskId, а не через ProjectId
                      .OnDelete(DeleteBehavior.Cascade);  // При удалении задачи удаляются заметки
            });

            // Конфигурация сущности Note
            modelBuilder.Entity<Note>(entity =>
            {
                // Настройка первичного ключа
                entity.HasKey(n => n.Id);

                // Поле Content обязательно
                entity.Property(n => n.Content)
                      .HasMaxLength(1000)
                      .IsRequired();  // Поле обязательно

                // Поле CreatedAt должно быть записано при создании заметки
                entity.Property(n => n.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");  // Дата по умолчанию — текущее время
            });

            // Конфигурация сущности Category с использованием Enum
            modelBuilder.Entity<Project>(entity =>
            {
                // Хранение перечисления Category как строки в базе данных
                entity.Property(p => p.Category)
                      .HasConversion<string>()
                      .IsRequired();  // Поле обязательно
            });

            // Конфигурация сущности Priority с использованием Enum
            modelBuilder.Entity<Project>(entity =>
            {
                // Хранение перечисления Priority как строки в базе данных
                entity.Property(p => p.Priority)
                      .HasConversion<string>()
                      .IsRequired();  // Поле обязательно
            });
        }
    
    }
}
