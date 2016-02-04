
/bin/rm server.*

log_file=cert.log

openssl genrsa -des3 -passout pass:x -out server.pass.key 2048 >$log_file 2>&1

openssl rsa -passin pass:x -in server.pass.key -out server.key >>$log_file 2>&1

/bin/rm server.pass.key >>$log_file 2>&1

openssl req -new -key server.key -out server.csr >>$log_file 2>&1 <<!
US
Texas

j64

j64Server.local



!

openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt >>$log_file 2>&1


echo "The server.crt file is you site certificate along with the server.key private key"
