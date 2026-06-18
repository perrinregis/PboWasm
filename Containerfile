# Utiliser l'image officielle du SDK .NET 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0

# Installer Java JDK (requis pour le SDK Android) et les outils nécessaires
RUN apt-get update && apt-get install -y openjdk-17-jdk wget unzip && rm -rf /var/lib/apt/lists/*

# Définir les variables d'environnement du SDK Android
ENV ANDROID_HOME=/usr/local/android-sdk
ENV PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools

# Télécharger et installer les outils en ligne de commande Android (cmdline-tools)
RUN mkdir -p $ANDROID_HOME/cmdline-tools && \
    wget -q https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip -O /tmp/cmdline-tools.zip && \
    unzip -q /tmp/cmdline-tools.zip -d /tmp && \
    mv /tmp/cmdline-tools $ANDROID_HOME/cmdline-tools/latest && \
    rm -f /tmp/cmdline-tools.zip

# Accepter les licences Android et installer les packages requis (Platform API 34 & build-tools)
RUN yes | sdkmanager --licenses && \
    sdkmanager "platform-tools" "platforms;android-34" "build-tools;34.0.0"

# Installer la charge de travail MAUI Android en ignorant la mise à jour des manifestes publicitaires (--skip-manifest-update)
RUN dotnet workload install maui-android --skip-manifest-update --source https://api.nuget.org/v3/index.json

# Définir le répertoire de travail dans le conteneur
WORKDIR /app

# Par défaut, compile le projet MAUI en Release et dépose le résultat dans /app/publish
CMD ["dotnet", "publish", "PboWasm.Maui/PboWasm.Maui.csproj", "-f", "net10.0-android", "-c", "Release", "-o", "/app/publish"]
