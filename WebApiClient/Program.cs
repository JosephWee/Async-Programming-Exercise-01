using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace WebApiClient
{
    class Program
    {
        private class ResultSubSet
        {
            public Uri RequestUri { get; set; }
            public List<int> Operand { get; set; }
            public HttpResponseMessage ResponseMessage { get; set; }
        }

        static int MinimumNumberOfOperand = 10;

        static void Main(string[] args)
        {
            List<int> operand = new List<int>();
            for (int i = 1; i <= 1000; i++)
            {
                operand.Add(i);
            }

            int? total = Sum(operand);

            if (total.HasValue)
                Console.WriteLine("Total = {0}", total);
            else
                Console.WriteLine("Something went wrong.");

            Console.WriteLine("Press any key to continue...");

            Console.ReadKey();
        }

        private static int? Sum(List<int> operand)
        {
            int? total = null;

            List<Uri> webApiUris =
                new List<Uri>()
                {
                    new Uri("https://localhost:44316/"),
                    new Uri("https://localhost:44360/"),
                    new Uri("https://localhost:44350/")
                };

            List<Task<ResultSubSet>> tasks = new List<Task<ResultSubSet>>();
            List<int> subTotal = new List<int>();
            int operandCountPerApi = operand.Count / webApiUris.Count;
            int index = 0;
            int apiIndex = 0;

            while (index < operand.Count)
            {
                Uri webApiUri = webApiUris[apiIndex++];

                List<int> operandSubSet = new List<int>();

                if (operandCountPerApi > MinimumNumberOfOperand)
                {
                    operandSubSet.AddRange(
                        operand.Skip(index).Take(operandCountPerApi)
                    );
                }
                else
                {
                    operandSubSet.AddRange(operand);
                }

                index += operandSubSet.Count;

                if (operandSubSet.Count > 1)
                    tasks.Add(CallApiSumAsync(webApiUri, operandSubSet));
                else
                    subTotal.AddRange(operandSubSet);

                if (apiIndex >= webApiUris.Count)
                    apiIndex = 0;
            }

            if (tasks.Count > 1)
            {
                Task<ResultSubSet[]> t = Task.WhenAll(tasks);

                try
                {
                    t.Wait();

                    ResultSubSet[] results = t.Result;

                    foreach (ResultSubSet result in results)
                    {
                        if (result.ResponseMessage != null
                            && result.ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Task<int> sum =
                                result.ResponseMessage.Content.ReadFromJsonAsync<int>();

                            subTotal.Add(
                                sum.Result
                            );
                        }
                    }

                    if (subTotal.Count > 1)
                        total = Sum(subTotal);
                    else
                        total = subTotal[0];
                }
                catch
                {
                }
            }
            else
            {
                tasks[0].Wait();
                ResultSubSet result = tasks[0].Result;

                if (result.ResponseMessage != null
                    && result.ResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Task<int> sum =
                        result.ResponseMessage.Content.ReadFromJsonAsync<int>();

                    total = sum.Result;
                }
            }

            return total;
        }

        private static async Task<ResultSubSet> CallApiSumAsync(Uri ApiUri, List<int> OperandSubSet)
        {
            ResultSubSet result =
                new ResultSubSet()
                {
                    RequestUri = null,
                    Operand = OperandSubSet,
                    ResponseMessage = null
                };

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string apiAbsoluteUri = ApiUri.AbsoluteUri.EndsWith("/") ? ApiUri.AbsoluteUri.TrimEnd('/') : ApiUri.AbsoluteUri;

                result.RequestUri =
                    new Uri($"{apiAbsoluteUri}/api/Math/Sum");

                MathWebApi.SumRequestModel requestModel = new MathWebApi.SumRequestModel()
                {
                    operand = OperandSubSet.ToArray()
                };

                try
                {
                    result.ResponseMessage =
                        await httpClient.PostAsJsonAsync(result.RequestUri.AbsoluteUri, requestModel);
                }
                catch (Exception ex)
                {
                }
            }

            return result;
        }
    }
}
