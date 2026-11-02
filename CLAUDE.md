# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AriaPrint (کافینت آریا) is a Persian-language website for an internet cafe/business center offering printing, registration services, student services, bill payments, and other administrative services. Built with ASP.NET Core MVC 8.0.

## Build & Run Commands

```bash
# Run the application (development mode with HTTPS)
dotnet run --project AriaPrint.csproj --launch-profile https

# Run with HTTP only
dotnet run --project AriaPrint.csproj --launch-profile http

# Build the project
dotnet build AriaPrint.csproj

# Run with IIS Express
dotnet run --project AriaPrint.csproj --launch-profile "IIS Express"
```

**Development URLs:**
- HTTPS: `https://localhost:7077`
- HTTP: `http://localhost:5194`
- IIS Express: `https://localhost:44388`

## Architecture

### Technology Stack
- **Framework:** ASP.NET Core MVC 8.0
- **ORM:** Dapper (lightweight ORM)
- **Database:** SQL Server (LocalDB/Express)
- **Authentication:** Cookie-based authentication with ASP.NET Core Identity
- **Frontend:** Razor Views, custom CSS (no frontend framework), Bootstrap (minimal usage), vanilla JavaScript

### Project Structure

```
AriaPrint/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs      # Home page
│   ├── NewsController.cs      # News pages
│   ├── AccountController.cs   # Auth (login/register/logout)
│   ├── AdminPanelController.cs # Admin dashboard
│   ├── ProfileController.cs   # User profile
│   ├── RequestController.cs   # Submit request
│   ├── CommentsController.cs  # Comments/Q&A
│   └── AnswersController.cs   # Comment answers
│
├── Models/                # View Models
│   └── IndexViewModel.cs      # Home page data container
│
├── Repository/            # Domain entities & data models
│   ├── AppUser.cs             # User entity
│   ├── News.cs                # News article entity
│   ├── Comment.cs, Answer.cs  # Q&A entities
│   ├── ServicesCard.cs        # Service card model
│   ├── WhyUsCard.cs           # "Why us" feature model
│
├── BL/                    # Business Logic layer
│   ├── UserRepository.cs      # User data access (Dapper)
│   ├── PasswordService.cs     # Password hashing
│   └── FindNewsPageById.cs    # News lookup
│
├── Data Access Layer/     # Data fetching services
│   ├── SetIndexContent.cs     # Aggregates home page data
│   ├── SetNews.cs             # Fetches news from DB
│   ├── SetAllNewsPage.cs      # All news page data
│   ├── SetComments.cs         # Comments data
│   ├── SetAnswers.cs          # Answers data
│
├── Views/
│   ├── Home/Index.cshtml      # Home page (main landing)
│   ├── Shared/
│   │   ├── _Layout.cshtml         # Main layout (header + footer)
│   │   ├── _NewsCard.cshtml       # News card partial
│   │   ├── _ServicesCard.cshtml   # Service card partial
│   │   ├── _WhyUsCard.cshtml      # "Why us" card partial
│   │   ├── _CommentsSection.cshtml # Q&A section
│   │   ├── _SvgSprite.cshtml      # SVG icon definitions
│   │   └── _Hero3dLogo.cshtml     # Hero section 3D logo
│   ├── News/                  # News views
│   ├── Account/               # Login/Register views
│   ├── Profile/               # User profile views
│   ├── AdminPanel/            # Admin dashboard views
│   └── Request/               # Submit request views
│
└── wwwroot/
    ├── css/site.css           # Main stylesheet (5500+ lines)
    ├── css/home-redesign.css  # Home-page-only redesign, scoped under .home-page
    ├── js/
    │   ├── site.js            # Main JavaScript
    │   ├── starfield.js       # Canvas star animation
    │   └── submit-request.js  # Request form scripts
    ├── fonts/                 # Vazirmatn Persian font (woff2)
    ├── images/                # Static images
    ├── videos/                # Hero video
    └── lib/bootstrap/         # Bootstrap (minimal use)
```

### Database

- **Connection String:** In `appsettings.json` under `ConnectionStrings:DefaultConnection`
- **Database Name:** `AriaPrintDB`
- **Server:** Local SQL Server (`.`)
- **Auth:** Windows Integrated Security
- **ORM:** Dapper for all database operations

**Key Tables (inferred from code):**
- `UsersTbl` - User accounts (Id, FirstName, LastName, PhoneNumber, PasswordHash, Role)
- News, Comments, Answers tables

### Authentication Flow

- Cookie-based authentication with 14-day expiration
- Sliding expiration enabled
- Login path: `/Account/Login`
- Roles: "Admin" and regular users
- Admin user seeded on first run (phone: 09923365373, password: Aa@123456)

### Styling Approach

The site uses a custom **dark glassmorphism** design system defined entirely in `wwwroot/css/site.css`:

- **CSS Custom Properties (Design Tokens):** Colors, spacing, radii, shadows defined in `:root`
- **Navy/Deep blue palette:** `--navy-1000` through `--blue-500`
- **Glass effects:** `--glass-bg`, `--glass-border`, `backdrop-filter: blur()`
- **Persian font:** Vazirmatn (self-hosted woff2 files)
- **RTL layout:** All pages are RTL (`dir="rtl"`, `direction: rtl`)
- **No CSS framework:** Custom CSS throughout, minimal Bootstrap (only bundled for occasional utilities)

**Key CSS sections:**
- Design tokens: `:root` (lines 49-98)
- Header: `.site-header` (lines 435-577)
- Hero: `.hero` (lines 581-748)
- News cards: `.news-card` (lines 765-905)
- Services grid: `.services-section` (lines 952-1049)
- Why Us section: `.whyus-section` (lines 1053-1137)
- Footer: `.site-footer` (lines 2512-2676)

### Home Page Structure (Views/Home/Index.cshtml)

1. **Hero Section** (`.hero`) - Main landing with 3D logo animation
2. **News Section** (`.news-section`) - Latest 3 news cards
3. **Services Section** (`.services-section`) - 6 service cards in bento grid
4. **Why Us Section** (`.whyus-section`) - 4 feature cards with side image

The `_Layout.cshtml` wraps all pages with:
- Header (`.site-header`) - Logo, navigation, user menu/login button
- Footer (`.site-footer`) - Brand info, quick links, contact info, working hours

### JavaScript

Minimal JavaScript, mainly for:
- Mobile navigation toggle
- Header user dropdown menu
- Starfield canvas animation

## Development Notes

### Adding a new page
1. Create controller in `Controllers/`
2. Create view in `Views/[ControllerName]/`
3. If page should not show footer, set `ViewData["HideFooter"] = true` in the action

### Modifying styles
- All styles are in `wwwroot/css/site.css`
- Use existing CSS custom properties for consistency
- Maintain RTL support (`transform: scaleX(-1)` for flipped icons, `margin-inline` for directional spacing)

### Database changes
- Connection string in `appsettings.json`
- Use Dapper for data access (see `BL/UserRepository.cs` for pattern)
- Create data access classes in `Data Access Layer/`

### Working with Persian/RTL
- All text content is Persian
- Use logical properties (`margin-inline-start` instead of `margin-right`)
- Flip icons that have directional meaning: `transform: scaleX(-1)`
- Font: Vazirmatn (already loaded in CSS)
