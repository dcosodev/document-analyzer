# 🎉 **Document Analyzer** 🖼️

¡Bienvenido a **Document Analyzer**, una poderosa API en .NET diseñada para analizar imágenes y documentos de manera eficiente! 🎯 Utilizando las capacidades de Azure, esta API puede extraer metadatos, verificar la autenticidad de las imágenes, detectar modificaciones y más. 📸📝

---

## 🚀 **Características Principales**

- 📷 **Análisis de Imágenes**: Extrae metadatos de las imágenes, como datos de geolocalización y detalles de la cámara.
- 🛡️ **Verificación de Autenticidad**: Asegura que las imágenes provengan de una cámara real y no de fuentes sospechosas.
- 🖼️ **Detección de Modificaciones**: Identifica si las imágenes han sido editadas o manipuladas.
- 📝 **Extracción de Datos de Documentos**: Con Azure Form Recognizer, se extraen datos clave de documentos como facturas o identificaciones.
- ☁️ **Almacenamiento en Azure Blob**: Guarda archivos en la nube de manera segura y accesible.

---

## 🛠️ **Requisitos del Sistema**

Para utilizar **Document Analyzer**, necesitarás:

- .NET 8.0 o superior 💻
- Cuenta de Azure con acceso a:
  - **Azure Blob Storage** ☁️
  - **Azure Form Recognizer** 🧾
  - **Azure IP Geolocation** 🌍
  - **Azure Computer Vision** 👁️

---

## 📦 **Instalación y Configuración**

### 1. Clonar el repositorio:

```bash
git clone https://github.com/dcosodev/document-analyzer.git
cd document-analyzer
```

### 2. Configurar los parámetros de conexión en el archivo `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Azure": {
    "AI": {
      "Key": "your_azure_key",
      "Endpoint": "https://your_azure_endpoint.cognitiveservices.azure.com/"
    },
    "DocumentIntelligence": {
      "Key": "your_form_recognizer_key",
      "Endpoint": "https://your_form_recognizer_endpoint.cognitiveservices.azure.com/"
    },
    "Storage": {
      "ConnectionString": "your_azure_blob_storage_connection_string"
    }
  },
  "IPGeolocation": {
    "ApiKey": "your_ip_geolocation_key",
    "Endpoint": "https://api.ipgeolocation.io/ipgeo"
  }
}
```

### 3. Construir y ejecutar el proyecto:

```bash
dotnet build
dotnet run
```

---

## 🖥️ **Uso de la API**

Con **Document Analyzer** en funcionamiento, puedes utilizar herramientas como Postman para hacer solicitudes a la API. Aquí algunos ejemplos:

### 📩 **Análisis de Imágenes**
**POST `/api/ImageAnalysis/analyze`**

Envía imágenes para analizar su autenticidad y si han sido modificadas.

- **Parámetros**:
  - `paths` (opcional): Arreglo de rutas de imágenes.
  - `gps` (opcional): Datos GPS.
  - `images` (opcional): Arreglo de archivos de imagen.
  - `clientCamera` (opcional): Indica si la imagen fue tomada con una cámara cliente.
  - `clientIP` (opcional): Dirección IP del cliente.
  - `type` (requerido): Tipo de documento a analizar (selfie, id, pasaporte, factura).

---

## 🔍 **Ejemplos de Uso**

### 1. **Análisis de una selfie**
```bash
POST /api/ImageAnalysis/analyze
{
  "type": "selfie",
  "images": [file.jpg]
}
```

### 2. **Análisis de un pasaporte**
```bash
POST /api/ImageAnalysis/analyze
{
  "type": "passport",
  "images": [file.jpg]
}
```

---

## 🛡️ **Seguridad**

Toda la comunicación con **Document Analyzer** está protegida mediante HTTPS. Asegúrate de proteger tus claves y tokens de Azure adecuadamente, evitando su exposición pública.

---

## 🧩 **Contribuciones**

¡Contribuciones son siempre bienvenidas! 🌟 Si tienes alguna idea de mejora o encontraste un error, no dudes en abrir un _issue_ o enviar un _pull request_. Juntos podemos hacer **Document Analyzer** aún mejor.

---

## 📜 **Licencia**

Este proyecto está bajo la licencia **MIT**. ¡Siéntete libre de usarlo y modificarlo como gustes! 🎉

---

Con esta guía, estás listo para empezar a usar **Document Analyzer**. ¡Disfruta de la automatización y análisis de imágenes con la potencia de .NET y Azure! 🚀
