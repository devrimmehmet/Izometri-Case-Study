FROM debian:12-slim AS base
WORKDIR /app
EXPOSE 8080

# Use Debian + Microsoft apt packages instead of MCR base images.
# This avoids docker build failures on networks where Docker can reach Docker Hub
# but MCR manifest/blob requests are reset, which was reproducible on this case machine.
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

# Build stage uses the same apt-based strategy so compose no longer depends on
# pulling SDK layers from mcr.microsoft.com during evaluation.
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
RUN dotnet publish src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseService.Api.dll"]
