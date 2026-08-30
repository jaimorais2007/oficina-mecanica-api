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

ARG OTEL_EXPORTER_OTLP_HEADERS

ENV OTEL_SERVICE_NAME=oficina-mecanica 
ENV OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.nr-data.net 
ENV OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT=4095 
ENV OTEL_EXPORTER_OTLP_COMPRESSION=gzip 
ENV OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf 
ENV OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE=delta 
ENV OTEL_EXPORTER_OTLP_HEADERS=$OTEL_EXPORTER_OTLP_HEADERS

RUN addgroup --system appgroup \
 && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=builder /publish .

EXPOSE $PORT
ENTRYPOINT ["dotnet", "OficinaApi.Presentation.dll"]
