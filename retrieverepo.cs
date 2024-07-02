
/*Replace [Your_Client_Secret], YourRepoName, and C:\Path\To\Clone\Repo with your actual client secret, repository name, and local path where you want to clone the repository.
This example uses a placeholder URL (https://dev.azure.com/...) for the clone URL. You need to parse the actual clone URL from the API response.
Ensure Git is installed and accessible from the command line.
The example uses a confidential client flow suitable for applications that can securely store a client secret. The actual implementation of parsing the JSON response to extract the clone URL and handling errors more gracefully 
is left as an exercise.*/


using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var tenantId = ConfigurationManager.AppSettings["aad:Tenant"];
        var clientId = ConfigurationManager.AppSettings["aad:ClientId"];
        var clientSecret = "[Your_Client_Secret]";
        var aadInstance = ConfigurationManager.AppSettings["aad:AADInstance"];
        var organizationUrl = ConfigurationManager.AppSettings["ado:OrganizationUrl"];
        var authority = String.Format(aadInstance, tenantId);
        var repositoryName = "YourRepoName"; // Replace with your repository name
        var localPath = @"C:\Path\To\Clone\Repo"; // Replace with your local path

        var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri(authority))
            .Build();

        var scopes = new string[] { $"{organizationUrl}/.default" };

        try
        {
            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                var url = $"{organizationUrl}/_apis/git/repositories/{repositoryName}?api-version=6.0";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Parse the content to find the repository clone URL
                    // This is a simplified example. You'll need to parse the JSON response correctly.
                    var cloneUrl = "https://dev.azure.com/..."; // Extracted clone URL

                    // Clone the repository
                    CloneRepository(cloneUrl, localPath);
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve repository: {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error acquiring access token or cloning repository: {ex.Message}");
        }
    }

    static void CloneRepository(string cloneUrl, string localPath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("git")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = $"clone {cloneUrl} \"{localPath}\""
        };

        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Repository cloned successfully.");
            }
            else
            {
                Console.WriteLine($"Error cloning repository: {error}");
            }
        }
    }
}
