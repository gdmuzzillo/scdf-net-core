FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic
WORKDIR /app
COPY . .

RUN ls -lart
RUN ln -s /lib/x86_64-linux-gnu/libdl-2.24.so /lib/x86_64-linux-gnu/libdl.so
RUN apt-get update
RUN apt-get install -y wget
RUN apt-get install -y libcurl4
RUN apt-get install -y libcurl4-openssl-dev

RUN wget https://dot.net/v1/dotnet-install.sh

RUN chmod 777 dotnet-install.sh

RUN ./dotnet-install.sh -v 3.1.102

ENV PATH=/root/.dotnet:$PATH
# build netcore project and publish
RUN dotnet restore
RUN dotnet build
RUN dotnet publish 
LABEL "org.springframework.cloud.dataflow.spring.configuration.metadata.json"="{\n  \"groups\": [{\n    \"name\": \"simple-netcore-processor\",\n    \"type\": \"org.springframework.cloud.stream.app.simple-netocre-processor.processor.SimpleNetcoreProcessorProcessorkProperties\",\n    \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\"\n  }],\n  \"properties\": [\n    {\n      \"name\": \"simpleNetcoreProcessor.externaltopic\",\n      \"type\": \"java.lang.String\",\n      \"description\": \"An external Kafka topic to read as table for caputiring message enhancing data.\",\n      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\",\n      \"defaultValue\": \"products\"\n    },\n    {\n      \"name\": \"simpleNetcoreProcessor.table\",\n      \"type\": \"java.lang.String\",\n      \"description\": \"The enhacing data table name.\",\n      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\"\n    }\n      ],\n  \"hints\": []\n}"
ENTRYPOINT ["dotnet","build/PublishOutput/simple-netcore-processor.dll"]
