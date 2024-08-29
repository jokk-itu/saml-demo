# saml-demo
Sample WebApp authenticating with KeyCloak using SAML

## Run

Use the run-keycloak.sh command to start KeyCloak in docker.
Login through the browser at http://localhost:8080,
where username is admin and password is admin.

Get the Clients certificate (with private key) and install in your certificate store.
Get the thumbprint and paste it into samlwebapp/appsettings.json,
into the Saml2:ServiceProviderConfiguration:Certificate:Thumbprint line.

Get Keycloaks certificate (without private key) and install in your certificate store.
Get the thumbprint and paste it into samlwebapp/appsettings.json,
into the Saml2:IdentityProviderConfiguration:Certificate:Thumbprint line.

Run the WebApp using <code>dotnet run</code>.
Navigate to https://localhost:7006,
and click on the "Claims" navigation.
This will trigger the default authentication scheme,
and lets you login through KeyCloak.