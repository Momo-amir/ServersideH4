docker build -t blazorapp1 .

docker run -d -p 8443:443 -v /Users/momoamer/https:/https --name blazorapp1container blazorapp1

