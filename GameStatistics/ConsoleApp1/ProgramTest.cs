using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Newtonsoft.Json;



public class GameDataObj
{
    public string id { get; set; }
    public string gameName { get; set; }
    public string category { get; set; }
    public int totalBets { get; set; }
    public int totalWins { get; set; }
    public float averageBetAmount { get; set; }
    public float popularityScore { get; set; }
    public DateTime lastUpdated { get; set; }


    public override string ToString()
    {
        return "id: "+this.id + " name: " + this.gameName;
    }

}
            

class ProgramTest
{
    public static int numErrors;
    public static int numSuccess;
    public static int numTotalExecutions;

    public const int GET_ALL_ACTION = 0;
    public const int GET_BY_ID_ACTION = 1;
    public const int PUT_ACTION = 2;
    public const int POST_ACTION = 3;
    public const int DELETE_ACTION = 4;

    static async Task Main(string[] args)
    {

        // Configura la URL de la API que deseas probar
        string apiUrl = "https://localhost:7170/GameData";

        //SMOKE TEST
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("iniciando SMOKE TEST");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        int numberOfRequests = 2;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        //AVERAGE LOAD TEST
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("iniciando AVERAGE LOAD TEST");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        numberOfRequests = 20;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        //SPIKE TEST
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("iniciando SPIKE TEST");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        numberOfRequests = 350;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("ACABADOS LOS TESTS");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
    }

    static async Task stressTest(string apiUrl, int numberOfRequests)
    {
        
        for (int action = 0; action <= DELETE_ACTION; action++)
        {
            switch (action)
            {
                case GET_ALL_ACTION:
                    Console.WriteLine("-------------------------------\niniciando pruebas de GET_ALL");
                    break;
                case GET_BY_ID_ACTION:
                    Console.WriteLine("-------------------------------\niniciando pruebas de GET_BY_ID");
                    break;
                case PUT_ACTION:
                    Console.WriteLine("-------------------------------\niniciando pruebas de PUT");
                    break;
                case POST_ACTION:
                    Console.WriteLine("-------------------------------\niniciando pruebas de POST");
                    break;
                case DELETE_ACTION:
                    Console.WriteLine("-------------------------------\niniciando pruebas de DELETE");
                    break;
                default:
                    Console.WriteLine("-------------------------------\niniciando pruebas de GET_ALL");
                    break;
            }
            
            // Inicia el cronómetro
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Realiza las solicitudes en paralelo
            await RunParallelRequests(apiUrl, numberOfRequests, action);

            // Detiene el cronómetro y muestra el tiempo transcurrido
            stopwatch.Stop();
            Console.WriteLine($"Pruebas de estrés completadas en {stopwatch.Elapsed.TotalSeconds} segundos.");

        }

        Console.WriteLine("-------------------------------\nTest finalizado, pulsa cualquier tecla para continuar");
    }


    static async Task RunParallelRequests(string apiUrl, int numberOfRequests, int action, string id="")
    {
        // Lista para almacenar las tareas de solicitud
        var requestTasks = new List<Task>();

        var application = new WebApplicationFactory<Program>();

        // Configura el cliente HTTP
        using (var httpClient = application.CreateClient())
        {

            // Inicia las tareas de solicitud
            for (int i = 0; i < numberOfRequests; i++)
            {
                
                // Agrega una tarea para cada solicitud
                var requestTask = GetAllRequest(httpClient, apiUrl);

                switch (action)
                {
                    case GET_ALL_ACTION:
                        requestTask = GetAllRequest(httpClient, apiUrl);
                        break;
                    case GET_BY_ID_ACTION:
                        requestTask = GetByIdRequest(httpClient, apiUrl, id);
                        break;
                    case PUT_ACTION:
                        requestTask = PutRequest(httpClient, apiUrl, id);
                        break;
                    case POST_ACTION:
                        requestTask = PostRequest(httpClient, apiUrl);
                        break;
                    case DELETE_ACTION:
                        requestTask = DeleteRequest(httpClient, apiUrl);
                        break;
                    default:
                        requestTask = GetAllRequest(httpClient, apiUrl);
                        break;
                }

                requestTasks.Add(requestTask);
                
            }
            
            
            // Espera a que todas las tareas se completen
            await Task.WhenAll(requestTasks);
            }

        Console.WriteLine("numSuccess: "+numSuccess);
        Console.WriteLine("numErrors: " + numErrors);
        Console.WriteLine("total: " + numTotalExecutions);
        numSuccess= 0;
        numErrors= 0;
        numTotalExecutions= 0;

    }

    static async Task PostRequest(HttpClient httpClient, string apiUrl)
    {
        try
        {
            
            var obj = new
            {
                GameName = "testName",
                Category = "someCategory",
                TotalBets = 1,
                TotalWins = 1,
                AverageBetAmount = 1,
                PopularityScore = 1,
                LastUpdated = new DateTime()
            };

            JsonContent content = JsonContent.Create(obj);

            var response = await httpClient.PostAsync(apiUrl, content);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            //Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
        numTotalExecutions++;
    }

    static async Task GetAllRequest(HttpClient httpClient, string apiUrl)
    {
        try
        {
            var response = await httpClient.GetAsync(apiUrl);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            //Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
        numTotalExecutions++;
    }

    static async Task GetByIdRequest(HttpClient httpClient, string apiUrl, string id)
    {
        try
        {
            var response = await httpClient.GetAsync(apiUrl+"/"+id);
            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            //Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
        numTotalExecutions++;
    }


    static async Task PutRequest(HttpClient httpClient, string apiUrl, string id)
    {
        try
        {

            var obj = new
            {
                Id = id,
                GameName = "testUpdatedName",
                Category = "someUpdatedCategory",
                TotalBets = 1,
                TotalWins = 1,
                AverageBetAmount = 1,
                PopularityScore = 1,
                LastUpdated = new DateTime()
            };

            JsonContent content = JsonContent.Create(obj);

            var response = await httpClient.PutAsync(apiUrl + "/" + id, content);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            //Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
        numTotalExecutions++;
    }

    static async Task DeleteRequest(HttpClient httpClient, string apiUrl)
    {
        try
        {
            //obtener todos los objetos
            var preResponse = await httpClient.GetAsync(apiUrl);
            string responseBody = await preResponse.Content.ReadAsStringAsync();

            List<GameDataObj> listaObjetos = JsonConvert.DeserializeObject<List<GameDataObj>>(responseBody);

            // Obtener el primer objeto de la lista
            GameDataObj primerObjeto = listaObjetos.FirstOrDefault();

            var response = await httpClient.DeleteAsync(apiUrl + "/" + primerObjeto.id);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            //Console.WriteLine($"Error en la solicitud: {ex.Message}");
        }
        numTotalExecutions++;
    }

}

