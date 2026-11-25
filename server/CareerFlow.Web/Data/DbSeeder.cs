using System.Text;
using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Data;

public static class DbSeeder
{
    // Public demo logins. The landing page shows these same values.
    private const string EmployerEmail = "demo.employer@careerflow.dev";
    private const string EmployerPassword = "DemoEmployer123";
    private const string SeekerEmail = "demo.seeker@careerflow.dev";
    private const string SeekerPassword = "DemoSeeker123";

    private record CompanySeed(string Name, string Website, string Location);

    private record JobSeed(
        string Company, string Title, string Description, EmploymentType Kind,
        decimal? Min, decimal? Max, int ClosesInDays, int PostedDaysAgo);

    private record TalentSeed(string Name, string Email, string Headline);

    // ".example" is a reserved domain that can never be a real website.
    private static readonly CompanySeed[] Companies =
    [
        new("Investek", "https://investek.example", "Johannesburg"),
        new("FND", "https://fnd.example", "Cape Town"),
        new("Capifec", "https://capifec.example", "Pretoria"),
        new("Nexora Labs", "https://nexoralabs.example", "Durban"),
        new("Karoo Cloud", "https://karoocloud.example", "Cape Town"),
        new("BlueRidge Analytics", "https://blueridge.example", "Johannesburg"),
    ];

    private static readonly JobSeed[] Jobs =
    [
        new("Capifec", "Junior .NET Developer",
            "Build and maintain ASP.NET Core services for our lending platform. You will work with C#, EF Core and SQL Server alongside senior engineers.",
            EmploymentType.FullTime, 380_000, 500_000, 60, 0),
        new("Investek", "Junior Full-Stack Developer",
            "Join the team behind our investment dashboard. React and TypeScript on the front end, ASP.NET Core on the back end.",
            EmploymentType.FullTime, 360_000, 480_000, 75, 1),
        new("FND", "Software Developer Intern",
            "A 12-month internship for students and recent graduates. You will ship real features, with a mentor guiding your code reviews.",
            EmploymentType.Internship, 96_000, 120_000, 45, 2),
        new("Nexora Labs", "React Front-End Developer",
            "Own the user interface of our customer portal. Strong JavaScript or TypeScript and an eye for clean, accessible UI are essential.",
            EmploymentType.FullTime, 420_000, 560_000, 80, 3),
        new("Investek", "Graduate Software Engineer Programme",
            "Our 18-month graduate programme rotates you through backend, frontend and DevOps teams, ending with a permanent placement.",
            EmploymentType.GraduateProgramme, 300_000, 360_000, 90, 4),
        new("Karoo Cloud", "Cloud Support Learnership",
            "A 12-month learnership for people starting in IT. You will learn cloud basics, monitoring and customer support on the job.",
            EmploymentType.Learnership, 84_000, 108_000, 50, 5),
        new("Capifec", "Data Analyst",
            "Turn lending and repayment data into reports the business can act on. SQL is required; Power BI or Python is a bonus.",
            EmploymentType.FullTime, 400_000, 520_000, 55, 6),
        new("FND", "UX/UI Designer (6-month contract)",
            "Design flows and prototypes for a new mobile product, working closely with developers. Figma portfolio required.",
            EmploymentType.Contract, 450_000, 600_000, 70, 7),
        new("BlueRidge Analytics", "QA Automation Engineer",
            "Write automated tests that keep our analytics platform reliable. Experience with Playwright, Selenium or xUnit is a plus.",
            EmploymentType.FullTime, 390_000, 510_000, 85, 8),
        new("Nexora Labs", "Part-Time Support Engineer",
            "Help customers troubleshoot our software for about 20 hours a week. Good communication and a basic technical background needed.",
            EmploymentType.PartTime, 180_000, 240_000, 65, 9),
    ];

