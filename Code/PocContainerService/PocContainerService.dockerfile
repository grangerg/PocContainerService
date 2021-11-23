# escape=`
#
#		https://docs.docker.com/engine/reference/builder/
#
#		From Windows (in this folder), build the app:
#			.\_deploy.ps1 -RuntimeId linux-musl-x64 -SelfContained $true
#		From WSL2 Ubuntu Linux (/mnt/c is readonly), build the container image: 
#			docker build -t gg-poc-cs -f /mnt/c/{this-folder-in-windows}/PocContainerService.dockerfile /mnt/c/{this-folder-in-windows}/BuildArtifacts
#		After the image successfully builds, run it (and auto-delete when it exits):
#			docker run -d --rm --name poccs -p 8080:80 gg-poc-cs
#		Then you can look at things:
#			curl http://localhost:8080/
#			curl http://localhost:8080/info
#		And once you're bored, stop it:
#			docker conatiner stop poccs
#				or
#			curl http://localhost:8080/shutdown
#
# Alpine Linux container for our POC service
#	We don't have TLS/SSL/Https setup in the service, so we don't bother config'ing and exposing that port.
#
FROM alpine:latest

# We use curl for the health-check (~250kiB)
RUN apk add curl

# Without these, you get "error loading shared library" logs for both "libgcc_s.so.1" and "libstdc++.so.6"
RUN apk add libgcc libstdc++

# Without this, you get this error: Couldn't find a valid ICU package installed on the system. Please install libicu using your package manager and try again. Alternatively you can set the configuration flag System.Globalization.Invariant to true if you want to run with no globalization support. Please see https://aka.ms/dotnet-missing-libicu for more information.
# And I don't feel like following that alternative advice.
RUN apk add icu

# Src folder is relative to what's given to "docker build"
COPY ["/", "/srv/PocContainerService/"]
# Linux needs to think it's an executable, like Windows does.
RUN chmod +x /srv/PocContainerService/PocContainerService

# Make it use the normal ports 
# (can also do ASPNETCORE_URLS='...', but I find directly overriding appsettings.json config less confusing in the long run.)
ENV Kestrel__Endpoints__Http__Url=http://*:80/
#ENV Kestrel__Endpoints__Https__Url=https://*:443/

# Then expose them.
EXPOSE 80/tcp
#EXPOSE 443/tcp

# It would be better if the first check was at "--start-period" instead of "--interval".
# We now use "/Ready" because "/" is heavier to execute now that it generates an Html page.
HEALTHCHECK --interval=10s CMD curl -f -s http://localhost/ready || exit 1

# When sshd exits, the container stops.
ENTRYPOINT ["/srv/PocContainerService/PocContainerService"]
