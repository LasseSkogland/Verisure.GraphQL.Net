# Verisure.GraphQL.Net (WIP)
This is very much a work in progress.
### Disclaimer
- This product is meant for educational purposes only.
- This is work in progress and subject to change.
- Void where prohibited.
- No other warranty expressed or implied.
- Some assembly required.
- Batteries not included.
- Use only as directed.
- Do not use while operating a motor vehicle or heavy equipment.

# Usage:
```csharp
// Instantiate VerisureClient and login
VerisureClient verisure = new VerisureClient("Email/Username", "Password as String Or SecureString");
await verisure.LoginAsync();

// Create Query
var query = new GraphQLQuery() {
    OperationName = "AccountInstallations",
    // QueryString gets created using Expressions, returns String
    Query = GraphQLQuery.CreateQueryString("query", () => new {
        // Parameters should come before Operation
        _params = "$email: String!",
        AccountInstallations = new {
            // Parameters should come before Operation
            _params = "email: $email",
            account = new {
                owainstallations = new[] { "giid", "alias", "type", "subsidiary", "dealerId" }
            }
        }
    }),
    // Variables gets created by Expressions, returns IDictionary<string, dynamic>
    Variables = GraphQLQuery.CreateVariables(() => new {
        email = "EMAIL"
    })
};
// Debug to see how the query looks
var dict = query.ToDictionary();
var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions() { WriteIndented = true });

// Make request to GraphQL endpoint and parse as json.
var res = await verisure.QueryAsync(query);
json = JsonSerializer.Serialize(res, new JsonSerializerOptions() { WriteIndented = true });
```
### Generated Request
```json
{
  "operationName": "AccountInstallations",
  "query": "query AccountInstallations($email: String!) {\n  account(email: $email) {\n    owainstallations {\n      giid\n      alias\n      type\n      subsidiary\n      dealerId\n    }\n  }\n}\n",
  "variables": {
    "email": "email"
  }
}
```
### Generated Query
```graphql
query AccountInstallations($email: String!) {
  account(email: $email) {
    owainstallations {
      giid
      alias
      type
      subsidiary
      dealerId
    }
  }
}

```

### Example Response
```json
{
  "data": {
    "account": {
      "owainstallations": [
        {
          "giid": "12345678910",
          "alias": "My house",
          "type": "GW",
          "subsidiary": null,
          "dealerId": "Don't do drugs kids"
        },
        {
          "giid": "12345678911",
          "alias": "Your house",
          "type": "GW",
          "subsidiary": null,
          "dealerId": "Don't do drugs kids"
        }
      ]
    }
  }
}
```