    // Profiles that only exist so the employer's "Find talent" page is not empty.
    // They get a random password, so nobody can log in as them.
    private static readonly TalentSeed[] Talent =
    [
        new("Sipho Dlamini", "sipho.demo@careerflow.dev", "Graduate Software Engineer - Java, Spring, PostgreSQL"),
        new("Ayesha Patel", "ayesha.demo@careerflow.dev", "Front-End Developer - React, TypeScript, accessibility"),
        new("Lerato Mokoena", "lerato.demo@careerflow.dev", "Data Analyst - SQL, Power BI, Python"),
        new("Johan van der Merwe", "johan.demo@careerflow.dev", "QA Automation Engineer - Playwright, xUnit, CI pipelines"),
    ];

    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Employer", "JobSeeker", "Admin" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Safe to run on every start: it creates what is missing and repairs the demo jobs.
    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var owner = await userManager.FindByEmailAsync(EmployerEmail);
        if (owner is null)
        {
            owner = new ApplicationUser
            {
                UserName = EmployerEmail,
                Email = EmployerEmail,
                DisplayName = "CareerFlow Demo Employer",
                EmailConfirmed = true
            };
            await CreateUserAsync(userManager, owner, EmployerPassword, "Employer");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Companies: create the missing ones.
        var existingCompanies = await db.Companies.Where(c => c.OwnerId == owner.Id).ToListAsync();
        var companyIds = new Dictionary<string, Guid>();
        foreach (var seed in Companies)
        {
            var company = existingCompanies.FirstOrDefault(c => c.Name == seed.Name);
            if (company is null)
            {
                company = new Company
                {
                    Name = seed.Name,
                    Website = seed.Website,
                    Location = seed.Location,
                    OwnerId = owner.Id
                };
                db.Companies.Add(company);
            }
            companyIds[seed.Name] = company.Id;
        }

        // Jobs: create the missing ones and put every demo job back to open with a fresh closing date.
        var existingJobs = await db.JobOpportunities.Where(j => j.OwnerId == owner.Id).ToListAsync();
        foreach (var seed in Jobs)
        {
            var companyId = companyIds[seed.Company];
            var job = existingJobs.FirstOrDefault(j => j.CompanyId == companyId && j.Title == seed.Title);
            if (job is null)
            {
                job = new JobOpportunity
                {
                    Title = seed.Title,
                    CompanyId = companyId,
                    OwnerId = owner.Id,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-seed.PostedDaysAgo)
                };
                db.JobOpportunities.Add(job);
            }

            job.Description = seed.Description;
            job.EmploymentType = seed.Kind;
            job.SalaryMin = seed.Min;
            job.SalaryMax = seed.Max;
            job.ClosingDate = today.AddDays(seed.ClosesInDays);
            job.IsClosed = false;
        }

        // Anything else on the shared demo account (jobs visitors posted) is hidden, not deleted.
        foreach (var job in existingJobs)
        {
            var isDemoJob = Jobs.Any(s => companyIds[s.Company] == job.CompanyId && s.Title == job.Title);
            if (!isDemoJob) job.IsClosed = true;
        }

        await db.SaveChangesAsync();
    }

    // Demo job seeker (with a CV and four applications) plus the directory-only profiles.
    public static async Task SeedDemoApplicationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var employer = await userManager.FindByEmailAsync(EmployerEmail);
        if (employer is null) return;

        var seeker = await userManager.FindByEmailAsync(SeekerEmail);
        if (seeker is null)
        {
            seeker = new ApplicationUser
            {
                UserName = SeekerEmail,
                Email = SeekerEmail,
                DisplayName = "Thandi Demo",
                EmailConfirmed = true,
                IsDiscoverable = true
            };
            await CreateUserAsync(userManager, seeker, SeekerPassword, "JobSeeker");

            var cv = NewDemoCv(seeker.Id, "Thandi Demo", "Junior Full-Stack Developer - C#, ASP.NET Core, React, SQL");
            db.ResumeDocuments.Add(cv);

            var jobs = await db.JobOpportunities
                .Where(j => j.OwnerId == employer.Id && !j.IsClosed)
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
                    ResumeDocumentId = cv.Id,
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
        else if (!seeker.IsDiscoverable)
        {
            // Repair: a visitor switched the demo seeker's visibility off.
            seeker.IsDiscoverable = true;
            await userManager.UpdateAsync(seeker);
        }

        foreach (var t in Talent)
        {
            if (await userManager.FindByEmailAsync(t.Email) is not null) continue;

            var person = new ApplicationUser
            {
                UserName = t.Email,
                Email = t.Email,
                DisplayName = t.Name,
                EmailConfirmed = true,
                IsDiscoverable = true
            };
            await CreateUserAsync(userManager, person, $"Aa1-{Guid.NewGuid():N}", "JobSeeker");

            db.ResumeDocuments.Add(NewDemoCv(person.Id, t.Name, t.Headline));
            await db.SaveChangesAsync();
        }
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
    {
        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                $"Could not create {user.Email}: " +
                string.Join("; ", created.Errors.Select(e => e.Description)));
        await userManager.AddToRoleAsync(user, role);
    }

    private static ResumeDocument NewDemoCv(string ownerId, string name, string headline)
    {
        var pdf = BuildDemoPdf(name, headline);
        return new ResumeDocument
        {
            OwnerId = ownerId,
            OriginalFileName = $"{name.Replace(' ', '_')}_CV.pdf",
            ContentType = "application/pdf",
            SizeBytes = pdf.Length,
            Content = pdf
        };
    }

    // Builds a tiny but valid one-page PDF in memory, so demo CVs open in any PDF viewer.
    private static byte[] BuildDemoPdf(string title, string subtitle)
    {
        static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        var content =
            $"BT /F1 24 Tf 72 760 Td ({Escape(title)}) Tj 0 -32 Td /F1 12 Tf ({Escape(subtitle)}) Tj " +
            "0 -28 Td (Demo CV created by CareerFlow for demonstration purposes.) Tj ET";

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var sb = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int>();
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(sb.Length);
            sb.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xrefStart = sb.Length;
        sb.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) sb.Append($"{offset:D10} 00000 n \n");
        sb.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefStart}\n%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}
