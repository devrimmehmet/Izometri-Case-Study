FROM debian:12-slim AS base
WORKDIR /app
EXPOSE 8080

# Use Debian + Microsoft apt packages instead of MCR base images.
# This keeps compose builds working on machines where Docker Hub is reachable
# but mcr.microsoft.com pulls fail with EOF / connection reset.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL -o /tmp/packages-microsoft-prod.deb https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends aspnetcore-runtime-10.0 \
    && rm -rf /var/lib/apt/lists/*

FROM debian:12-slim AS build
WORKDIR /src

# Build stage uses apt-installed .NET SDK to remove the MCR dependency from
# the case evaluation flow.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL -o /tmp/packages-microsoft-prod.deb https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends dotnet-sdk-10.0 \
    && rm -rf /var/lib/apt/lists/*

COPY . .
RUN dotnet restore Izometri.CaseStudy.slnx
RUN dotnet publish src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
