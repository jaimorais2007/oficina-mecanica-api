FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app

COPY *.sln .
COPY src/ ./src/
COPY tests/ ./tests/
RUN dotnet restore

RUN dotnet publish src/OficinaApi.Presentation/OficinaApi.Presentation.csproj \
    -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runner
WORKDIR /app

ARG JWT_SECRET
ARG EMAIL_PASSWORD
ARG DB_CONNECTION_STRING
ARG DOTNET_ENVIRONMENT=Development
ARG PORT=8080

ENV Jwt__Secret=$JWT_SECRET
ENV EmailSettings__Password=$EMAIL_PASSWORD
ENV ConnectionStrings__DefaultConnection=$DB_CONNECTION_STRING
ENV ASPNETCORE_ENVIRONMENT=$DOTNET_ENVIRONMENT
ENV ASPNETCORE_HTTP_PORTS=$PORT
RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=builder /publish .

EXPOSE $PORT
ENTRYPOINT ["dotnet", "OficinaApi.Presentation.dll"]
