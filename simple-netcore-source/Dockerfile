FROM registry.access.redhat.com/ubi8/dotnet-31

ENV ASPNETCORE_URLS=http://*:8080

WORKDIR /app
COPY . .

USER 0
RUN dotnet restore
RUN dotnet build
RUN dotnet publish -o /app/build/PublishOutput
RUN chown -R 1001:0 /app && chmod -R og+rwx /app

LABEL "org.springframework.cloud.dataflow.spring.configuration.metadata.json"="{  \"groups\": [{    \"name\": \"simple-netcore-source\",    \"type\": \"org.springframework.cloud.stream.app.simple-netcore-source.source.SimpleNetcoreSourceProcessorProperties\",    \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-source.source.SimpleNetcoreSourceProcessorProperties\"  }],  \"properties\": [    {      \"name\": \"simpleNetcoreProcessor.filePath\",      \"type\": \"java.lang.String\",      \"description\": \"An external Kafka topic to read as table for caputiring message enhancing data.\",      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-source.source.SimpleNetcoreSourceProcessorProperties\",      \"defaultValue\": \"products\"    },    {      \"name\": \"simpleNetcoreProcessor.table\",      \"type\": \"java.lang.String\",      \"description\": \"The enhacing data table name.\",      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-source.source.SimpleNetcoreSourceProcessorProperties\"    }      ],  \"hints\": []}"
USER 1001
ENTRYPOINT ["dotnet","build/PublishOutput/simple-netcore-source.dll"]
