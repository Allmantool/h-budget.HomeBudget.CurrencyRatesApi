FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /scr

COPY --from=mcr.microsoft.com/dotnet/sdk:9.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

ARG BUILD_VERSION
ENV BUILD_VERSION=${BUILD_VERSION}

RUN --mount=type=cache,target=/var/cache/apt \
    apt-get update && \
    apt-get install -y --quiet --no-install-recommends \
    apt-transport-https && \
    apt-get -y autoremove && \
    apt-get clean autoclean

RUN wget https://download.oracle.com/java/21/latest/jdk-21_linux-x64_bin.tar.gz -O jdk-21_linux-x64_bin.tar.gz
RUN mkdir /usr/lib/jvm && \
    tar -xvf jdk-21_linux-x64_bin.tar.gz -C /usr/lib/jvm

RUN --mount=type=cache,target=/var/cache/apt \
    apt-get update && \   
    apt-get install -f -y --quiet --no-install-recommends \
    ant ca-certificates-java && \
    apt-get -y autoremove && \
    apt-get clean autoclean

# Fix certificate issues
RUN update-ca-certificates -f

ENV JAVA_HOME /usr/lib/jvm/jdk-21.0.1
RUN export JAVA_HOME=/usr/lib/jvm/jdk-21.0.1
RUN export PATH=$JAVA_HOME/bin:$PATH

RUN dotnet new tool-manifest

# Not compatible with .net 9.0 (will be updated later)
# RUN dotnet tool install snitch --tool-path /tools --version 2.0.0

RUN dotnet tool restore

RUN echo "##vso[task.prependpath]$HOME/.dotnet/tools"
RUN export PATH="$PATH:/root/.dotnet/tools"

COPY ["HomeBudget.Components.CurrencyRates.Tests/*.csproj", "HomeBudget.Components.CurrencyRates.Tests/"]
COPY ["HomeBudget.Components.IntegrationTests/*.csproj", "HomeBudget.Components.IntegrationTests/"]
COPY ["HomeBudget.Rates.Api.Tests/*.csproj", "HomeBudget.Rates.Api.Tests/"]

COPY ["HomeBudget.Components.CurrencyRates/*.csproj", "HomeBudget.Components.CurrencyRates/"]
COPY ["HomeBudget.Components.Exchange/*.csproj", "HomeBudget.Components.Exchange/"]
COPY ["HomeBudget.DataAccess/*.csproj", "HomeBudget.DataAccess/"]
COPY ["HomeBudget.Core/*.csproj", "HomeBudget.Core/"]
COPY ["HomeBudget.Rates.Api/*.csproj", "HomeBudget.Rates.Api/"]
COPY ["HomeBudget.DataAccess.Dapper/*.csproj", "HomeBudget.DataAccess.Dapper/"]

COPY ["HomeBudgetRatesApi.sln", "HomeBudgetRatesApi.sln"]

COPY ["startsonar.sh", "startsonar.sh"]

COPY . .

# Clean artifacts from test projects
RUN dotnet sln HomeBudgetRatesApi.sln remove \
    HomeBudget.Components.IntegrationTests\HomeBudget.Components.IntegrationTests.csproj \
    HomeBudget.Components.CurrencyRates.Tests\HomeBudget.Components.CurrencyRates.Tests.csproj \
    HomeBudget.Rates.Api.Tests\HomeBudget.Rates.Api.Tests.csproj

RUN dotnet build HomeBudgetRatesApi.sln -c Release --no-incremental --framework:net9.0 -maxcpucount:1 -o /app/build

# Not compatible with .net 9.0 (will be updated later)
# RUN /tools/snitch

FROM build AS publish
RUN dotnet publish HomeBudgetRatesApi.sln \
    --no-dependencies \
    --no-restore \
    --framework net9.0 \
    -c Release \
    -v Diagnostic \
    -o /app/publish

FROM base AS final
WORKDIR /app
LABEL build_version="${BUILD_VERSION}"
LABEL service=CurrencyRatesService
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HomeBudget.Rates.Api.dll"]