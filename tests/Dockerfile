FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files for layer caching
COPY src/building-blocks/Core/Educore.Core.csproj src/building-blocks/Core/
COPY src/building-blocks/Shared/Educore.Shared.csproj src/building-blocks/Shared/
COPY src/building-blocks/Database/Educore.Database.csproj src/building-blocks/Database/
COPY src/modules/Financial/Educore.Financial.csproj src/modules/Financial/
COPY tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj tests/Financial.IntegrationTests/

# Restore (cache layer)
RUN dotnet restore tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj

# Copy everything and build
COPY . .

FROM build AS test
WORKDIR /app

# Default: run all tests, excluding PostgreSQL tests (they need external DB)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
CMD dotnet test tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj \
    --configuration Release \
    --no-restore \
    --filter "FullyQualifiedName!~Postgres" \
    --logger "console;verbosity=detailed"
