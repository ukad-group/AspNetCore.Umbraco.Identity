# AspNetCore.Umbraco.Identity

ASP.NET Core Identity stores for Umbraco members - access Umbraco member data without referencing the Umbraco libraries.

## Features

- User store for Umbraco members (`IUserStore`, `IUserPasswordStore`, `IUserEmailStore`, `IUserRoleStore`, `IUserClaimStore`)
- Role-based authentication via Umbraco member groups
- Password hasher compatible with Umbraco's password format
- Queryable user store support

## Installation

```bash
dotnet add package AspNetCore.Umbraco.Identity
```

Or via NuGet Package Manager:
```
Install-Package AspNetCore.Umbraco.Identity
```

## Usage

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddUmbracoIdentity<ApplicationUser>();
}
```

Your user class must implement `IUser`:

```csharp
public class ApplicationUser : IUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Alias { get; set; }
    public string PasswordHash { get; set; }
}
```

## Requirements

- .NET Standard 2.0+
- Entity Framework Core (for `IRepository` implementation)

## Downloads

[![NuGet](https://img.shields.io/nuget/v/AspNetCore.Umbraco.Identity.svg)](https://www.nuget.org/packages/AspNetCore.Umbraco.Identity)

## License

MIT License - see [LICENSE](LICENSE) for details.
