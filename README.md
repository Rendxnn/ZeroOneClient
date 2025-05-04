# ZeroOneClient

Cliente .NET para interactuar con la API de ZeroOne de manera simple y efectiva.

## Descripción

ZeroOneClient es una librería que facilita la integración con la API de ZeroOne, permitiendo realizar operaciones como:

- Autenticación automática
- Consulta de datos mediante vistas
- Creación de registros individuales
- Carga masiva de datos

## Instalación

Para instalar el paquete a través de NuGet:

```bash
dotnet add package ZeroOneClient
```

O usando el administrador de paquetes de Visual Studio:

```
Install-Package ZeroOneClient
```

## Uso básico

### Inicialización del cliente

```csharp
using ZeroOneClient;

// Inicializar el cliente con credenciales
var client = new ZeroOneClient("usuario@ejemplo.com", "contraseña", "id-de-empresa");
```

### Obtener datos de una vista

```csharp
// Consultar datos con paginación
var resultado = await client.ObtenerVista<MiModelo>(
    vistaId: "id-de-la-vista", 
    numeroPagina: 1, 
    tamañoPagina: 10,
    ordenarPorFecha: true
);

// Procesando los resultados
if (resultado != null && resultado.Items.Any())
{
    foreach (var item in resultado.Items)
    {
        Console.WriteLine($"ID: {item.Id}, Nombre: {item.Nombre}");
    }
}
```

### Crear un registro individual

```csharp
// Crear un nuevo objeto
var nuevoRegistro = new MiModelo
{
    Nombre = "Ejemplo",
    Fecha = DateTime.Now,
    // Otras propiedades...
};

// Enviar a ZeroOne
var registroCreado = await client.CrearRegistro<MiModelo>("id-de-la-vista", nuevoRegistro);

if (registroCreado != null)
{
    Console.WriteLine($"Registro creado con ID: {registroCreado.Id}");
}
```

### Carga masiva de datos

```csharp
// Preparar varios registros
var registros = new List<MiModelo>
{
    new MiModelo { Nombre = "Registro 1", /* ... */ },
    new MiModelo { Nombre = "Registro 2", /* ... */ },
    new MiModelo { Nombre = "Registro 3", /* ... */ }
};

// Realizar la carga masiva
var resultado = await client.CargaMasiva<MiModelo>("id-de-la-vista", registros);

if (resultado != null)
{
    Console.WriteLine($"Se cargaron {resultado.Count} registros exitosamente");
}
```

## Modelos de datos

Para el correcto funcionamiento de la librería, es necesario crear modelos de datos que correspondan con la estructura esperada por ZeroOne.

Ejemplo de modelo:

```csharp
using System.Text.Json.Serialization;

public class RegistroHora
{
    [JsonPropertyName("RegistroHoraId")]
    public string RegistroHoraId { get; set; }

    [JsonPropertyName("Usuario")]
    public string Usuario { get; set; }

    [JsonPropertyName("ClienteId")]
    public string ClienteId { get; set; }

    [JsonPropertyName("Cliente")]
    public string Cliente { get; set; }

    [JsonPropertyName("ProyectoId")]
    public string ProyectoId { get; set; }

    [JsonPropertyName("Proyecto")]
    public string Proyecto { get; set; }

    [JsonPropertyName("ActividadId")]
    public string ActividadId { get; set; }

    [JsonPropertyName("Actividad")]
    public string Actividad { get; set; }

    [JsonPropertyName("Fecha")]
    public string Fecha { get; set; }

    [JsonPropertyName("Horas")]
    public string Horas { get; set; }

    [JsonPropertyName("NumeroCasoSolicitud")]
    public string NumeroCasoSolicitud { get; set; }

    [JsonPropertyName("Observacion")]
    public string Observacion { get; set; }
}
```

## Manejo de errores

ZeroOneClient proporciona mensajes de error detallados a través de Console.WriteLine. Para un entorno de producción, se recomienda implementar un sistema de logging más robusto.

Ejemplos de manejo de errores:

```csharp
try
{
    var resultado = await client.ObtenerVista<MiModelo>("id-vista", 1, 10);
    // Procesar resultado...
}
catch (Exception ex)
{
    // Manejar la excepción según las necesidades de la aplicación
    Console.WriteLine($"Error al obtener datos: {ex.Message}");
}
```

## Consideraciones importantes

- El token de autenticación se maneja automáticamente, incluyendo su renovación cuando expira.
- Se recomienda reutilizar la instancia del cliente para optimizar recursos.
- Para operaciones masivas, considere el rendimiento de la API de ZeroOne y ajuste el tamaño de los lotes según sea necesario.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abra un issue para discutir los cambios propuestos antes de realizar un pull request.

## Licencia

[Especificar la licencia] 