using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;

namespace FITON.Server.Services
{
 public class VertexImageGenerator : IImageGenerator
 {
 private readonly IConfiguration _config;
 public VertexImageGenerator(IConfiguration config) => _config = config;

 public async Task<string> GenerateImageAsync(string prompt)
 {
 var projectId = _config["GoogleCloud:ProjectId"];
 var location = _config["GoogleCloud:Location"];
 var modelId = "imagegeneration@006";
 if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(location))
 {
 throw new System.InvalidOperationException("Google Cloud is not configured");
 }
 var endpointName = EndpointName.FromProjectLocationPublisherModel(projectId, location, "google", modelId);
 var predictionServiceClient = new PredictionServiceClientBuilder { Endpoint = $"{location}-aiplatform.googleapis.com" }.Build();
 var parameters = Google.Protobuf.WellKnownTypes.Value.ForStruct(new Google.Protobuf.WellKnownTypes.Struct
 {
 Fields =
 {
 { "sampleCount", Google.Protobuf.WellKnownTypes.Value.ForNumber(1) },
 { "aspectRatio", Google.Protobuf.WellKnownTypes.Value.ForString("9:16") },
 { "safetySetting", Google.Protobuf.WellKnownTypes.Value.ForString("block_some") },
 { "personGeneration", Google.Protobuf.WellKnownTypes.Value.ForString("allow_adult") }
 }
 });
 var instance = Google.Protobuf.WellKnownTypes.Value.ForStruct(new Google.Protobuf.WellKnownTypes.Struct
 {
 Fields = { { "prompt", Google.Protobuf.WellKnownTypes.Value.ForString(prompt) } }
 });
 var response = await predictionServiceClient.PredictAsync(endpointName, new[] { instance }, parameters);
 if (response.Predictions.Count ==0) throw new System.InvalidOperationException("No image generated");
 var prediction = response.Predictions[0];
 var base64 = prediction.StructValue.Fields["bytesBase64Encoded"].StringValue;
 return $"data:image/png;base64,{base64}";
 }
 }
}