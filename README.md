# v-commerce

eCommerce platform built with an Automation Agentic AI, using GCP Vertex AI and GitHub Copilot Coding Agent.

## 🏗️ Architecture

This project follows a **production-ready, best-practice .NET 10 architecture** combining:

- **Modular Monolithic Architecture**: Self-contained modules with clear boundaries
- **Vertical Slice Architecture**: Features organized by use case, not technical layers
- **CQRS Pattern**: Separation of commands and queries using MediatR 12.4.1
- **Domain-Driven Design**: Pragmatic DDD approach with rich domain models
- **Entity Framework Core 10**: Code-First approach with manual migrations
- **Unit Testing**: Comprehensive test coverage with xUnit, FluentAssertions, and Moq

## 📁 Project Structure

```
v-commerce/
├── src/
│   ├── VCommerce.Api/              # API entry point (Minimal APIs)
│   ├── VCommerce.Shared/           # Shared abstractions and common code
│   ├── VCommerce.Infrastructure/   # Cross-cutting infrastructure (EF Core, DbContext)
│   └── Modules/
│       └── Products/               # Products module (example)
├── tests/
│   └── VCommerce.Tests.Unit/       # Unit tests
└── .github/
    └── copilot-instructions.md     # AI Agent guidelines
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Your favorite IDE (Visual Studio 2025, VS Code, or Rider)

### Build and Run

```bash
# Clone the repository
git clone https://github.com/alshadev/v-commerce.git
cd v-commerce

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run --project src/VCommerce.Api
```

The API will be available at `https://localhost:5001` (or the port specified in the console output).

### API Documentation

When running in development mode, OpenAPI documentation is available at:
- OpenAPI JSON: `https://localhost:5001/openapi/v1.json`

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/VCommerce.Tests.Unit
```

## 📚 Architecture Documentation

For detailed architecture guidelines, patterns, and best practices, see [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

## 🔧 Technology Stack

- **.NET 10**: Latest framework
- **ASP.NET Core**: Minimal APIs
- **Entity Framework Core 10**: ORM
- **MediatR 12.4.1**: CQRS implementation
- **xUnit**: Testing framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework

## 📦 Modules

### Products Module

The Products module demonstrates the architecture with a complete CRUD implementation:

**Features:**
- Create Product
- Get Product by ID
- Get All Products
- Update Product
- Delete Product

**API Endpoints:**
- `POST /api/products` - Create a new product
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

## 🎯 Key Principles

1. **Vertical Slice Architecture**: Each feature is self-contained
2. **CQRS**: Commands modify state, Queries retrieve data
3. **Rich Domain Models**: Business logic encapsulated in entities
4. **Result Pattern**: Explicit error handling without exceptions
5. **Testability**: Easy to test with dependency injection
6. **Modularity**: Easy to add new modules without affecting existing ones

## 🤖 AI Agent Integration

This project is designed to work seamlessly with AI coding agents. The `.github/copilot-instructions.md` file contains comprehensive guidelines for AI agents to:

- Understand the architecture
- Follow established patterns
- Add new features consistently
- Maintain code quality
- Write appropriate tests

## 📝 Contributing

1. Follow the architectural patterns defined in `.github/copilot-instructions.md`
2. Write unit tests for new features
3. Ensure all tests pass before submitting
4. Follow C# coding conventions

## 📄 License

This project is licensed under the MIT License.

## 🙋 Support

For questions or issues, please open an issue in the GitHub repository.
