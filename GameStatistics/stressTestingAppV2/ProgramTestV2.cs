using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;



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
        return "id: " + this.id + " name: " + this.gameName;
    }

}


class ProgramTestV2
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
        int numberOfRequests = 100;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        //AVERAGE LOAD TEST
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("iniciando AVERAGE LOAD TEST");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        numberOfRequests = 1000;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        //SPIKE TEST
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("iniciando SPIKE TEST");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        numberOfRequests = 2000;
        stressTest(apiUrl, numberOfRequests);
        Console.ReadLine();
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
        Console.WriteLine("ACABADOS LOS TESTS");
        Console.WriteLine("-------------------------------\n-------------------------------\n-------------------------------\n");
    }

    static async Task stressTest(string apiUrl, int numberOfRequests)
    {

        // Inicia el cronómetro
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Realiza las solicitudes en paralelo
        await RunParallelRequests(apiUrl, numberOfRequests);

        // Detiene el cronómetro y muestra el tiempo transcurrido
        stopwatch.Stop();
        Console.WriteLine($"Pruebas de estrés completadas en {stopwatch.Elapsed.TotalSeconds} segundos.");

        Console.WriteLine("-------------------------------\nTest finalizado, pulsa cualquier tecla para continuar");
    }


    static async Task RunParallelRequests(string apiUrl, int numberOfRequests)
    {
        // Lista para almacenar las tareas de solicitud
        var requestTasks = new List<Task>();

        var application = new WebApplicationFactory<Program>();
        for (int i = 0; i < numberOfRequests; i++)
        {
            
            var requestTask = createUser(apiUrl, numberOfRequests, application);
            requestTasks.Add(requestTask);

            //createUser(apiUrl, numberOfRequests, application);

        }

        // Espera a que todas las tareas se completen
        await Task.WhenAll(requestTasks);

        Console.WriteLine("numSuccess: " + numSuccess);
        Console.WriteLine("numErrors: " + numErrors);
        Console.WriteLine("total: " + numTotalExecutions);
        numSuccess = 0;
        numErrors = 0;
        numTotalExecutions = 0;
        
    }

    static async Task createUser(string apiUrl, int numberOfRequests, WebApplicationFactory<Program> application)
    {
        // Configura el cliente HTTP
        using (var httpClient = application.CreateClient())
        {
            //Console.WriteLine("conexion de nuevo usuario");

            //accion del usuario
            Random random = new Random();
            int action = random.Next(0, 4);

            //tiempo en el que el usuario tarda en hacer cosas 
            await Task.Delay(random.Next(10, 50));

            switch (action)
            {
                case 0:
                    //Console.WriteLine("el usuario hace un GET_ALL");

                    var requestTask = GetAllRequest(httpClient, apiUrl);
                    //tiempo en el que el usuario tarda en hacer cosas (guardar los datos, por ejemplo)
                    await Task.Delay(random.Next(50, 100));

                    break;
                case 1:
                    //Console.WriteLine("el usuario hace un POST, seguido de un GET_BY_ID");

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

                        //postear el objeto
                        var preResponse = await httpClient.PostAsync(apiUrl, content);
                        await Task.Delay(random.Next(10, 50));

                        //obtener la ID del objeto posteado
                        string responseBody = await preResponse.Content.ReadAsStringAsync();
                        GameDataObj Objeto = JsonConvert.DeserializeObject<GameDataObj>(responseBody);

                        //getbyId del objeto
                        requestTask = GetByIdRequest(httpClient, apiUrl, Objeto.id);
                        await Task.Delay(random.Next(10, 50));

                        //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
                        numSuccess++;
                    }
                    catch (Exception ex)
                    {
                        numErrors++;
                        Console.WriteLine($"Error en la solicitud: {ex.Message}");
                        Console.WriteLine("Durante la ejecucion de Post -> Get");
                    }
                    numTotalExecutions++;




                    break;
                case 2:
                    //Console.WriteLine("el usuario hace un PUT, seguido de un GET_BY_ID");

                    try
                    {

                        var obj = new
                        {
                            GameName = "testNameUPDATED",
                            Category = "someCategoryUPDATED",
                            TotalBets = 1,
                            TotalWins = 1,
                            AverageBetAmount = 1,
                            PopularityScore = 1,
                            LastUpdated = new DateTime()
                        };

                        JsonContent content = JsonContent.Create(obj);


                        //obtener id del objeto a actualizar
                        var preResponse = await httpClient.GetAsync(apiUrl);
                        string responseBody = await preResponse.Content.ReadAsStringAsync();

                        List<GameDataObj> listaObjetos = JsonConvert.DeserializeObject<List<GameDataObj>>(responseBody);

                        //cogemos uno aleatorio
                        GameDataObj objetoRandom = listaObjetos[random.Next(0, listaObjetos.Count)];
                        await Task.Delay(random.Next(10, 50));

                        //actualizar el objeto
                        var preResponse2 = await httpClient.PutAsync(apiUrl + "/" + objetoRandom.id, content);
                        await Task.Delay(random.Next(10, 50));

                        //getbyId del objeto
                        requestTask = GetByIdRequest(httpClient, apiUrl, objetoRandom.id);
                        await Task.Delay(random.Next(10, 50));

                        //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
                        numSuccess++;
                    }
                    catch (Exception ex)
                    {
                        numErrors++;
                        Console.WriteLine($"Error en la solicitud: {ex.Message}");
                        Console.WriteLine("Durante la ejecucion de Put -> Get");
                    }
                    numTotalExecutions++;

                    break;
                case 3:
                    //Console.WriteLine("el usuario hace un POST, seguido de un DELETE");

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

                        //postear el objeto
                        var preResponse = await httpClient.PostAsync(apiUrl, content);
                        await Task.Delay(random.Next(10, 50));

                        //obtener la ID del objeto posteado
                        string responseBody = await preResponse.Content.ReadAsStringAsync();
                        GameDataObj Objeto = JsonConvert.DeserializeObject<GameDataObj>(responseBody);

                        //delete del objeto
                        requestTask = DeleteRequest(httpClient, apiUrl, Objeto.id);
                        await Task.Delay(random.Next(10, 50));

                        //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
                        numSuccess++;
                    }
                    catch (Exception ex)
                    {
                        numErrors++;
                        Console.WriteLine($"Error en la solicitud: {ex.Message}");
                        Console.WriteLine("Durante la ejecucion de Post -> Delete");
                    }
                    numTotalExecutions++;




                    break;
                default:
                    requestTask = GetAllRequest(httpClient, apiUrl);
                    break;
            }


            /*
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
            */

        }

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

            var response = httpClient.PostAsync(apiUrl, content);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
            Console.WriteLine("Durante la ejecucion de Post");
        }
        numTotalExecutions++;
    }

    static async Task GetAllRequest(HttpClient httpClient, string apiUrl)
    {
        try
        {
            var response = httpClient.GetAsync(apiUrl);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
            Console.WriteLine("Durante la ejecucion de GetAll");
        }
        numTotalExecutions++;
    }

    static async Task GetByIdRequest(HttpClient httpClient, string apiUrl, string id)
    {
        try
        {
            var response = httpClient.GetAsync(apiUrl + "/" + id);
            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            Console.WriteLine($"Error en la solicitud: {ex.Message}");

            Console.WriteLine("Durante la ejecucion de Get");
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

            var response = httpClient.PutAsync(apiUrl + "/" + id, content);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
            Console.WriteLine("Durante la ejecucion de Put");
        }
        numTotalExecutions++;
    }

    static async Task DeleteRequest(HttpClient httpClient, string apiUrl, string id)
    {
        try
        {
            var response = httpClient.DeleteAsync(apiUrl + "/" + id);

            //Console.WriteLine($"Solicitud completada con estado: {response.StatusCode}");
            numSuccess++;
        }
        catch (Exception ex)
        {
            numErrors++;
            Console.WriteLine($"Error en la solicitud: {ex.Message}");
            Console.WriteLine("Durante la ejecucion de Delete");
        }
        numTotalExecutions++;
    }

}


