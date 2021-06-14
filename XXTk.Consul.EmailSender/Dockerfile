#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["XXTk.Consul.EmailSender/XXTk.Consul.EmailSender.csproj", "XXTk.Consul.EmailSender/"]
RUN dotnet restore "XXTk.Consul.EmailSender/XXTk.Consul.EmailSender.csproj"
COPY . .
WORKDIR "/src/XXTk.Consul.EmailSender"
RUN dotnet build "XXTk.Consul.EmailSender.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XXTk.Consul.EmailSender.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XXTk.Consul.EmailSender.dll"]