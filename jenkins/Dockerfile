
FROM jenkins/jenkins:lts-jdk17

USER root
RUN apt-get update && apt-get install -y docker.io && rm -rf /var/lib/apt/lists/*

RUN jenkins-plugin-cli --plugins \
    workflow-aggregator \
    git \
    docker-workflow \
    pipeline-stage-view \
    credentials-binding \
    junit \
    timestamper


USER root
