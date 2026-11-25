using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Employer", "JobSeeker", "Admin" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Development-only sample data so the site is not empty while you build and demo it.
    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const string demoEmail = "demo.employer@careerflow.dev";
        var demoPassword = scope.ServiceProvider.GetRequiredService<IConfiguration>()["Seed:EmployerPassword"]
    ?? "DemoEmployer123";

        // Idempotent: if the demo employer exists, the data was already seeded.
        if (await userManager.FindByEmailAsync(demoEmail) is not null) return;

        var owner = new ApplicationUser
        {
            UserName = demoEmail,
            Email = demoEmail,
            DisplayName = "CareerFlow Demo Employer",
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(owner, demoPassword);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                "Could not create the demo employer: " +
                string.Join("; ", created.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(owner, "Employer");

        // ".example" is a reserved domain that can never be a real website.
        var investek = new Company { Name = "Investek", Website = "https://investek.example", Location = "Johannesburg", OwnerId = owner.Id };
        var fnd = new Company { Name = "FND", Website = "https://fnd.example", Location = "Cape Town", OwnerId = owner.Id };
        var capifec = new Company { Name = "Capifec", Website = "https://capifec.example", Location = "Pretoria", OwnerId = owner.Id };
        var nexora = new Company { Name = "Nexora Labs", Website = "https://nexoralabs.example", Location = "Durban", OwnerId = owner.Id };
        var karoo = new Company { Name = "Karoo Cloud", Website = "https://karoocloud.example", Location = "Cape Town", OwnerId = owner.Id };
        var blueridge = new Company { Name = "BlueRidge Analytics", Website = "https://blueridge.example", Location = "Johannesburg", OwnerId = owner.Id };

        db.Companies.AddRange(investek, fnd, capifec, nexora, karoo, blueridge);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        JobOpportunity Job(Company company, string title, string description, EmploymentType type,
            decimal? min, decimal? max, int closesInDays, int postedDaysAgo) => new()
        {
            Title = title,
            Description = description,
            EmploymentType = type,
            SalaryMin = min,
            SalaryMax = max,
            ClosingDate = today.AddDays(closesInDays),
            CompanyId = company.Id,
            OwnerId = owner.Id,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-postedDaysAgo)
        };

        db.JobOpportunities.AddRange(
            Job(capifec, "Junior .NET Developer",
                "Build and maintain ASP.NET Core services for our lending platform. You will work with C#, EF Core and SQL Server alongside senior engineers.",
                EmploymentType.FullTime, 380_000, 500_000, 21, 0),
            Job(investek, "Junior Full-Stack Developer",
                "Join the team behind our investment dashboard. React and TypeScript on the front end, ASP.NET Core on the back end.",
                EmploymentType.FullTime, 360_000, 480_000, 28, 1),
            Job(fnd, "Software Developer Intern",
                "A 12-month internship for students and recent graduates. You will ship real features, with a mentor guiding your code reviews.",
                EmploymentType.Internship, 96_000, 120_000, 14, 2),
            Job(nexora, "React Front-End Developer",
                "Own the user interface of our customer portal. Strong JavaScript or TypeScript and an eye for clean, accessible UI are essential.",
                EmploymentType.FullTime, 420_000, 560_000, 30, 3),
            Job(investek, "Graduate Software Engineer Programme",
                "Our 18-month graduate programme rotates you through backend, frontend and DevOps teams, ending with a permanent placement.",
                EmploymentType.GraduateProgramme, 300_000, 360_000, 35, 4),
            Job(karoo, "Cloud Support Learnership",
                "A 12-month learnership for people starting in IT. You will learn cloud basics, monitoring and customer support on the job.",
                EmploymentType.Learnership, 84_000, 108_000, 20, 5),
            Job(capifec, "Data Analyst",
                "Turn lending and repayment data into reports the business can act on. SQL is required; Power BI or Python is a bonus.",
                EmploymentType.FullTime, 400_000, 520_000, 18, 6),
            Job(fnd, "UX/UI Designer (6-month contract)",
                "Design flows and prototypes for a new mobile product, working closely with developers. Figma portfolio required.",
                EmploymentType.Contract, 450_000, 600_000, 25, 7),
            Job(blueridge, "QA Automation Engineer",
                "Write automated tests that keep our analytics platform reliable. Experience with Playwright, Selenium or xUnit is a plus.",
                EmploymentType.FullTime, 390_000, 510_000, 32, 8),
            Job(nexora, "Part-Time Support Engineer",
                "Help customers troubleshoot our software for about 20 hours a week. Good communication and a basic technical background needed.",
                EmploymentType.PartTime, 180_000, 240_000, 16, 9));

        await db.SaveChangesAsync();
    }

    // Development-only: a demo job seeker with a few applications so the employer pages are not empty.
    public static async Task SeedDemoApplicationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const string seekerEmail = "demo.seeker@careerflow.dev";
        var seekerPassword = scope.ServiceProvider.GetRequiredService<IConfiguration>()["Seed:SeekerPassword"]
    ?? "DemoSeeker123";

        if (await userManager.FindByEmailAsync(seekerEmail) is not null) return;

        var employer = await userManager.FindByEmailAsync("demo.employer@careerflow.dev");
        if (employer is null) return;

        var seeker = new ApplicationUser
        {
            UserName = seekerEmail,
            Email = seekerEmail,
            DisplayName = "Thandi Demo",
            EmailConfirmed = true
        };
        var created = await userManager.CreateAsync(seeker, seekerPassword);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                "Could not create the demo seeker: " +
                string.Join("; ", created.Errors.Select(e => e.Description)));
        await userManager.AddToRoleAsync(seeker, "JobSeeker");

        var jobs = await db.JobOpportunities
            .Where(j => j.OwnerId == employer.Id)
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(4)
            .ToListAsync();

        var statuses = new[]
        {
            ApplicationStatus.Interview, ApplicationStatus.Assessment,
            ApplicationStatus.Applied, ApplicationStatus.Rejected
        };

        for (var i = 0; i < jobs.Count; i++)
        {
            var application = new JobApplication
            {
                JobOpportunityId = jobs[i].Id,
                OwnerId = seeker.Id,
                Status = statuses[i],
                AppliedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(i + 2))),
                CoverLetter = "I am excited about this role and would love to contribute. My experience with C# and React fits well."
            };
            db.JobApplications.Add(application);

            db.ApplicationStatusChanges.Add(new ApplicationStatusChange
            {
                JobApplicationId = application.Id,
                FromStatus = ApplicationStatus.Saved,
                ToStatus = ApplicationStatus.Applied
            });
            if (statuses[i] != ApplicationStatus.Applied)
                db.ApplicationStatusChanges.Add(new ApplicationStatusChange
                {
                    JobApplicationId = application.Id,
                    FromStatus = ApplicationStatus.Applied,
                    ToStatus = statuses[i]
                });
        }

        await db.SaveChangesAsync();
    }
}
