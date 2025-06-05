docker build -t blazorapp1 .


# For MacOS please use the following command to run the container 
docker run -d -p 8443:443 -v /Users/$USER/https:/https --name blazorapp1container blazorapp1

# For Windows please use the following command to run the container
docker run -d -p 8443:443 -v C:\Users\%USERNAME%\https:/https --name blazorapp1container blazorapp1




# Hardcoded path for my MacOS 
docker run -d -p 8443:443 -v /Users/momoamer/https:/https --name blazorapp1container blazorapp1

