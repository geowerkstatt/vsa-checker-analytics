# VSA Checker Analytics

## Architecture

A description of the pipeline architecture and a flow chart of the processors can be found in the [architecture documentation](docs/architektur.md) (in German).

Detailed process documentation:

- [VSA Matcher](docs/vsa-matcher.md) -- configuration, inputs/outputs, file identification and validation rules

## Setup

The project references the [GeoWerkstatt.Geopilot.PipelineCore](https://github.com/geowerkstatt/geopilot/pkgs/nuget/GeoWerkstatt.Geopilot.PipelineCore/) NuGet package, published on the GitHub registry.
The GitHub registry requires every user to authenticate before they can pull packages. Because of that, you need add authentication to your NuGet configuration before pulling the package.
You can use the following command to add the authentication:
```bash
dotnet nuget add source https://nuget.pkg.github.com/GeoWerkstatt/index.json \
  --name github-geowerkstatt \
  --username your-github-username \
  --password ghp_yourPatGoesHere \
  --store-password-in-clear-text
```

You need to create a personal access token (PAT) with the `read:packages` scope. For more information on how to create a PAT, see the [GitHub documentation](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token).
