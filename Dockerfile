FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files for layer caching
COPY src/building-blocks/Core/Educore.Core.csproj src/building-blocks/Core/
COPY src/building-blocks/Shared/Educore.Shared.csproj src/building-blocks/Shared/
COPY src/building-blocks/Database/Educore.Database.csproj src/building-blocks/Database/
COPY src/modules/Organization/Educore.Organization.csproj src/modules/Organization/
COPY src/modules/Secretary/Educore.Secretary.csproj src/modules/Secretary/
COPY src/modules/Academic/Educore.Academic.csproj src/modules/Academic/
COPY src/modules/Grading/Educore.Grading.csproj src/modules/Grading/
COPY src/modules/Financial/Educore.Financial.csproj src/modules/Financial/
COPY src/api/Gateway/Educore.Api.csproj src/api/Gateway/
COPY tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj tests/Financial.IntegrationTests/
RUN dotnet restore src/api/Gateway/Educore.Api.csproj && \
    dotnet restore tests/Financial.IntegrationTests/EduCore.Financial.IntegrationTests.csproj

# Copy everything
COPY . .

# Build
RUN dotnet build --configuration Release --no-restore

# ──────────────────────────────────────────────────
# Stage 1: Test Runner — usado em homologação
# ──────────────────────────────────────────────────
FROM build AS test-runner
WORKDIR /app
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
CMD ["bash", "scripts/deploy-pipeline.sh"]

# ──────────────────────────────────────────────────
# Stage 2: Produção — app final
# ──────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish src/api/Gateway/Educore.Api.csproj \
    --configuration Release \
    --no-build \
    -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /out .
ENTRYPOINT ["dotnet", "Educore.Api.dll"]
