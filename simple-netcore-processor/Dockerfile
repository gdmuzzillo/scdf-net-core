FROM registry.access.redhat.com/ubi8/dotnet-31
WORKDIR /app
COPY . .

#RUN ls -lart
#RUN ln -s /lib/x86_64-linux-gnu/libdl-2.24.so /lib/x86_64-linux-gnu/libdl.so
#RUN apt-get update
#RUN apt-get install -y wget
#RUN apt-get install -y libcurl4
#RUN apt-get install -y libcurl4-openssl-dev

#RUN wget https://dot.net/v1/dotnet-install.sh

#RUN chmod 777 dotnet-install.sh

#RUN ./dotnet-install.sh -v 3.1.102

#ENV PATH=/root/.dotnet:$PATH
# build netcore project and publish
USER 0
RUN dotnet restore
RUN dotnet build
RUN dotnet publish -o /app/build/PublishOutput
#RUN chmod -R 766  /app/build/PublishOutput
#RUN chown -R 1001:1001 /app/build/PublishOutput
LABEL org.springframework.cloud.dataflow.spring.configuration.metadata.json="{  \"groups\": [{    \"name\": \"simple-netcore-processor\",    \"type\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\",    \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\"  }],  \"properties\": [    {      \"name\": \"simpleNetcoreProcessor.externaltopic\",      \"type\": \"java.lang.String\",      \"description\": \"An external Kafka topic to read as table for caputiring message enhancing data.\",      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\",      \"defaultValue\": \"products\"    },    {      \"name\": \"simpleNetcoreProcessor.table\",      \"type\": \"java.lang.String\",      \"description\": \"The enhacing data table name.\",      \"sourceType\": \"org.springframework.cloud.stream.app.simple-netcore-processor.processor.SimpleNetcoreProcessorProcessorProperties\"    }      ],  \"hints\": []}"
USER 1001
ENTRYPOINT ["dotnet","build/PublishOutput/simple-netcore-processor.dll"]
