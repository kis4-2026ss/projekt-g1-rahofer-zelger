FROM python:3.11-slim

FROM python:3.11-slim

# 1. Install System Dependencies & .NET SDK Dependencies
RUN apt-get update && apt-get install -y \
    curl \
    docker.io \
    wget \
    libicu-dev \
    # Avalonia/GUI build dependencies (often needed for headless testing)
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# 2. Install .NET 8.0 SDK (Current Standard for Avalonia)
# Install .NET 8 SDK
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet && \
    rm dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH=$PATH:/usr/share/dotnet

WORKDIR /app

# 4. Python Setup
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["python", "src/crew_assembly.py"]