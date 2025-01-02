using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/tasks", [Authorize] async (TaskItem task, AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            task.UserId = dbUser.Id;
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        endpoints.MapGet("/tasks", [Authorize] async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            var tasks = await db.Tasks.Where(t => t.UserId == dbUser.Id).ToListAsync();
            return Results.Ok(tasks);
        });

        endpoints.MapPut("/tasks/{id}", [Authorize] async (int id, TaskItem updatedTask, AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == dbUser.Id);
            if (task == null) return Results.NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.IsCompleted = updatedTask.IsCompleted;

            await db.SaveChangesAsync();
            return Results.Ok(task);
        });

        endpoints.MapDelete("/tasks/{id}", [Authorize] async (int id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == dbUser.Id);
            if (task == null) return Results.NotFound();

            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
        endpoints.MapGet("/tasks/filter", [Authorize] async (bool? isCompleted, AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            var query = db.Tasks.Where(t => t.UserId == dbUser.Id);

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            var tasks = await query.ToListAsync();
            return Results.Ok(tasks);
        });

        endpoints.MapGet("/tasks/sort", [Authorize] async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var username = user.Identity?.Name;
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (dbUser == null) return Results.Unauthorized();

            var tasks = await db.Tasks.Where(t => t.UserId == dbUser.Id).OrderBy(t => t.DueDate).ToListAsync();
            return Results.Ok(tasks);
        });
        

    }
}
