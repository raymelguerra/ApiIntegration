# ApiIntegration

# run migrations

- Create a new migrations
  
`dotnet ef migrations add InitialCreate --project ../Infrastructure/Infrastructure.csproj --startup-project ./Api.csproj -o Persistence/Migrations`

- Update database
  
`dotnet ef database update --startup-project ./Api.csproj`