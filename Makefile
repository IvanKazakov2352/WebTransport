create-cert:
	mkdir credentials
	openssl genpkey -algorithm RSA -out ./credentials/key.pem -pkeyopt rsa_keygen_bits:2048
	openssl req -new -key ./credentials/key.pem -out ./credentials/request.csr -subj "/C=US/ST=State/L=City/O=Organization/OU=Department/CN=localhost"
	openssl x509 -req -in ./credentials/request.csr -signkey ./credentials/key.pem -out ./credentials/cert.pem -days 365
	openssl pkcs12 -export -out ./credentials/certificate.pfx -inkey ./credentials/key.pem -in ./credentials/cert.pem -passout pass:localhost