# Use an official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory inside the container
WORKDIR /app

# Copy the published app files into the container
COPY publish/ .

# Expose the necessary port
EXPOSE 8080

# Start the app using the DLL
CMD ["dotnet", "hospitalManagment.dll"]
