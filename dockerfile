FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build
WORKDIR /scr

COPY --from=mcr.microsoft.com/dotnet/sdk:6.0 /usr/share/dotnet/shared /usr/share/dotnet/shared

ARG SONAR_TOKEN
ARG PULL_REQUEST_ID
ARG PULL_REQUEST_SOURCE_BRANCH
ARG PULL_REQUEST_TARGET_BRANCH
ARG GITHUB_RUN_ID

ENV SONAR_TOKEN=${SONAR_TOKEN}
ENV PULL_REQUEST_ID=${PULL_REQUEST_ID}
ENV PULL_REQUEST_SOURCE_BRANCH=${PULL_REQUEST_SOURCE_BRANCH}
ENV PULL_REQUEST_TARGET_BRANCH=${PULL_REQUEST_TARGET_BRANCH}
ENV GITHUB_RUN_ID=${GITHUB_RUN_ID}

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
    ant dos2unix ca-certificates-java dotnet-sdk-6.0 dotnet-sdk-7.0 && \
    apt-get -y autoremove && \
    apt-get clean autoclean

# Fix certificate issues
RUN update-ca-certificates -f

ENV JAVA_HOME /usr/lib/jvm/jdk-21.0.1
RUN export JAVA_HOME=/usr/lib/jvm/jdk-21.0.1
RUN export PATH=$JAVA_HOME/bin:$PATH

RUN dotnet new tool-manifest
RUN dotnet tool install dotnet-sonarscanner --tool-path /tools --version 5.14.0
RUN dotnet tool install snitch --tool-path /tools --version 1.12.0
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path /tools --version 5.2.0
RUN dotnet tool install JetBrains.dotCover.GlobalTool --tool-path /tool --version 2023.2.3

RUN dotnet tool restore

RUN echo "##vso[task.prependpath]$HOME/.dotnet/tools"
RUN export PATH="$PATH:/root/.dotnet/tools"

RUN echo '--->PULL_REQUEST_ID:' ${PULL_REQUEST_ID}
RUN echo '--->GITHUB_RUN_ID:' ${GITHUB_RUN_ID}
RUN echo '--->PULL_REQUEST_SOURCE_BRANCH:' ${PULL_REQUEST_SOURCE_BRANCH}
RUN echo '--->PULL_REQUEST_TARGET_BRANCH:' ${PULL_REQUEST_TARGET_BRANCH}

COPY ["HomeBudget.Components.CurrencyRates.Tests/*.csproj", "HomeBudget.Components.CurrencyRates.Tests/"]
COPY ["HomeBudget.Components.IntegrationTests/*.csproj", "HomeBudget.Components.IntegrationTests/"]
COPY ["HomeBudget.Components.CurrencyRates/*.csproj", "HomeBudget.Components.CurrencyRates/"]
COPY ["HomeBudget.DataAccess/*.csproj", "HomeBudget.DataAccess/"]
COPY ["HomeBudget.Core/*.csproj", "HomeBudget.Core/"]
COPY ["HomeBudget.Rates.Api/*.csproj", "HomeBudget.Rates.Api/"]
COPY ["HomeBudget.DataAccess.Dapper/*.csproj", "HomeBudget.DataAccess.Dapper/"]

COPY ["test-results/*", "test-results/"]
COPY ["startsonar.sh", "startsonar.sh"]

COPY ["HomeBudgetRatesApi.sln", "HomeBudgetRatesApi.sln"]

COPY . .

RUN dos2unix ./startsonar.sh
RUN chmod +x ./startsonar.sh
RUN ./startsonar.sh;

RUN dotnet build HomeBudgetRatesApi.sln -c Release -o /app/build /maxcpucount:1

LABEL build_version="${BUILD_VERSION}"
LABEL service=CurrencyRatesService

RUN dotnet dev-certs https --trust

RUN dotnet dotcover test HomeBudgetRatesApi.sln --dcReportType=HTML --dcOutput="test-results/rates-coverage.html"

RUN /tools/reportgenerator \
    -reports:'test-results/rates-coverage.xml' \
    -targetdir:'test-results/' \
    -reporttypes:'dotCover';

RUN cat test-results/SonarQube.xml

RUN /tools/dotnet-sonarscanner end /d:sonar.token="${SONAR_TOKEN}";

RUN /tools/snitch

FROM build AS publish
RUN dotnet publish HomeBudgetRatesApi.sln \
    --no-dependencies \
    --no-restore \
    --framework net7.0 \
    -c Release \
    -v Diagnostic \
    -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HomeBudget.Rates.Api.dll"]