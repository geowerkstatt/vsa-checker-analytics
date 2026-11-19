# Holds only the plugin files and never runs: a host mounts plugin/ and resources/ read-only.
# Build from a clean `dotnet publish src/VSACheckerAnalytics/VSACheckerAnalytics.csproj -c Release -r linux-x64 -o publish`.
FROM scratch
LABEL org.opencontainers.image.source=https://github.com/geowerkstatt/vsa-checker-analytics
COPY publish/ /plugin/
COPY Resources/vsa-checker-analytics/ /resources/
