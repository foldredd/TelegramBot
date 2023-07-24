using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading;

namespace TelegramBot {
    internal class Program
    {
        Week? week;
        private static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient("6182246916:AAHxP6Al3kUIRirI6Tym3j6Q2WUp9g_xWmA");
            var apiUrl = $"https://api.telegram.org/bot{botClient}/getMe";
            ApiResponse apiResponse;
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();
                 apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
            }
            if (apiResponse.Ok && apiResponse.Result.CanJoinGroups)
            {
                Console.WriteLine("Your bot is using an up-to-date version of Telegram API.");
            }
            else
            {
                Console.WriteLine("Your bot is not able to join groups, or it is using an outdated version of Telegram API.");
            }

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.CallbackQuery is not null)
                {
                    await OnCallbackQueryHandler(botClient, update.CallbackQuery);
                }
                else if (update.Message is not null)
                {
                    // Only process text messages
                    if (update.Message.Text is not { } messageText)
                        return;

                    var chatId = update.Message.Chat.Id;

                    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
                    await Processing.commandMap(messageText, botClient, chatId, cancellationToken,update);
                }
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }

            static async Task OnCallbackQueryHandler(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                if (callbackQuery.Data == "1")
                {
                    await botClient.EditMessageTextAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        messageId: callbackQuery.Message.MessageId,
                        text: "+"
                    );
                }
            }
        }
    }

    class Processing
    {
        public static async Task commandMap(string messageText, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken,Update update)
        {
            if (messageText.ToLower().Contains("/setting"))
            {
                var button = InlineKeyboardButton.WithCallbackData(text: "Setting", callbackData: "1");

                var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Hello Friend",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
            if (messageText.ToLower().Contains("/start"))
            {
                List<Schedule> scheduleList;
                Week week;
                try
                {
                    string json = System.IO.File.ReadAllText("timetable.json");
                    TGbotData tgbotData = new TGbotData();
                    tgbotData.botClient = botClient;
                    tgbotData.chatId = chatId;
                    tgbotData.cancellationToken = cancellationToken;
                    scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(json);
                    week = new Week(scheduleList,tgbotData);
                    await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "bot start",
                   cancellationToken: cancellationToken
               );
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(
                  chatId: chatId,
                  text: "Error:(",
                  cancellationToken: cancellationToken
                  );

                   Console.WriteLine(ex.ToString());
                }
            }
        }
        public static async Task SentLinktoUser(string[] lectureData ,ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"{lectureData[0]}={lectureData[1]}",
                   cancellationToken: cancellationToken
               );
        }
    }
    struct TGbotData
    {
       public ITelegramBotClient botClient;
        public long chatId;
        public CancellationToken cancellationToken;
    }
    public class ApiResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("result")]
        public User Result { get; set; }
    }

    public class User
    {
        [JsonProperty("can_join_groups")]
        public bool CanJoinGroups { get; set; }
    }
}


