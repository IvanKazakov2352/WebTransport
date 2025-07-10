certs:
	openssl ecparam -name prime256v1 -genkey -noout -out ./ssl/tls.key
	openssl req -new -key ./ssl/tls.key -out ./ssl/tls.csr -subj "/C=RU/ST=Moscow/L=Moscow/O=MyOrg/OU=MyUnit/CN=localhost" -addext "subjectAltName = DNS:localhost,IP:127.0.0.1" -addext "extendedKeyUsage = serverAuth"
	openssl x509 -req -in ./ssl/tls.csr -signkey ./ssl/tls.key -out ./ssl/tls.crt -days 365
	openssl pkcs12 -export -inkey ./ssl/tls.key -in ./ssl/tls.crt -out ./ssl/certificate.pfx -passout pass:localhost
dotnet-certs:
	dotnet dev-certs https --export-path ./ssl/cert.pfx --password localhost