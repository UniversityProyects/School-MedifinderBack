# MedifinderBack

## Versión: 1.0.0

- **Autor:** Jessica Delgado
- **Fecha:** 19/09/2024
- **Descripcion:**
  - Se inicializa proyecto

## Configuración Técnica

- **IDE:** Visual Studio 2022

## Configuración de la Conexión a la Base de Datos

1. Abre el archivo `appsettings.json` que se encuentra en la raíz del proyecto.
2. Busca la sección `ConnectionStrings` y modifica la cadena de conexión para que apunte a tu servidor de base de datos. Debe tener el siguiente formato:
   ```csharp
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=tu_servidor;Database=tu_base_de_datos;User Id=tu_usuario;Password=tu_contraseña;"
      }
    }

   ```

## Configuración de la Dirección IP y Puerto

1. Abre el archivo Program.cs en la raíz del proyecto.
2. Busca la configuración de la variable `host` y modificala.
3. Reemplaza `tu_direccion_ip` y `puerto` con la dirección IP y el puerto deseados.


    - **Ejemplo:** `var host = "http://localhost:5257";`
