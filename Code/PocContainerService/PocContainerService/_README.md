# Overview

A Proof-of-Concept .Net "Service" application for a Docker Container that also works with Kubernetes.

The purpose is to create a simple, long-running, cross-platform IHostedService application that can be the core of exploratory work with Ci/Cd pipelines, containerization, and K8. So it needs to do something while its running, allow people to interact/observe that "work", and support the various "health checks" that make it so Docker and Kubernetes can monitor it.

The whole point of choosing an Asp.Net application is to kill two birds with one stone. I find it best to have some "backend"/realtime API as the means to control and report on the service. Http is pretty ubiquitous, and the barrier-to-entry for a client application is pretty low. Once Http is chosen, it's relatively simple to add in the infrastructure health checks as additional endpoints.

I was going to implement the API as GraphQL, because I like it a **lot** better than RESTless APIs. However, not only do I think that it would make this POC unnecessarily complicated, but I haven't yet dug into [HotChocolate](https://chillicream.com/docs/hotchocolate) enough to decide how I want to use it.

This is just going to have a bunch of (lame) GET endpoints that return short bits of text.

**Tangentential opinion**: Nobody agrees what "true REST" really is. Consequently, I find it presumptuous when people say their stuff is "RESTful" or otherwise imply they got it right. I much prefer the term "RESTless" as I think it avoids that negative perception while being playful. But I also think REST is like Javascript; most people use it while wishing they could use something else.



# Health Checks
Notes about what to do, before I try to implement things.


## Docker Health Checks

Technically, it's just a command that's executed from within the container that returns either a 0 (good/healthy/ok) or 1 (bad/unhealthy/failure). You use [HEALTHCHECK](https://docs.docker.com/engine/reference/builder/#healthcheck) in a dockerfile and you can also override options later in a [compose .yml](https://docs.docker.com/compose/compose-file/compose-file-v3/) file or in [docker run](https://docs.docker.com/engine/reference/run/). 

As the **HEALTHCHECK** docs indicate, when the container starts up, the container will be reported as "starting" until the health check succeeds for the first time. Thereafter, if the health check either fails or takes too long to complete, it is switched to "unhealthy". Whenever a health check succeeds, the container is switched to "healthy". 

In the simplest of cases, for this POC, we just need to have an endpoint that returns a blank page, so we can support the most common [example command with the default port](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0#docker-example):  `curl -f -s http://localhost:5000/ || exit 1`

Or we could use this if we want to use [wget instead](https://stackoverflow.com/a/47722899/530545):  `wget --quiet --tries=1 --spider http://localhost:5000/ || exit 1`

For the future, it seems wise to have a command that can also "fail" if something other than an Http 200 status code is returned (including a 302 type response).



## Kubernetes Health Checks
K8 is a bit more complicated, since it supports more granularity with its 3 different [container probes](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#container-probes): [startup, readiness, and liveness](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/). 


With the Http probes directly supported, it's easy to configure the probe to check the Http endpoint. Success is any status code between 200 and 400; failure is anything else.

For this POC, we'll have the startup and liveness probes use the same endpoint as the Docker healthcheck one, but the startup probe will be more lenient on how long it tolerates failures. Given this, it also makes sense to have the readiness probe be the same as the liveness, since we aren't doing anything complicated enough to have to tell K8 to "pause" the use of our container---like having an external dependency.

