FROM microsoft/dotnet:2.0.5-sdk-2.1.4-jessie
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/MailQ.dll"]



