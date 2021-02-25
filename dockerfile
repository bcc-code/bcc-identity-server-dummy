FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app


# Copy project file over and restore as distinct layers
COPY . ./
RUN dotnet restore ./Bcc.Members.Identity.sln

RUN dotnet publish ./src/Bcc.Members.Identity.Domain -c Release -o out --no-restore


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /App
COPY --from=build-env /app/out .

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Bcc.Members.Identity.Domain.dll"]