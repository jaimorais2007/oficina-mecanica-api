FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /app

COPY *.sln .
COPY src/ ./src/
RUN dotnet restore

RUN dotnet publish src/OficinaApi/OficinaApi.csproj \
    -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runner
WORKDIR /app

RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=builder /publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "OficinaApi.dll"]
