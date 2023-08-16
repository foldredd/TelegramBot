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
    internal class Program {
        Week? week;
        private static async Task Main(string[] args) {
            // Створення об'єкта бота з використанням токена доступу
            var botClient = new TelegramBotClient("6182246916:AAHxP6Al3kUIRirI6Tym3j6Q2WUp9g_xWmA");
            // Отримання інформації про бота за допомогою API
            var apiUrl = $"https://api.telegram.org/bot{botClient}/getMe";
            ApiResponse apiResponse;
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                var content = await response.Content.ReadAsStringAsync();
                apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);
            }
            // Перевірка, чи бот може приєднуватися до груп та версії API
            if (apiResponse.Ok && apiResponse.Result.CanJoinGroups)
            {
                Console.WriteLine("Your bot is using an up-to-date version of Telegram API.");
            }
            else
            {
                Console.WriteLine("Your bot is not able to join groups, or it is using an outdated version of Telegram API.");
            }
            // Створення токену скасування для відміни операцій
            using CancellationTokenSource cts = new();

            // Налаштування опцій для отримання оновлень від Telegram
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // отримувати всі типи оновлень
            };
            // Запуск процесу отримання оновлень та обробка їх
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            // Отримання інформації про бота та вивід імені користувача
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            // Очікування вводу з консолі
            Console.ReadLine();

            // Відправка запиту на скасування операцій
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
                // Обробка оновлення типу CallbackQuery
                if (update.CallbackQuery is not null)
                {
                    await OnCallbackQueryHandler(botClient, update.CallbackQuery);
                }
                // Обробка повідомлення
                else if (update.Message is not null)
                {
                    
                    if (update.Message.Text is not { } messageText)
                        return;

                    var chatId = update.Message.Chat.Id;

                    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
                    await Processing.commandMap(messageText, botClient, chatId, cancellationToken, update);
                }
            }
            // Обробка помилок під час отримання оновлень
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
            // Обробка натискання на кнопку "1" у повідомленні
            static async Task OnCallbackQueryHandler(ITelegramBotClient botClient, CallbackQuery callbackQuery) {
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

    class Processing {
        // Обробник команд та повідомлень користувачів
        public static async Task commandMap(string messageText, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, Update update) {
            // Обробка команди /setting
            if (messageText.ToLower().Contains("/setting"))
            {
                // Створення кнопки для відображення
                var button = InlineKeyboardButton.WithCallbackData(text: "Setting", callbackData: "1");
                // Створення клавіатури з кнопкою
                var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });
                // Відправка повідомлення з клавіатурою
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Hello Friend",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
            // Обробка команди /start
            if (messageText.ToLower().Contains("/start"))
            {
                List<Schedule> scheduleList;
                Week week;
                try
                {
                    // Зчитування розкладу з файлу
                    string json = System.IO.File.ReadAllText("timetable.json");
                    TGbotData tgbotData = new TGbotData();
                    tgbotData.botClient = botClient;
                    tgbotData.chatId = chatId;
                    tgbotData.cancellationToken = cancellationToken;
                    scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(json);
                    week = new Week(scheduleList, tgbotData);
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
        // Відправка посилання користувачеві
        public static async Task SentLinktoUser(string[] lectureData, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken) {
            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"{lectureData[0]}={lectureData[1]}",
                   cancellationToken: cancellationToken
               );
        }
    }
    // Структура для зберігання даних про бота
    struct TGbotData {
        public ITelegramBotClient botClient;
        public long chatId;
        public CancellationToken cancellationToken;
    }
    // Клас для зберігання відповіді від API Telegram
    public class ApiResponse {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("result")]
        public User Result { get; set; }
    }
    // Клас для зберігання інформації про користувача
    public class User {
        [JsonProperty("can_join_groups")]
        public bool CanJoinGroups { get; set; }
    }
}