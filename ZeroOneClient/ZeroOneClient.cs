using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using static ZeroOneClient.DTOs.ZeroOneModels;
using System.Collections.ObjectModel;

namespace ZeroOneClient;

/// <summary>
/// Cliente para interactuar con la API de ZeroOne  
/// </summary>
public class ZeroOneClient
{
    private readonly Uri _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _password;
    private readonly string _companyId;
    private string? _accessToken = null;

    /// <summary>
    /// Constructor de la clase ZeroOneClient   
    /// </summary>
    /// <param name="username">Nombre de usuario de ZeroOne</param>
    /// <param name="password">Contraseña</param>
    /// <param name="companyId">ID de la empresa</param>
    public ZeroOneClient(string username, string password, string companyId)
    {
        _baseUrl = new Uri("https://api.zeroone.la/api/");
        _username = username;
        _password = password;
        _companyId = companyId;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Obtiene los datos de una vista específica con paginación y ordenamiento
    /// </summary>
    /// <typeparam name="T">Tipo de datos a deserializar</typeparam>
    /// <param name="vistaId">ID de la vista a consultar</param>
    /// <param name="numeroPagina">Número de página actual</param>
    /// <param name="tamañoPagina">Cantidad de registros por página</param>
    /// <param name="ordenarPorFecha">Indica si se debe ordenar por fecha</param>
    /// <param name="ordenarPorProyecto">Indica si se debe ordenar por proyecto</param>
    /// <param name="ordenarPorUsuario">Indica si se debe ordenar por usuario</param>
    /// <returns>Colección de elementos del tipo especificado</returns>
    public async Task<VistaResponse<T>?> ObtenerVista<T>(string vistaId, int numeroPagina, int tamañoPagina, bool ordenarPorFecha = false, bool ordenarPorProyecto = false, bool ordenarPorUsuario = false)
    {
        try
        {
            if (!IsAuthenticated()) await Login();

            var uriBuilder = new UriBuilder(_baseUrl);
            uriBuilder.Path = Path.Combine(uriBuilder.Path, "vistas", vistaId, "datos").Replace("\\", "/");
            
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["pageNumber"] = numeroPagina.ToString();
            query["pageSize"] = tamañoPagina.ToString();
            query["O_Fecha"] = ordenarPorFecha.ToString().ToLower();
            query["O_Proyecto"] = ordenarPorProyecto.ToString().ToLower();
            query["O_Usuario"] = ordenarPorUsuario.ToString().ToLower();
            
            uriBuilder.Query = query.ToString();

            HttpResponseMessage respuesta = await _httpClient.GetAsync(uriBuilder.Uri);
            string contenidoRespuesta = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error realizando consulta a ZeroOne. Código de respuesta {respuesta.StatusCode}. {contenidoRespuesta}");
                return null;
            }

            VistaResponse<T>? vistaResponse = JsonConvert.DeserializeObject<VistaResponse<T>>(contenidoRespuesta);

            if (vistaResponse?.Items == null) 
            {
                Console.WriteLine("La petición se completó correctamente pero el deserializado resultó null");
                return null;
            }

            return vistaResponse;
        }
        catch (Exception e){
            Console.WriteLine($"Ocurrió un error obteniendo la información de ZeroOne. {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Crea un registro en ZeroOne para una vista específica
    /// </summary>
    /// <typeparam name="T">Tipo de datos a crear (lo que va dentro de Dato en la petición a ZeroOne)</typeparam>
    /// <param name="vistaId">ID de la vista</param>
    /// <param name="dato">Registro a crear</param>
    /// <returns>Datos creados</returns>
    public async Task<T?> CrearRegistro<T>(string vistaId, T dato)
    {
        if (!IsAuthenticated()) await Login();

        var uriBuilder = new UriBuilder(_baseUrl);
        
        uriBuilder.Path = Path.Combine(uriBuilder.Path, "vistas", vistaId).Replace("\\", "/");

        Registro<T> registro = new() { Dato = dato, Parametros = new Dictionary<string, object>() };

        string serializedRequest = JsonConvert.SerializeObject(registro);

        StringContent requestContent = new(serializedRequest, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(uriBuilder.Uri, requestContent);

        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) 
        {
            Console.WriteLine($"Error creando registro en ZeroOne. Código de respuesta {response.StatusCode}. {responseContent}");
            return default!;
        }

        List<T>? creado = JsonConvert.DeserializeObject<List<T>>(responseContent);

        if (creado == null)
        {
            Console.WriteLine("El deserializado resultó null");
            return default!;
        }

        return creado.First();
    }   

    /// <summary>
    /// Realiza una carga masiva de datos en ZeroOne para una vista específica
    /// </summary>
    /// <typeparam name="T">Tipo de datos a crear (lo que va dentro de datos en la petición a ZeroOne)</typeparam>
    /// <param name="vistaId">ID de la vista</param>
    /// <param name="datos">Lista de datos a crear</param>
    /// <returns>Colección de datos creados</returns>
    public async Task<Collection<T>?> CargaMasiva<T>(string vistaId, List<T> datos) 
    {
        if (!IsAuthenticated()) await Login();

        UriBuilder uriBuilder = new(_baseUrl);
        uriBuilder.Path = Path.Combine(uriBuilder.Path, "vistas", vistaId, "carga-masiva-datos").Replace("\\", "/");

        object requestObject = new { datos = datos };
        StringContent requestContent = new(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await _httpClient.PostAsync(uriBuilder.Uri, requestContent);

        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error realizando carga masiva en ZeroOne. Código de respuesta {response.StatusCode}. {responseContent}");
            return null;
        }

        return JsonConvert.DeserializeObject<Collection<T>>(responseContent);
    }

    /// <summary>
    /// Realiza el login en ZeroOne y guarda el token de acceso en el header de la petición 
    /// </summary>
    /// <returns>Token de acceso</returns>
    /// <exception cref="Exception">Excepción que ocurre si el login falla</exception>
    private async Task Login()
    {
        Uri baseUrl = new(_baseUrl, "auth/login");

        LoginRequest request = new() { Email = _username, Password = _password, CompanyId = _companyId };

        string serializedRequest = JsonConvert.SerializeObject(request);

        StringContent requestContent = new(serializedRequest, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(baseUrl, requestContent);

        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error realizando login. {responseContent}");
        }

        LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent)
            ?? throw new Exception("Error while trying to log in");

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginResponse.Token}");

        _accessToken = loginResponse.Token;
    }

    /// <summary>
    /// Verifica si el token de acceso está expirado o si no existe
    /// </summary>
    /// <returns>True si el token está expirado, false en caso contrario</returns>
    private bool IsAuthenticated()
    {
        if (_accessToken == null) return false;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(_accessToken);
            
            if (!token.Claims.Any(c => c.Type == "exp")) return false;

            var expClaim = token.Claims.First(c => c.Type == "exp").Value;
            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;

            return DateTime.UtcNow.AddSeconds(30) < expirationDate;
        }
        catch
        {
            return false;
        }
    }
}

