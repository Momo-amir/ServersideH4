docker build -t blazorapp1 .

docker run -d \
-p 8080:80 -p 8443:443 \
--name blazorapp1 \
-v /Users/momoamer/https/devcert.pfx:/https/devcert.pfx \
blazorapp1


