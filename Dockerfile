FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /app

COPY *.sln .
COPY src/ ./src/
COPY tests/ ./tests/
RUN dotnet restore

RUN dotnet publish src/OficinaApi.WebApi/OficinaApi.WebApi.csproj \
    -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runner
WORKDIR /app

RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=builder /publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "OficinaApi.WebApi.dll"]
