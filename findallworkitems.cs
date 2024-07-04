using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

class Program
{
    private static readonly string organizationUrl = "https://dev.azure.com/{your_organization}";
    private static readonly string personalAccessToken = "your_personal_access_token";

    static async Task Main(string[] args)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(
                    string.Format("{0}:{1}", "", personalAccessToken))));

            // Get a list of projects
            var projectsResponse = await client.GetAsync($"{organizationUrl}/_apis/projects?api-version=6.0");
            projectsResponse.EnsureSuccessStatusCode();
            var projects = await projectsResponse.Content.ReadAsAsync<dynamic>();

            foreach (var project in projects.value)
            {
                string projectName = project.name;
                Console.WriteLine($"Project: {projectName}");

                // Get a list of work items
                var workItemsResponse = await client.GetAsync($"{organizationUrl}/{projectName}/_apis/wit/workitems?api-version=6.0");
                workItemsResponse.EnsureSuccessStatusCode();
                var workItems = await workItemsResponse.Content.ReadAsAsync<dynamic>();

                foreach (var workItem in workItems.value)
                {
                    Console.WriteLine($"Work Item ID: {workItem.id}, Title: {workItem.fields["System.Title"]}");
                }
            }
        }
    }
}
