//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Playground
//{
//    public class DeepSeekReasonerService
//    {
//        private readonly ChatEndpoint _chatEndpoint;

//        public DeepSeekReasonerService(string apiKey)
//        {
//            var client = new OpenAIClient(apiKey, new OpenAIClientSettings(
//                baseUrl: "https://api.deepseek.com/v1"
//            ));
//            _chatEndpoint = new ChatEndpoint(client);
//        }

//        public async Task<string> GetReasonedResponseAsync(string userMessage)
//        {
//            var messages = new ChatMessage[]
//            {
//            new ChatMessage(ChatMessageRole.User, userMessage)
//            };

//            var chatRequest = new ChatRequest(
//                messages: messages,
//                model: "deepseek-reasoner",
//                maxTokens: 4000,
//                temperature: 0.7,
//                stream: false
//            );

//            var response = await _chatEndpoint.GetCompletionAsync(chatRequest);
//            var result = response.FirstChoice.Message;

//            // 输出思考过程（如果有）
//            if (!string.IsNullOrEmpty(result.ReasoningContent))
//            {
//                Console.WriteLine("=== 思考过程 ===");
//                Console.WriteLine(result.ReasoningContent);
//                Console.WriteLine("=== 最终回答 ===");
//            }

//            return result.Content;
//        }
//    }
//}
