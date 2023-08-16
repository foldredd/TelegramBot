using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot {
    class Week {
        Timer timer;
        Timer timerMonday;
        List<DateTime> dateForLecture;
        List<Lecture> lectureList;
        public List<Schedule> scheduleList;
        TGbotData tgbotData;
        // Конструктор класу Week
        public Week(List<Schedule> scheduleList, TGbotData tgbotData) {
            timer = new Timer(new TimerCallback(SendLinks), null, 0, Timeout.Infinite);
            this.scheduleList = scheduleList;
            this.tgbotData = tgbotData;
        }
        // Відправка посилань
        public void SendLinks(object state) {
            DateTime now = DateTime.Now;
            // Визначення парності тижня
            if (CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) % 2 == 0)
            {
                Console.WriteLine("Текущая неделя четная.");
            }
            else
            {
                Console.WriteLine("Текущая неделя нечетная.");
            }
            DateTime date = DateTime.Now;
            DayOfWeek dayOfWeek = date.DayOfWeek;
            dateForLecture = new List<DateTime>();
            lectureList = new List<Lecture>();
            Lecture lecture = new Lecture();
            // Визначення дій в залежності від дня тижня
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Monday:
                    lectureList = new List<Lecture>();

                    foreach (Schedule schedule in scheduleList)
                    {
                        lecture.pair = Int32.Parse(schedule.Pair);
                        lecture.lesson = schedule.Monday;
                        lecture.link = schedule.Links.MondayLink;
                        lectureList.Add(lecture);
                    }
                    ValueofTimelecture();
                    CheckTime();
                    break;
                case System.DayOfWeek.Tuesday:
                    foreach (Schedule schedule in scheduleList)
                    {
                        lecture.pair = Int32.Parse(schedule.Pair);
                        lecture.lesson = schedule.Tuesday;
                        lecture.link = schedule.Links.TusdayLink;
                        lectureList.Add(lecture);
                    }
                    ValueofTimelecture();
                    CheckTime();
                    break;
                case System.DayOfWeek.Wednesday:

                    foreach (Schedule schedule in scheduleList)
                    {
                        lecture.pair = Int32.Parse(schedule.Pair);
                        lecture.lesson = schedule.Wednesday;
                        lecture.link = schedule.Links.WednesdayLink;
                        lectureList.Add(lecture);
                    }
                    ValueofTimelecture();
                    CheckTime();
                    break;
                case System.DayOfWeek.Thursday:
                    foreach (Schedule schedule in scheduleList)
                    {
                        lecture.pair = Int32.Parse(schedule.Pair);
                        lecture.lesson = schedule.Thursday;
                        lecture.link = schedule.Links.ThursdayLink;
                        lectureList.Add(lecture);
                    }
                    ValueofTimelecture();
                    CheckTime();
                    break;
                case System.DayOfWeek.Friday:
                    foreach (Schedule schedule in scheduleList)
                    {
                        lecture.pair = Int32.Parse(schedule.Pair);
                        lecture.lesson = schedule.Friday;
                        lecture.link = schedule.Links.FridayLink;
                        lectureList.Add(lecture);
                    }
                    ValueofTimelecture();
                    CheckTime();
                    break;
                case System.DayOfWeek.Saturday:

                    break;
                case System.DayOfWeek.Sunday:

                    break;
            }

            // Установка таймера на наступний день
            DateTime Today = DateTime.Now;
            DateTime startOfDay = Today.Date;
            long ticksAtStartOfDay = startOfDay.Ticks; // Ticks для початку дня
            DateTime tomorrow = Today.AddDays(1).Date; // Початок дня наступного дня
            TimeSpan timeUntilTomorrow = new TimeSpan(tomorrow.Ticks - ticksAtStartOfDay);

            timer = new Timer(new TimerCallback(SendLinks), null, timeUntilTomorrow, Timeout.InfiniteTimeSpan);
        }
        // Встановлення часу лекцій
        public void ValueofTimelecture() {
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 8, minute: 20, second: 0));
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 10, minute: 00, second: 00));
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 11, minute: 50, second: 0));
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 13, minute: 30, second: 0));
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 15, minute: 10, second: 0));
            dateForLecture.Add(new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day,
                hour: 16, minute: 50, second: 0));
        }
        // Перевірка часу
        public async void CheckTime() {
            DateTime datenow = DateTime.Now;
            DateTime rangeDateX = new DateTime(year: datenow.Year, month: datenow.Month, day: datenow.Day,
            hour: 8, minute: 20, second: 0);
            DateTime rangeDateY = new DateTime(year: datenow.Year, month: datenow.Month, day: datenow.Day,
            hour: 16, minute: 50, second: 0);

            if (datenow >= rangeDateX && datenow <= rangeDateY)
            {
                // Встановлення таймера для наступної лекції
                timerMonday = new Timer(new TimerCallback(SetlinkTimer), null, 0, Timeout.Infinite);
            }
            else
            {
                TimeSpan timeUntilNextRangeStart = (rangeDateX > datenow) ? rangeDateX - datenow : rangeDateX.AddDays(1) - datenow;
                Console.WriteLine($"Waiting {timeUntilNextRangeStart.TotalMinutes} minutes until next range start...");
                timerMonday = new Timer(new TimerCallback(state => CheckTime()), null, timeUntilNextRangeStart, Timeout.InfiniteTimeSpan);
            }
        }
        // Встановлення таймера для надсилання посилань на лекції
        public async void SetlinkTimer(object state) {
            DateTime dateReal = DateTime.Now;
            int indexlecture = 0;
            Lecture lecture;
            string[] valueLecture = new string[2];
            DateTime time = DateTime.MinValue;
            foreach (DateTime dateforlecture in dateForLecture)
            {
                time = dateforlecture;
                if (dateReal.Hour == dateforlecture.Hour && dateReal.Minute == dateforlecture.Minute)
                {
                    lecture = lectureList[indexlecture];
                    indexlecture++;
                    if (lecture.lesson != "-" || lecture.lesson != null)
                    {
                        valueLecture[0] = lecture.lesson;
                        valueLecture[1] = lecture.link;

                        await Processing.SentLinktoUser(valueLecture, tgbotData.botClient, tgbotData.chatId, tgbotData.cancellationToken);
                    }

                }
                else if (dateReal > dateforlecture)
                {
                    indexlecture++;
                }
                else if (dateReal < dateforlecture)
                {
                    break;
                }

            }


            if (time != DateTime.MinValue)
            {
                Console.WriteLine(time);
                long ticksAtStartOfDay = time.Ticks;
                TimeSpan timeForNextLecture = new TimeSpan(ticksAtStartOfDay - DateTime.Now.Ticks);

                timerMonday = new Timer(new TimerCallback(SetlinkTimer), null, timeForNextLecture, Timeout.InfiniteTimeSpan);
            }
        }
    }


    struct Lecture {
        public int pair; // Номер пари
        public string lesson; // Назва лекції
        public string link; // Посилання на лекцію
    }
}