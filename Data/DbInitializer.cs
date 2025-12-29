using System.Linq;
using System.Threading.Tasks;
using Elearning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Elearning.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher<User> hasher)
        {
            if (context == null) return;

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Ensure ThemePreference column exists (for backward compatibility)
            try
            {
                var hasColumn = await context.Database.ExecuteSqlRawAsync(@"SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'Users' 
                    AND COLUMN_NAME = 'ThemePreference' 
                    LIMIT 1;");
                // ExecuteSqlRawAsync returns number of rows affected for commands; SELECT returns -1 or may throw.
            }
            catch
            {
                // If the select fails, fallback to attempting to add column if not exists
            }

            // Attempt to add the column safely (ignore errors if it already exists)
            try
            {
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE `Users` ADD COLUMN `ThemePreference` VARCHAR(20) NOT NULL DEFAULT 'light';");
            }
            catch
            {
                // ignore if column already exists or ALTER fails
            }

            // Seed roles if not present
            if (!context.Roles.Any())
            {
                var adminRole = new Role { Name = "Admin" };
                var formRole = new Role { Name = "Formateur" };
                var studentRole = new Role { Name = "Etudiant" };

                context.Roles.AddRange(adminRole, formRole, studentRole);
                await context.SaveChangesAsync();
            }

            // Seed an admin user
            var adminEmail = "admin@elearning.local";
            if (!context.Users.Any(u => u.Email == adminEmail))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                var admin = new User
                {
                    FullName = "Site Administrator",
                    Email = adminEmail,
                    RoleId = adminRole?.Id ?? 0,
                    ThemePreference = "light"
                };

                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }

            // Seed example courses with local images if not present
            if (!context.Courses.Any())
            {
                var courses = new[]
                {
                    new Course { Title = "Fondamentaux du développement web", Description = "Apprendre HTML, CSS et JavaScript pour creer des pages responsives et interactives.", Category = "Web", ImagePath = "images/courses/web-development.svg" },
                    new Course { Title = "Programmation en C# .NET 8", Description = "Decouvrir la syntaxe C#, les concepts orientes objet et le developpement d'applications avec .NET 8.", Category = "Programming", ImagePath = "images/courses/programming-csharp.svg" },
                    new Course { Title = "Introduction a la base de donnees MySQL", Description = "Concevoir des schemas, ecrire des requetes SQL, indexation et optimisation basiques.", Category = "Database", ImagePath = "images/courses/database-mysql.svg" }
                };

                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();
            }

            // Update existing courses that lack an ImagePath to use local themed images
            var coursesToUpdate = await context.Courses.Where(c => string.IsNullOrEmpty(c.ImagePath)).ToListAsync();
            if (coursesToUpdate.Any())
            {
                foreach (var c in coursesToUpdate)
                {
                    var title = (c.Title ?? string.Empty).ToLowerInvariant();
                    var category = (c.Category ?? string.Empty).ToLowerInvariant();

                    if (category.Contains("web") || title.Contains("web") || title.Contains("html") || title.Contains("css") || title.Contains("javascript"))
                    {
                        c.ImagePath = "images/courses/web-development.svg";
                    }
                    else if (category.Contains("program") || title.Contains("c#") || title.Contains(".net") || title.Contains("programming") || title.Contains("development"))
                    {
                        c.ImagePath = "images/courses/programming-csharp.svg";
                    }
                    else if (category.Contains("db") || category.Contains("database") || title.Contains("mysql") || title.Contains("sql") || title.Contains("database"))
                    {
                        c.ImagePath = "images/courses/database-mysql.svg";
                    }
                    else
                    {
                        // default fallback image
                        c.ImagePath = "images/courses/web-development.svg";
                    }

                    context.Courses.Update(c);
                }

                await context.SaveChangesAsync();
            }

            // Replace legacy or unrelated image files references (e.g. .jfif) by mapping to the new themed SVGs
            var legacyCourses = await context.Courses.Where(c => !string.IsNullOrEmpty(c.ImagePath) && (c.ImagePath.EndsWith(".jfif") || c.ImagePath.EndsWith(".jpg") || c.ImagePath.EndsWith(".jpeg"))).ToListAsync();
            if (legacyCourses.Any())
            {
                foreach (var c in legacyCourses)
                {
                    var title = (c.Title ?? string.Empty).ToLowerInvariant();
                    var category = (c.Category ?? string.Empty).ToLowerInvariant();

                    if (category.Contains("web") || title.Contains("web") || title.Contains("html") || title.Contains("css") || title.Contains("javascript"))
                    {
                        c.ImagePath = "images/courses/web-development.svg";
                    }
                    else if (category.Contains("program") || title.Contains("c#") || title.Contains(".net") || title.Contains("programming") || title.Contains("development"))
                    {
                        c.ImagePath = "images/courses/programming-csharp.svg";
                    }
                    else if (category.Contains("db") || category.Contains("database") || title.Contains("mysql") || title.Contains("sql") || title.Contains("database"))
                    {
                        c.ImagePath = "images/courses/database-mysql.svg";
                    }
                    else if (title.Contains("data") || category.Contains("data") || title.Contains("science"))
                    {
                        c.ImagePath = "images/courses/data-science.svg";
                    }
                    else
                    {
                        c.ImagePath = "images/courses/web-development.svg";
                    }

                    context.Courses.Update(c);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}